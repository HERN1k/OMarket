using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OMarket.Domain.DTOs;
using OMarket.Domain.Entities;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

namespace OMarket.Infrastructure.Repositories
{
    public class AdminsRepository : IAdminsRepository
    {
        private readonly IDbContextFactory<AppDBContext> _contextFactory;

        private readonly ILogger<AdminsRepository> _logger;

        private readonly int _pageSizeReview = 5;

        private readonly int _pageSizeProduct = 12;

        public AdminsRepository(
                IDbContextFactory<AppDBContext> contextFactory,
                ILogger<AdminsRepository> logger
            )
        {
            _contextFactory = contextFactory;
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
                StoreId = credentials.Admin.Store?.Id
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

        public async Task RemoveRefreshTokenByIdAsync(Guid adminId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (adminId == Guid.Empty)
            {
                return;
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            AdminToken? adminToken = await context.AdminTokens
                .Where(adminToken => adminToken.AdminId == adminId)
                .SingleOrDefaultAsync(token);

            if (adminToken is null)
            {
                return;
            }

            context.Remove(adminToken);

            await context.SaveChangesAsync(token);
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

            bool hasStoresOrAdmins = await context.Stores
                .Where(store => store.CityId == city.Id)
                .AnyAsync()
                || await context.Admins
                .Include(admin => admin.Store)
                .Where(admin => admin.Store != null && admin.Store.CityId == city.Id)
                .AnyAsync();

            if (!hasStoresOrAdmins)
            {
                context.Cities.Remove(city);
            }
            else
            {
                List<Admin> adminsToRemove = await context.Admins
                    .Where(admin => admin.Store != null && admin.Store.CityId == city.Id)
                    .ToListAsync();

                context.Cities.Remove(city);
                context.Admins.RemoveRange(adminsToRemove);
            }

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

            bool hasStoresOrAdmins = await context.Stores
                .Where(store => store.CityId == city.Id)
                .AnyAsync(token)
                || await context.Admins
                .Include(admin => admin.Store)
                .Where(admin => admin.Store != null && admin.Store.CityId == city.Id)
                .AnyAsync(token);

            if (!hasStoresOrAdmins)
            {
                context.Cities.Remove(city);
            }
            else
            {
                List<Admin> adminsToRemove = await context.Admins
                    .Where(admin => admin.Store != null && admin.Store.CityId == city.Id)
                    .ToListAsync(token);

                context.Cities.Remove(city);
                context.Admins.RemoveRange(adminsToRemove);
            }

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

            if (store.AdminId != null)
            {
                Admin? admin = await context.Admins
                    .Where(admin => admin.Id == store.AdminId)
                    .SingleOrDefaultAsync();

                if (admin is null)
                {
                    context.Stores.Remove(store);

                    await context.SaveChangesAsync();

                    return;
                }

                context.Stores.Remove(store);
                context.Admins.Remove(admin);

                await context.SaveChangesAsync();
            }
            else
            {
                context.Stores.Remove(store);

                await context.SaveChangesAsync();
            }
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
                CityId = store.CityId,
                AdminId = store.AdminId,
                Address = store.Address.Address,
                City = store.City.CityName,
                AdminLogin = store.Admin?.AdminsCredentials?.Login,
                TgChatId = store.TgChatId,
                PhoneNumber = store.PhoneNumber
            }).ToList();
        }

        public async Task RemoveAdminAsync(Guid adminId)
        {
            await using AppDBContext context = await _contextFactory.CreateDbContextAsync();

            Admin? admin = await context.Admins
                .Where(admin => admin.Id == adminId)
                .SingleOrDefaultAsync();

            if (admin is null)
            {
                return;
            }

            context.Admins.Remove(admin);

            await context.SaveChangesAsync();
        }

