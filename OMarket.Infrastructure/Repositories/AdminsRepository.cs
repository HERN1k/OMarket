using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OMarket.Domain.DTOs;
using OMarket.Domain.Entities;
using OMarket.Domain.Exceptions.App;
using OMarket.Domain.Interfaces.Application.Services.Password;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

namespace OMarket.Infrastructure.Repositories
{
    public class AdminsRepository : IAdminsRepository
    {
        private readonly IDbContextFactory<AppDBContext> _contextFactory;

        private readonly IPasswordService _passwordService;

        private readonly ILogger<AdminsRepository> _logger;

        public AdminsRepository(
                IDbContextFactory<AppDBContext> contextFactory,
                IPasswordService passwordService,
                ILogger<AdminsRepository> logger
            )
        {
            _contextFactory = contextFactory;
            _passwordService = passwordService;
            _logger = logger;
        }

        public async Task SaveNewAdminAsync(RegisterRequestDto request, string hash, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(hash))
            {
                throw new ApplicationException("Невдалось створити хеш паролю.");
            }

            if (request.Permission == "SuperAdmin")
            {
                throw new ApplicationException("Неможна створити нового супер адміна.");
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            bool isNewAdmin = await context.Admins
                .Where(admin => admin.Store!.Id == request.StoreId)
                .AnyAsync(token);

            if (isNewAdmin)
            {
                throw new ApplicationException("За цим магазином вже закріпленний адміністратор.");
            }

            bool isNewAdminLogin = await context.Admins
                .Where(admin => admin.AdminsCredentials.Login == request.Login)
                .AnyAsync(token);

            if (isNewAdminLogin)
            {
                throw new ApplicationException("Такий логін вже зайнято.");
            }

            AdminsPermission adminPermission = await context.AdminsPermissions
                .Where(permission => permission.Permission == request.Permission)
                .SingleOrDefaultAsync(token)
                    ?? throw new ArgumentException("Некоректне поле дозвіл.");

            Store store = await context.Stores
                .Where(store => store.Id == request.StoreId)
                .SingleOrDefaultAsync(token)
                    ?? throw new ArgumentException("Некоректне поле унікальний ідентифікатор магазину.");

            AdminsCredentials adminCredentials = new() { Login = request.Login, Hash = hash };

            Admin admin = new()
            {
                Store = store,
                AdminsPermission = adminPermission,
                AdminsCredentials = adminCredentials
            };

            await context.AddAsync(admin, token);

            await context.SaveChangesAsync(token);
        }

