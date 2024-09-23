using System.Reflection;

using AutoMapper;

using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

using NLog.Web;

using OMarket.Application.Middlewares;
using OMarket.Application.Services.Bot;
using OMarket.Application.Services.Distributor;
using OMarket.Application.Services.KeyboardMarkup;
using OMarket.Application.Services.Processor;
using OMarket.Application.Services.SendResponse;
using OMarket.Application.Services.StaticCollections;
using OMarket.Application.Services.TgUpdate;
using OMarket.Application.Services.Translator;
using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.Interfaces.Application.Services.Bot;
using OMarket.Domain.Interfaces.Application.Services.Distributor;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
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
using OMarket.Infrastructure.Extensions;
using OMarket.Infrastructure.Repositories;

using Telegram.Bot;

namespace OMarket.Configurators
{
    public class ApplicationConfigurator
    {
        #region Public properties
        public WebApplication WebApplication { get => _application; }
        #endregion

        #region Private properties
        private readonly WebApplicationBuilder _applicationBuilder;

        private readonly WebApplication _application;
        #endregion

        public ApplicationConfigurator(WebApplicationBuilder applicationBuilder)
        {
            ArgumentNullException.ThrowIfNull(applicationBuilder, nameof(applicationBuilder));

            _applicationBuilder = applicationBuilder;
            _application = ConfigureBuilder();

            ConfigureApplication();

            ArgumentNullException.ThrowIfNull(_applicationBuilder, nameof(_applicationBuilder));
            ArgumentNullException.ThrowIfNull(_application, nameof(_application));
        }

        #region Private methods
        private WebApplication ConfigureBuilder()
        {
            ArgumentNullException.ThrowIfNull(_applicationBuilder, nameof(_applicationBuilder));

            ConfigureBaseBuilder();
            ConfigureLogger();
            ConfigureSettings();
            ConfigureDb();
            ConfigureMapper();
            ConfigureRedis();
            ConfigureTelegramBotClient();
            ConfigureDI();

            return _applicationBuilder.Build();
        }

        private void ConfigureApplication()
        {
            ArgumentNullException.ThrowIfNull(_application, nameof(_application));
#if DEBUG
            ForDevelopment();
#endif
            ApplyMigrations();
            ConfigureSecurityPolicies();
            ConfigureBaseApp();
            ConfigureCustomMiddlewares();
        }

