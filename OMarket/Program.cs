using System.Reflection;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using NLog.Web;

using OMarket.Application.Middlewares;
using OMarket.Application.Services.Admin;
using OMarket.Application.Services.Bot;
using OMarket.Application.Services.Cache;
using OMarket.Application.Services.Cart;
using OMarket.Application.Services.Distributor;
using OMarket.Application.Services.HealthCheck;
using OMarket.Application.Services.Jwt;
using OMarket.Application.Services.KeyboardMarkup;
using OMarket.Application.Services.Migration;
using OMarket.Application.Services.Password;
using OMarket.Application.Services.Processor;
using OMarket.Application.Services.SendResponse;
using OMarket.Application.Services.StaticCollections;
using OMarket.Application.Services.TgUpdate;
using OMarket.Application.Services.Translator;
using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.Interfaces.Application.Services.Admin;
using OMarket.Domain.Interfaces.Application.Services.Bot;
using OMarket.Domain.Interfaces.Application.Services.Cache;
using OMarket.Domain.Interfaces.Application.Services.Cart;
using OMarket.Domain.Interfaces.Application.Services.Distributor;
using OMarket.Domain.Interfaces.Application.Services.Jwt;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Password;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Domain.Mapper;
using OMarket.Domain.Settings;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;
using OMarket.Infrastructure.Repositories;

using StackExchange.Redis;

using Telegram.Bot;

namespace OMarket
{
    public class Program
    {
        public static DateTime StartupTime { get; private set; }

        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            #region Base
            builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Error)
                .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);

            builder.Services.AddControllers()
                .AddNewtonsoftJson();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen();

            builder.Services.AddMemoryCache();

            builder.Services.AddHealthChecks();

            builder.WebHost.UseKestrel(options =>
            {
                options.AddServerHeader = false;
            });

            var appUri = Environment.GetEnvironmentVariable("HTTPS_APPLICATION_URL");

            if (string.IsNullOrEmpty(appUri))
            {
                throw new ArgumentNullException("HTTPS_APPLICATION_URL", "The 'HTTPS_APPLICATION_URL' string environment variable is not set.");
            }

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", builder =>
                builder.WithOrigins(appUri)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"/app/.aspnet/DataProtection-Keys"))
                .SetApplicationName("OMarket")
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256,
                });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
            });
            #endregion

            #region Logger
            builder.Logging.ClearProviders();
            builder.Host.UseNLog();
            #endregion

            #region Settings
            builder.Services.Configure<WebhookSettings>(
                builder.Configuration.GetSection(nameof(WebhookSettings)));

            builder.Services.Configure<DatabaseInitialDataSettings>(
                builder.Configuration.GetSection(nameof(DatabaseInitialDataSettings)));

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection(nameof(JwtSettings)));
            #endregion

            #region JwtBearer
            JwtSettings jwtSettings = builder.Configuration
                .GetSection(nameof(JwtSettings)).Get<JwtSettings>()
                    ?? throw new ArgumentNullException(nameof(JwtSettings));

            string? jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("JWT_KEY", "The JWT key string environment variable is not set.");
            }

            byte[] key = Encoding.ASCII.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkewSeconds)
                };
            });

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("RequireSuperAdminRole", policy =>
                    policy.RequireRole("SuperAdmin"))

                .AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Admin"))

                .AddPolicy("RequireAdminOrSuperAdminRole", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Admin") || context.User.IsInRole("SuperAdmin")));

            builder.Services.AddAuthorization();
            #endregion

            #region Postgresql
            var connectionString = Environment.GetEnvironmentVariable("POSTGRESQL");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("POSTGRESQL", "The connection string environment variable is not set.");
            }

            builder.Services.AddPooledDbContextFactory<AppDBContext>(options =>
                options.UseNpgsql(connectionString, conf =>
                    conf.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
            #endregion

            #region Mapper
            builder.Services.AddAutoMapper(config => config.AddProfile<MapperProfile>());
            #endregion

            #region Redis
            var configurationString = Environment.GetEnvironmentVariable("REDIS_CONFIGURATION");
            var instanceName = Environment.GetEnvironmentVariable("REDIS_INSTANCE_NAME");

            if (string.IsNullOrEmpty(configurationString))
            {
                throw new ArgumentNullException("REDIS_CONFIGURATION", "The redis configuration string environment variable is not set.");
            }

            if (string.IsNullOrEmpty(instanceName))
            {
                throw new ArgumentNullException("REDIS_INSTANCE_NAME", "The redis instance name string environment variable is not set.");
            }

            builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
                ConnectionMultiplexer.Connect($"{configurationString}, allowAdmin = true"));

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configurationString;
                options.InstanceName = instanceName;
            });
            #endregion

            #region Telegram Bot
            var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

            if (string.IsNullOrEmpty(telegramBotToken))
            {
                throw new ArgumentNullException("TELEGRAM_BOT_TOKEN", "The connection string environment variable is not set.");
            }

            builder.Services.AddHttpClient("tgclient")
                .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(telegramBotToken, httpClient));
            #endregion

            #region DI
            builder.Services.AddHostedService<HealthCheckService>();
            builder.Services.AddHostedService<MigrationService>();

            builder.Services.AddSingleton<IStaticCollectionsService, StaticCollectionsService>();
            builder.Services.AddHostedService<StaticCollectionsInit>();

            builder.Services.AddSingleton<IBotService, BotService>();
            builder.Services.AddHostedService<BotHostedService>();

            builder.Services.AddScoped<ICustomersRepository, CustomersRepository>();
            builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IStoreRepository, StoreRepository>();
            builder.Services.AddScoped<IAdminsRepository, AdminsRepository>();
            builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();

            builder.Services.AddSingleton<IApplicationRepository, ApplicationRepository>();

            builder.Services.AddScoped<ICacheService, CacheService>();

            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAdminService, AdminService>();

            builder.Services.AddScoped<IReplyMarkupService, ReplyMarkupService>();
            builder.Services.AddScoped<IInlineMarkupService, InlineMarkupService>();

            builder.Services.AddScoped<ICartService, CartService>();

            builder.Services.AddScoped<ToCustomerMapper>();
            builder.Services.AddScoped<ToCustomerDtoMapper>();

            builder.Services.AddScoped<II18nService, I18nService>();
            builder.Services.AddSingleton<ILocalizationData, LocalizationData>();
            builder.Services.AddHostedService<LocalizationDataHostedService>();

            builder.Services.AddScoped<IUpdateManager, UpdateManager>();

            builder.Services.AddScoped<IDistributorService, DistributorService>();

            builder.Services.AddScoped<ISendResponseService, SendResponseService>();

            builder.Services.AddScoped<IDataProcessorService, DataProcessorService>();

            IEnumerable<Type> commandTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetInterfaces().Contains(typeof(ITgCommand)))
                .Where(t => t.GetCustomAttribute<TgCommandAttribute>() != null);

            foreach (Type type in commandTypes)
            {
                builder.Services.AddScoped(type);
            }
            #endregion

            WebApplication app = builder.Build();
            StartupTime = DateTime.UtcNow;

            #region Development
#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI();
#endif
            #endregion

            #region Middlewares
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = HttpOnlyPolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.Strict,
                Secure = CookieSecurePolicy.Always
            });

            app.UseRouting();

            //app.UseHsts();
            //app.UseHttpsRedirection();
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXfo(options => options.Deny());

            app.UseMiddleware<CookieJwtMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();

            app.MapControllers();
            app.MapHealthChecks("/health");

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            #endregion

            app.Run();
        }
    }
}