        public async Task<Guid?> VerifyAdminByIdAsync(long id, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                return await context.Admins
                    .AsNoTracking()
                    .Where(admin => admin.TgAccountId == id)
                    .Select(admin => admin.Id)
                    .SingleOrDefaultAsync(token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }

        public async Task<AdminDto> GetAdminByLoginAsync(string login, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            AdminsCredentials credentials = await context.AdminsCredentials
                .AsNoTracking()
                .Where(credentials => credentials.Login == login)
                .Include(credentials => credentials.Admin)
                    .ThenInclude(admin => admin.AdminsPermission)
                .Include(credentials => credentials.Admin.Store)
                .SingleOrDefaultAsync(token)
                    ?? throw new ArgumentException("Логін або пароль невірний.");

            return new AdminDto()
            {
                Id = credentials.Admin.Id,
                Login = credentials.Login,
                Hash = credentials.Hash,
                Permission = credentials.Admin.AdminsPermission.Permission,
                StoreId = credentials.Admin.Store?.Id,
                TgAccountId = credentials.Admin.TgAccountId
            };
        }

        public async Task SaveOrUpdateRefreshTokenAsync(string token, Guid adminId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(token))
            {
                throw new ApplicationException("Поле токен пусте.");
            }

            if (adminId == Guid.Empty)
            {
                throw new ApplicationException("Унікальній ідентифікатор адміністратора не знайдено.");
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            bool isValidAdmin = await context.Admins
                .Where(admin => admin.Id == adminId)
                .AnyAsync(cancellationToken);

            if (!isValidAdmin)
            {
                throw new ArgumentException("Такого адміністратора не знайдено.");
            }

            AdminToken? adminToken = await context.AdminTokens
                .Where(token => token.AdminId == adminId)
                .SingleOrDefaultAsync(cancellationToken);

            if (adminToken is not null)
            {
                adminToken.RefreshToken = token;
            }
            else
            {
                await context.AdminTokens.AddAsync(new()
                {
                    AdminId = adminId,
                    RefreshToken = token
                }, cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveRefreshTokenAsync(string login, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(login))
            {
                throw new ApplicationException("Поле логін пусте.");
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            Guid adminId = await context.AdminsCredentials
                .Where(credentials => credentials.Login == login)
                .Include(credentials => credentials.Admin)
                .Select(credentials => credentials.Admin.Id)
                .SingleOrDefaultAsync(token);

            if (adminId == Guid.Empty)
            {
                throw new ApplicationException("Унікальній ідентифікатор адміністратора не знайдено.");
            }

            AdminToken adminToken = await context.AdminTokens
                .Where(adminToken => adminToken.AdminId == adminId)
                .SingleOrDefaultAsync(token) ?? throw new ApplicationException("Токен не знайдено.");

            context.Remove(adminToken);

            await context.SaveChangesAsync(token);
        }

        public async Task RemoveRefreshTokenByTokenValueAsync(string token, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(token))
            {
                throw new ApplicationException("Поле токен пусте.");
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            AdminToken? adminToken = await context.AdminTokens
                .Where(adminToken => adminToken.RefreshToken == token)
                .SingleOrDefaultAsync(cancellationToken);

            if (adminToken is null)
            {
                return;
            }

            context.Remove(adminToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<string> ValidateLoginAndGetRefreshTokenAsync(string login, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(login))
            {
                throw new ApplicationException("Поле логін пусте.");
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            Guid adminId = await context.AdminsCredentials
                .Where(credentials => credentials.Login == login)
                .Include(credentials => credentials.Admin)
                .Select(credentials => credentials.Admin.Id)
                .SingleOrDefaultAsync(token);

            if (adminId == Guid.Empty)
            {
                throw new ApplicationException("Унікальній ідентифікатор адміністратора не знайдено.");
            }

            return await context.AdminTokens
                .Where(token => token.AdminId == adminId)
                .Select(token => token.RefreshToken)
                .SingleOrDefaultAsync(token) ?? throw new ApplicationException("Токен не знайдено.");
        }

        public async Task ChangePasswordAsync(string login, string hash, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            AdminsCredentials credentials = await context.AdminsCredentials
                .Where(credentials => credentials.Login == login)
                .SingleOrDefaultAsync(token)
                    ?? throw new ArgumentException("Логін або пароль невірний.");

            credentials.Hash = hash;

            await context.SaveChangesAsync(token);
        }

        public async Task RemoveAdminAsync(string superAdminLogin, string password, string removedLogin, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            string superAdminHash = await context.AdminsCredentials
                .Include(credentials => credentials.Admin)
                    .ThenInclude(admin => admin.AdminsPermission)
                .Where(credentials =>
                    credentials.Login == superAdminLogin &&
                    credentials.Admin.AdminsPermission.Permission == "SuperAdmin")
                .Select(credentials => credentials.Hash)
                .SingleOrDefaultAsync(token) ?? throw new ForbiddenAccessException();

            bool isValidPassword = _passwordService.Verify(password, superAdminHash);
            if (!isValidPassword)
            {
                throw new ArgumentException("Логін або пароль невірний.");
            }

            AdminsCredentials removedAdmin = await context.AdminsCredentials
                .Include(credentials => credentials.Admin)
                .Where(credentials => credentials.Login == removedLogin)
                .SingleOrDefaultAsync(token)
                    ?? throw new ArgumentException("Адміністратор для видалення не знайдений.");

            context.AdminsCredentials.Remove(removedAdmin);
            context.Admins.Remove(removedAdmin.Admin);

            await context.SaveChangesAsync(token);
        }

        public async Task AddNewCityAsync(string name, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            string cityName = name.Trim();
            if (cityName.Length > 63)
            {
                cityName = cityName[..63];
            }

            bool cityExists = await context.Cities
                .AsNoTracking()
                .Where(city => city.CityName == cityName)
                .AnyAsync(token);

            if (cityExists)
            {
                throw new ApplicationException($"Місто з назвою {cityName} вже існує.");
            }

            await context.Cities.AddAsync(new City() { CityName = cityName }, token);

            await context.SaveChangesAsync(token);
        }

        public async Task RemoveCityAsync(string name)
        {
            await using AppDBContext context = await _contextFactory.CreateDbContextAsync();

            string cityName = name.Trim();

            City? city = await context.Cities
                .Where(city => city.CityName == cityName)
                .SingleOrDefaultAsync();

            if (city is null)
            {
                return;
            }

            context.Cities.Remove(city);

            await context.SaveChangesAsync();
        }

        public async Task<List<CityDto>> GetCitiesAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            return await context.Cities
                .AsNoTracking()
                .Select(city => new CityDto()
                {
                    Id = city.Id,
                    CityName = city.CityName
                })
                .ToListAsync(token);
        }

        public async Task RemoveCityByIdAsync(Guid cityId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            City city = await context.Cities
                .Where(city => city.Id == cityId)
                .SingleOrDefaultAsync(token)
                    ?? throw new ApplicationException("Місто не знайдено.");

            context.Cities.Remove(city);

            await context.SaveChangesAsync(token);
        }

        public async Task<Guid> AddNewStoreAsync(AddNewStoreRequestDto request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            City? city = await context.Cities
                .Where(city => city.Id == request.CityId)
                .SingleOrDefaultAsync(token)
                    ?? throw new ApplicationException($"Місто не існує в базі данних.");

            bool storeExists = await context.StoreAddresses
                .AsNoTracking()
                .Where(address =>
                    address.Store.CityId == request.CityId &&
                    address.Address == request.Address)
                .AnyAsync(token);

            if (storeExists)
            {
                throw new ApplicationException($"Магазин за адресою {city.CityName} {request.Address} вже існує.");
            }

            StoreAddress address = new()
            {
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude
            };

            Store store = new()
            {
                PhoneNumber = request.PhoneNumber,
                Address = address,
                City = city
            };

            Guid result = store.Id;

            await context.Stores.AddAsync(store, token);

            await context.SaveChangesAsync(token);

            return result;
        }

        public async Task RemoveStoreAsync(Guid storeId)
        {
            await using AppDBContext context = await _contextFactory.CreateDbContextAsync();

            Store? store = await context.Stores
                .Where(store => store.Id == storeId)
                .SingleOrDefaultAsync();

            if (store is null)
            {
                return;
            }

            context.Stores.Remove(store);

            await context.SaveChangesAsync();
        }

        public async Task<List<StoreDtoResponse>> GetStoresAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            List<Store> stores = await context.Stores
                .AsNoTracking()
                .Include(store => store.Address)
                .Include(store => store.City)
                .Include(store => store.Admin)
#pragma warning disable CS8602
                    .ThenInclude(admin => admin.AdminsCredentials)
#pragma warning restore
                .ToListAsync(token);

            return stores.Select(store => new StoreDtoResponse()
            {
                Id = store.Id,
                AddressId = store.AddressId,
                CityId = store.CityId,
                AdminId = store.AdminId,
                Address = store.Address.Address,
                City = store.City.CityName,
                AdminLogin = store.Admin?.AdminsCredentials?.Login,
                TgChatId = store.TgChatId,
                PhoneNumber = store.PhoneNumber
            }).ToList();
        }
    }
}