        public async Task<Guid> AddNewAdminAsync(AddNewAdminRequestDto request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            bool adminExists = await context.AdminsCredentials
                .AsNoTracking()
                .Where(admin => admin.Login == request.Login)
                .AnyAsync(token);

            if (adminExists)
            {
                throw new ApplicationException($"Адміністратор з логіном {request.Login} вже існує.");
            }

            Store store = await context.Stores
                .Where(store => store.Id == request.StoreId)
                .SingleOrDefaultAsync(token) ?? throw new ApplicationException($"Такого магазин не знайдено.");

            if (store.AdminId != null)
            {
                throw new ApplicationException($"У цього магазина вже є адміністратор.");
            }

            AdminsPermission permission = await context.AdminsPermissions
                .Where(permission => permission.Permission == "Admin")
                .SingleOrDefaultAsync(token) ?? throw new ApplicationException($"Дозвіл не знайдено.");

            AdminsCredentials credentials = new()
            {
                Login = request.Login,
                Hash = request.Password
            };

            Admin admin = new()
            {
                Store = store,
                AdminsPermission = permission,
                AdminsCredentials = credentials
            };

            Guid result = admin.Id;

            await context.Admins.AddAsync(admin, token);

            await context.SaveChangesAsync(token);

            return result;
        }

        public async Task<List<AdminDtoResponse>> GetAdminsAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            List<Admin> admins = await context.Admins
                .AsNoTracking()
#pragma warning disable CS8602
                .Include(admin => admin.Store)
                    .ThenInclude(store => store.Address)
                .Include(admin => admin.Store)
                    .ThenInclude(store => store.City)
#pragma warning restore
                .Include(admin => admin.AdminsCredentials)
                .Include(admin => admin.AdminsPermission)
                .Where(admin => admin.AdminsPermission != null && admin.AdminsPermission.Permission != "SuperAdmin")
                .ToListAsync(token);

            return admins.Select(admin => new AdminDtoResponse()
            {
                Id = admin.Id,
                Login = admin.AdminsCredentials.Login,
                Permission = admin.AdminsPermission.Permission,
                StoreId = admin.Store?.Id,
                StoreName = $"{admin.Store?.City?.CityName} {admin.Store?.Address?.Address}"
            }).ToList();
        }

        public async Task ChangeAdminPasswordAsync(Guid adminId, string hash, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            Admin admin = await context.Admins
                .Where(admin => admin.Id == adminId)
                .Include(admin => admin.AdminsCredentials)
                .SingleOrDefaultAsync(token) ?? throw new ApplicationException($"Такого адміністратора не знайдено.");

            admin.AdminsCredentials.Hash = hash;

            await context.SaveChangesAsync(token);
        }

        public async Task ChangeCityNameAsync(ChangeCityNameRequestDto request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            City city = await context.Cities
                .Where(city => city.Id == request.CityId)
                .SingleOrDefaultAsync(token) ?? throw new ApplicationException($"Таке місто не знайдено.");

            city.CityName = request.CityName;

            await context.SaveChangesAsync(token);
        }

        public async Task ChangeStoreInfoAsync(ChangeStoreInfoRequestDto request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            Store store = await context.Stores
                .Where(store => store.Id == request.StoreId)
                .Include(store => store.Address)
                .SingleOrDefaultAsync(token) ?? throw new ApplicationException($"Такий магазин не знайдено.");

            if (request.Address is not null)
            {
                store.Address.Address = request.Address;
            }

            if (request.PhoneNumber is not null)
            {
                store.PhoneNumber = request.PhoneNumber;
            }

            if (request.Longitude is not null)
            {
                store.Address.Longitude = (decimal)request.Longitude;
            }

            if (request.Latitude is not null)
            {
                store.Address.Latitude = (decimal)request.Latitude;
            }

            if (request.TgChatId is not null)
            {
                store.TgChatId = request.TgChatId;
            }

            await context.SaveChangesAsync(token);
        }