        private void ConfigureBaseBuilder()
        {
            ArgumentNullException.ThrowIfNull(_applicationBuilder, nameof(_applicationBuilder));

            _applicationBuilder.Logging
                .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
                .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

            _applicationBuilder.Services.AddControllers()
                .AddNewtonsoftJson();

            _applicationBuilder.Services.AddEndpointsApiExplorer();

            _applicationBuilder.Services.AddSwaggerGen();

            _applicationBuilder.Services.AddMemoryCache();

            _applicationBuilder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"/home/app/.aspnet/DataProtection-Keys"))
                .SetApplicationName("TourneyPulse");
        }

        private void ConfigureLogger()
        {
            ArgumentNullException.ThrowIfNull(_applicationBuilder, nameof(_applicationBuilder));

            _applicationBuilder.Logging.ClearProviders();
            _applicationBuilder.Host.UseNLog();
        }

        private void ConfigureSettings()
        {
            ArgumentNullException.ThrowIfNull(_applicationBuilder, nameof(_applicationBuilder));

            _applicationBuilder.Services.Configure<WebhookSettings>(
                _applicationBuilder.Configuration.GetSection(nameof(WebhookSettings)));

            _applicationBuilder.Services.Configure<DatabaseInitialDataSettings>(
                _applicationBuilder.Configuration.GetSection(nameof(DatabaseInitialDataSettings)));
        }

        private void ConfigureDb()
        {
            ArgumentNullException.ThrowIfNull(_applicationBuilder, nameof(_applicationBuilder));

            var connectionString = Environment.GetEnvironmentVariable("POSTGRESQL");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("POSTGRESQL", "The connection string environment variable is not set.");
            }

            _applicationBuilder.Services.AddPooledDbContextFactory<AppDBContext>(options =>
                options.UseNpgsql(connectionString, conf =>
                    conf.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
        }

        private void ConfigureMapper()
        {
            ArgumentNullException.ThrowIfNull(_applicationBuilder, nameof(_applicationBuilder));

            _applicationBuilder.Services
                .AddAutoMapper(config => config.AddProfile<MapperProfile>());
        }

        private void ConfigureRedis()
        {
            ArgumentNullException.ThrowIfNull(_applicationBuilder, nameof(_applicationBuilder));

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

            _applicationBuilder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configurationString;
                options.InstanceName = instanceName;
            });

            _applicationBuilder.Services.AddSingleton<IConfigureOptions<DistributedCacheEntryOptions>>(provider =>
                new ConfigureOptions<DistributedCacheEntryOptions>(options =>
                    {
                        options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                    }));
        }

        private void ConfigureTelegramBotClient()
        {
            ArgumentNullException.ThrowIfNull(_applicationBuilder, nameof(_applicationBuilder));

            var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");

            if (string.IsNullOrEmpty(telegramBotToken))
            {
                throw new ArgumentNullException("TELEGRAM_BOT_TOKEN", "The connection string environment variable is not set.");
            }

            _applicationBuilder.Services.AddHttpClient("tgclient")
                .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(telegramBotToken, httpClient));
        }

        private void ConfigureDI()
        {
            ArgumentNullException.ThrowIfNull(_applicationBuilder, nameof(_applicationBuilder));

            _applicationBuilder.Services.AddSingleton<IStaticCollectionsService, StaticCollectionsService>();

            _applicationBuilder.Services.AddSingleton<IBotService, BotService>();
            _applicationBuilder.Services.AddHostedService<BotHostedService>();

            _applicationBuilder.Services.AddScoped<ICityRepository, CityRepository>();
            _applicationBuilder.Services.AddScoped<ICustomersRepository, CustomersRepository>();
            _applicationBuilder.Services.AddScoped<IProductsRepository, ProductsRepository>();

            _applicationBuilder.Services.AddSingleton<IApplicationRepository, ApplicationRepository>();

            _applicationBuilder.Services.AddScoped<IReplyMarkupService, ReplyMarkupService>();
            _applicationBuilder.Services.AddScoped<IInlineMarkupService, InlineMarkupService>();

            _applicationBuilder.Services.AddScoped<ToCustomerMapper>();
            _applicationBuilder.Services.AddScoped<ToCustomerDtoMapper>();

            _applicationBuilder.Services.AddScoped<II18nService, I18nService>();
            _applicationBuilder.Services.AddSingleton<ILocalizationData, LocalizationData>();
            _applicationBuilder.Services.AddHostedService<LocalizationDataHostedService>();

            _applicationBuilder.Services.AddScoped<IUpdateManager, UpdateManager>();

            _applicationBuilder.Services.AddScoped<IDistributorService, DistributorService>();

            _applicationBuilder.Services.AddScoped<ISendResponseService, SendResponseService>();

            _applicationBuilder.Services.AddScoped<IDataProcessorService, DataProcessorService>();

            IEnumerable<Type> commandTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetInterfaces().Contains(typeof(ITgCommand)))
                .Where(t => t.GetCustomAttribute<TgCommandAttribute>() != null);

            foreach (Type type in commandTypes)
            {
                _applicationBuilder.Services.AddScoped(type);
            }
        }

#if DEBUG
        private void ForDevelopment()
        {
            ArgumentNullException.ThrowIfNull(_application, nameof(_application));

            _application.UseSwagger();

            _application.UseSwaggerUI();

            using IServiceScope scope = _application.Services.CreateScope();

            IMapper mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
#endif

        private void ApplyMigrations()
        {
            ArgumentNullException.ThrowIfNull(_application, nameof(_application));

            MigrationExtensions.ApplyMigrations(_application);
        }

        private void ConfigureSecurityPolicies()
        {
            ArgumentNullException.ThrowIfNull(_application, nameof(_application));

            _application.UseHsts();
            _application.UseHttpsRedirection();
        }

        private void ConfigureBaseApp()
        {
            ArgumentNullException.ThrowIfNull(_application, nameof(_application));

            _ = _application.Services.GetRequiredService<IStaticCollectionsService>();

            _application.UseCookiePolicy(new CookiePolicyOptions
            {
                HttpOnly = HttpOnlyPolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.Lax,
                Secure = CookieSecurePolicy.Always
            });

            _application.MapControllers();
        }

        private void ConfigureCustomMiddlewares()
        {
            ArgumentNullException.ThrowIfNull(_application, nameof(_application));

            _application.UseMiddleware<ExceptionHandlingMiddleware>();
        }
        #endregion
    }
}