        public async Task<ReviewResponse> GetStoreReviewWithPagination(Guid storeId, int page, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (page == 0)
            {
                return new();
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            int maxPageNumber = await context.Reviews
                .AsNoTracking()
                .Where(review => review.StoreId == storeId)
                .CountAsync(token);

            if (maxPageNumber == 0)
            {
                return new();
            }

            List<ReviewDto> reviews = await context.Reviews
                .AsNoTracking()
                .Where(review => review.StoreId == storeId)
                .OrderByDescending(review => review.CreatedAt)
                .Skip((page - 1) * _pageSizeReview)
                .Take(_pageSizeReview)
                .Select(review => new ReviewDto()
                {
                    Id = review.Id,
                    Text = review.Text,
                    CustomerId = review.CustomerId,
                    StoreId = review.StoreId,
                    CreatedAt = review.CreatedAt,
                })
                .ToListAsync(token);

            return new()
            {
                Reviews = reviews,
                PageCount = (int)Math.Ceiling((double)maxPageNumber / _pageSizeReview),
                TotalQuantity = maxPageNumber
            };
        }

        public async Task RemoveReviewAsync(Guid reviewId)
        {
            if (reviewId == Guid.Empty)
            {
                throw new ApplicationException($"Невалідний унікальний ідентифікатор відгуку.");
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync();

            Review review = await context.Reviews
                .Where(review => review.Id == reviewId)
                .SingleOrDefaultAsync() ?? throw new ApplicationException($"Відгук не знайдено.");

            context.Reviews.Remove(review);

            await context.SaveChangesAsync();
        }

        public async Task BlockReviewsAsync(long customerId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            Customer customer = await context.Customers
                .Where(cutomer => cutomer.Id == customerId)
                .SingleOrDefaultAsync(token) ?? throw new ApplicationException($"Такого клієнта не знайдено.");

            customer.BlockedReviews = true;

            await context.SaveChangesAsync(token);
        }

        public async Task UnBlockReviewsAsync(long customerId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            Customer customer = await context.Customers
                .Where(cutomer => cutomer.Id == customerId)
                .SingleOrDefaultAsync(token) ?? throw new ApplicationException($"Такого клієнта не знайдено.");

            customer.BlockedReviews = false;

            await context.SaveChangesAsync(token);
        }

        public async Task BlockOrdersAsync(long customerId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            Customer customer = await context.Customers
                .Where(cutomer => cutomer.Id == customerId)
                .SingleOrDefaultAsync(token) ?? throw new ApplicationException($"Такого клієнта не знайдено.");

            customer.BlockedOrders = true;

            await context.SaveChangesAsync(token);
        }

        public async Task UnBlockOrdersAsync(long customerId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            Customer customer = await context.Customers
                .Where(cutomer => cutomer.Id == customerId)
                .SingleOrDefaultAsync(token) ?? throw new ApplicationException($"Такого клієнта не знайдено.");

            customer.BlockedOrders = false;

            await context.SaveChangesAsync(token);
        }

        public async Task<CustomerDtoResponse?> GetCustomerByIdAsync(long customerId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            CustomerDtoResponse? customer = await context.Customers
                .AsNoTracking()
                .Where(customer => customer.Id == customerId && customer.Store != null)
                .Include(customer => customer.Store)
                    .ThenInclude(store => store!.Address)
                .Include(customer => customer.Store)
                    .ThenInclude(store => store!.City)
                .Select(customer => new CustomerDtoResponse()
                {
                    Id = customer.Id,
                    Username = customer.Username,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    PhoneNumber = customer.PhoneNumber,
                    IsBot = customer.IsBot,
                    StoreAddress = $"{customer.Store!.City.CityName} {customer.Store!.Address.Address}",
                    CreatedAt = customer.CreatedAt,
                    BlockedOrders = customer.BlockedOrders,
                    BlockedReviews = customer.BlockedReviews
                })
                .SingleOrDefaultAsync(token);

            return customer;
        }

        public async Task<CustomerDtoResponse?> GetCustomerByPhoneNumberAsync(string phoneNumber, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(phoneNumber))
            {
                return null;
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            CustomerDtoResponse? customer = await context.Customers
                .AsNoTracking()
                .Where(customer => customer.PhoneNumber == phoneNumber && customer.Store != null)
                .Include(customer => customer.Store)
                    .ThenInclude(store => store!.Address)
                .Include(customer => customer.Store)
                    .ThenInclude(store => store!.City)
                .Select(customer => new CustomerDtoResponse()
                {
                    Id = customer.Id,
                    Username = customer.Username,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    PhoneNumber = customer.PhoneNumber,
                    IsBot = customer.IsBot,
                    StoreAddress = $"{customer.Store!.City.CityName} {customer.Store!.Address.Address}",
                    CreatedAt = customer.CreatedAt,
                    BlockedOrders = customer.BlockedOrders,
                    BlockedReviews = customer.BlockedReviews
                })
                .SingleOrDefaultAsync(token);

            return customer;
        }

        public async Task<List<ProductTypesDto>> ProductTypesAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            return await context.ProductTypes
                .AsNoTracking()
                .Include(type => type.ProductUnderTypes)
                .Select(type => new ProductTypesDto()
                {
                    TypeId = type.Id,
                    Type = type.TypeName,
                    UnderTypes = type.ProductUnderTypes
                        .Select(underType => new ProductUnderTypesDto()
                        {
                            UnderTypeId = underType.Id,
                            UnderType = underType.UnderTypeName
                        })
                        .ToList()
                })
                .ToListAsync(token);
        }

        public async Task RemoveProductByExceptionAsync(Guid productId)
        {
            if (productId == Guid.Empty)
            {
                return;
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync();

            Product? product = await context.Products
                .Where(product => product.Id == productId)
                .SingleOrDefaultAsync();

            if (product is null)
            {
                return;
            }

            context.Products.Remove(product);

            await context.SaveChangesAsync();
        }

        public async Task<Guid> CreateNewProductAsync(AddNewProductDto request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            ProductType type = await context.ProductTypes
                .Where(type => type.Id == request.TypeId)
                .SingleOrDefaultAsync(token) ?? throw new ArgumentException("Такого типу товарів незнайдено.");

            ProductUnderType underType = await context.ProductUnderTypes
                .Where(underType => underType.Id == request.UnderTypeId)
                .SingleOrDefaultAsync(token) ?? throw new ArgumentException("Такого під-типу товарів незнайдено.");

            Product product = new()
            {
                Name = request.Name,
                PhotoUri = string.Empty,
                ProductType = type,
                ProductUnderType = underType,
                Price = request.Price,
                Dimensions = request.Dimensions,
                Description = request.Description,
            };
            product.PhotoUri = $"{product.Id}{request.PhotoExtension}";

            Guid result = product.Id;

            await context.Products.AddAsync(product, token);

            await context.SaveChangesAsync(token);

            return result;
        }

        public async Task<Guid> ChangeProductAsync(ChangeProductDto request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            Product product = await context.Products
                .Where(product => product.Id == request.ProductId)
                .SingleOrDefaultAsync(token) ?? throw new ArgumentException("Такого товару незнайдено.");

            if (request.Name is not null)
            {
                product.Name = request.Name;
            }

            if (request.Price is not null)
            {
                product.Price = (decimal)request.Price;
            }

            if (request.Dimensions is not null)
            {
                product.Dimensions = request.Dimensions;
            }

            if (request.Description is not null)
            {
                product.Description = request.Description;
            }

            Guid result = product.Id;

            await context.SaveChangesAsync(token);

            return result;
        }

        public async Task<string> RemoveProductAsync(Guid productId)
        {
            if (productId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(productId), "Поле унікальний ідентифікатор продукту пусте.");
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync();

            Product product = await context.Products
                .Where(product => product.Id == productId)
                .SingleOrDefaultAsync() ?? throw new ArgumentException("Такого товару незнайдено.");

            string result = product.PhotoUri;

            context.Products.Remove(product);

            await context.SaveChangesAsync();

            return result;
        }

        public async Task<ProductResponse> GetProductsWithPaginationAsync(Guid typeId, int page, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (page == 0)
            {
                return new();
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            int maxPageNumber = await context.Products
                .AsNoTracking()
                .Where(product => product.TypeId == typeId)
                .CountAsync(token);

            if (maxPageNumber == 0)
            {
                return new();
            }

            List<ProductDtoResponse> products = await context.Products
                .AsNoTracking()
                .Where(product => product.TypeId == typeId)
                .Include(product => product.ProductType)
                .Include(product => product.ProductUnderType)
                .OrderBy(product => product.Price)
                .Skip((page - 1) * _pageSizeProduct)
                .Take(_pageSizeProduct)
                .Select(product => new ProductDtoResponse()
                {
                    Id = product.Id,
                    Name = product.Name,
                    PhotoUri = product.PhotoUri,
                    TypeId = product.TypeId,
                    Type = product.ProductType.TypeName,
                    UnderTypeId = product.UnderTypeId,
                    UnderType = product.ProductUnderType.UnderTypeName,
                    Price = product.Price,
                    Dimensions = product.Dimensions,
                    Description = product.Description,
                    Status = false
                })
                .ToListAsync(token);

            return new()
            {
                Products = products,
                PageCount = (int)Math.Ceiling((double)maxPageNumber / _pageSizeProduct),
                TotalQuantity = maxPageNumber
            };
        }

        public async Task<ProductResponse> GetProductsWithPaginationAndStoreIdAsync(Guid storeId, Guid typeId, int page, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (page == 0)
            {
                return new();
            }

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            int maxPageNumber = await context.Products
                .AsNoTracking()
                .Where(product => product.TypeId == typeId)
                .CountAsync(token);

            if (maxPageNumber == 0)
            {
                return new();
            }

            List<ProductDtoResponse> products = await (
                from productTemp in context.Products
                    .AsNoTracking()
                    .Include(product => product.ProductType)
                    .Include(product => product.ProductUnderType)
                join dataStoreProduct in context.DataStoreProducts.AsNoTracking()
                on new { ProductId = productTemp.Id, StoreId = storeId } equals new { dataStoreProduct.ProductId, dataStoreProduct.StoreId }
                into storeGroup
                from storeProduct in storeGroup.DefaultIfEmpty()
                where productTemp.TypeId == typeId
                orderby productTemp.Price
                select new ProductDtoResponse()
                {
                    Id = productTemp.Id,
                    Name = productTemp.Name,
                    PhotoUri = productTemp.PhotoUri,
                    TypeId = productTemp.TypeId,
                    Type = productTemp.ProductType.TypeName,
                    UnderTypeId = productTemp.UnderTypeId,
                    UnderType = productTemp.ProductUnderType.UnderTypeName,
                    Price = productTemp.Price,
                    Dimensions = productTemp.Dimensions,
                    Description = productTemp.Description,
                    Status = storeProduct != null && storeProduct.Status
                })
                .Skip((page - 1) * _pageSizeProduct)
                .Take(_pageSizeProduct)
                .ToListAsync(token);

            return new()
            {
                Products = products,
                PageCount = (int)Math.Ceiling((double)maxPageNumber / _pageSizeProduct),
                TotalQuantity = maxPageNumber
            };
        }

        public async Task ChangeDataStoreProductStatusAsync(Guid storeId, Guid productId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

            if (storeId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор магазину передано в неправильному форматі.");
            }

            if (productId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор товару передано в неправильному форматі.");
            }

            DataStoreProduct? storeProduct = await context.DataStoreProducts
                .Where(storeProduct =>
                    storeProduct.StoreId == storeId &&
                    storeProduct.ProductId == productId)
                .SingleOrDefaultAsync(token);

            if (storeProduct is not null)
            {
                bool status = !storeProduct.Status;

                storeProduct.Status = status;

                await context.SaveChangesAsync(token);

                return;
            }

            Store store = await context.Stores
                .Where(store => store.Id == storeId)
                .SingleOrDefaultAsync(token) ?? throw new ArgumentException("Такий магазин незнайдено.");

            Product product = await context.Products
                .Where(product => product.Id == productId)
                .SingleOrDefaultAsync(token) ?? throw new ArgumentException("Такий товар незнайдено.");

            ProductUnderType productUnderType = await context.ProductUnderTypes
                .Where(productUnderType => productUnderType.Id == product.UnderTypeId)
                .SingleOrDefaultAsync(token) ?? throw new ApplicationException();

            DataStoreProduct dataStoreProduct = new DataStoreProduct()
            {
                Product = product,
                Store = store,
                ProductUnderType = productUnderType,
                Status = true
            };

            await context.DataStoreProducts.AddAsync(dataStoreProduct, token);

            await context.SaveChangesAsync(token);
        }
    }
}