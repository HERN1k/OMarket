using System.Text.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.Admin;
using OMarket.Domain.Interfaces.Application.Services.Cache;
using OMarket.Domain.Interfaces.Application.Services.Jwt;
using OMarket.Domain.Interfaces.Application.Services.Password;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Utilities;

namespace OMarket.Application.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly IPasswordService _passwordService;

        private readonly IAdminsRepository _adminsRepository;

        private readonly IDistributedCache _distributedCache;

        private readonly ICacheService _cache;

        private readonly IJwtService _jwtService;

        private readonly IWebHostEnvironment _environment;

        public AdminService(
                IPasswordService passwordService,
                IAdminsRepository adminsRepository,
                IDistributedCache distributedCache,
                ICacheService cache,
                IJwtService jwtService,
                IWebHostEnvironment environment
            )
        {
            _passwordService = passwordService;
            _adminsRepository = adminsRepository;
            _distributedCache = distributedCache;
            _cache = cache;
            _jwtService = jwtService;
            _environment = environment;
        }

        public async Task RegisterAsync(RegisterRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var validRequest = request.VerificationData();

            string hash = _passwordService.Generate(validRequest.Password);

            await _adminsRepository.SaveNewAdminAsync(validRequest, hash, token);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, HttpContext httpContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var validRequest = request.VerificationData();

            AdminDto admin = await _adminsRepository.GetAdminByLoginAsync(validRequest.Login, token);

            bool isValidPassword = _passwordService.Verify(validRequest.Password, admin.Hash);
            if (!isValidPassword)
            {
                throw new ArgumentException("Логін або пароль невірний.");
            }

            string accessToken = _jwtService.Generate(admin, TokenType.Access);
            string refreshToken = _jwtService.Generate(admin, TokenType.Refresh);

            await _adminsRepository.SaveOrUpdateRefreshTokenAsync(refreshToken, admin.Id, token);

            _jwtService.SetCookies(httpContext, accessToken, refreshToken);

            return new(admin.Permission, admin.Login);
        }

        public async Task LogoutAsync(HttpContext httpContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            TokenClaims claims = httpContext.User.Claims.GetTokenClaims();

            await _adminsRepository.RemoveRefreshTokenAsync(claims.Login, token);

            _jwtService.RemoveCookies(httpContext);
        }

        public async Task RefreshTokenAsync(HttpContext httpContext, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!httpContext.Request.Cookies.TryGetValue("JwtRefreshToken", out var token))
                {
                    throw new UnauthorizedAccessException("Токен не знайдено.");
                }

                TokenClaims claims;
                try
                {
                    claims = _jwtService.Verify(token);

                    string tokenFromDb = await _adminsRepository
                        .ValidateLoginAndGetRefreshTokenAsync(claims.Login, cancellationToken);

                    _jwtService.Verify(tokenFromDb);

                    if (token != tokenFromDb)
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
                catch (Exception)
                {
                    await _adminsRepository.RemoveRefreshTokenByTokenValueAsync(token, cancellationToken);
                    throw;
                }

                AdminDto admin = await _adminsRepository
                    .GetAdminByLoginAsync(claims.Login, cancellationToken);

                claims.VerificationData(admin.Login, admin.Permission);

                string accessToken = _jwtService.Generate(admin, TokenType.Access);

                _jwtService.SetAccessTokenInCookies(httpContext, accessToken);
            }
            catch (OperationCanceledException)
            {
                _jwtService.RemoveCookies(httpContext);
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                _jwtService.RemoveCookies(httpContext);
                throw;
            }
            catch (Exception ex)
            {
                _jwtService.RemoveCookies(httpContext);
                throw new UnauthorizedAccessException(ex.Message);
            }
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest request, HttpContext httpContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var validRequest = request.VerificationData();

            if (!httpContext.Request.Cookies.TryGetValue("JwtAccessToken", out var accessToken))
            {
                throw new ArgumentException("Токен не знайдено.");
            }

            TokenClaims claims = _jwtService.Verify(accessToken);

            AdminDto admin = await _adminsRepository.GetAdminByLoginAsync(claims.Login, token);

            bool isValidPassword = _passwordService.Verify(validRequest.Password, admin.Hash);
            if (!isValidPassword)
            {
                throw new ArgumentException("Старий пароль не сходиться з збереженим.");
            }

            string hash = _passwordService.Generate(validRequest.NewPassword);

            await _adminsRepository.ChangePasswordAsync(claims.Login, hash, token);

            await _adminsRepository.RemoveRefreshTokenAsync(claims.Login, token);

            _jwtService.RemoveCookies(httpContext);
        }

        public async Task AddNewCityAsync(AddNewCityRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(request.CityName))
            {
                throw new ArgumentNullException(nameof(request.CityName), "Поле назва міста пустe.");
            }

            await _adminsRepository.AddNewCityAsync(request.CityName, token);

            try
            {
                await _cache.ClearAndUpdateCacheAsync();
            }
            catch (Exception)
            {
                await _adminsRepository.RemoveCityAsync(request.CityName);
                throw;
            }
        }

        public async Task<List<CityDto>> GetCitiesAsync(CancellationToken token)
        {
            string? data = await _distributedCache.GetStringAsync(CacheKeys.AdminCities, token);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonSerializer.Deserialize<List<CityDto>>(data) ?? new();
            }

            List<CityDto> result = await _adminsRepository.GetCitiesAsync(token);

            data = JsonSerializer.Serialize<List<CityDto>>(result);

            await _distributedCache.SetStringAsync(CacheKeys.AdminCities, data, token);

            return result;
        }

        public async Task RemoveCityAsync(RemoveCityRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var validRequest = request.VerificationData();

            await _adminsRepository.RemoveCityByIdAsync(validRequest.CityId, token);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task AddNewStoreAsync(AddNewStoreRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var validRequest = request.VerificationData();

            Guid storeId = await _adminsRepository.AddNewStoreAsync(validRequest, token);

            try
            {
                await _cache.ClearAndUpdateCacheAsync();
            }
            catch (Exception)
            {
                await _adminsRepository.RemoveStoreAsync(storeId);
                throw;
            }
        }

        public async Task<List<StoreDtoResponse>> GetStoresAsync(CancellationToken token)
        {
            string? data = await _distributedCache.GetStringAsync(CacheKeys.AdminStores, token);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonSerializer.Deserialize<List<StoreDtoResponse>>(data) ?? new();
            }

            List<StoreDtoResponse> result = await _adminsRepository.GetStoresAsync(token);

            data = JsonSerializer.Serialize<List<StoreDtoResponse>>(result);

            await _distributedCache.SetStringAsync(CacheKeys.AdminStores, data, token);

            return result;
        }

        public async Task RemoveStoreAsync(RemoveStoreRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var validRequest = request.VerificationData();

            await _adminsRepository.RemoveStoreAsync(validRequest.StoreId);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task AddNewAdminAsync(AddNewAdminRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var validRequest = request.VerificationData();

            string hash = _passwordService.Generate(validRequest.Password);

            Guid adminId = await _adminsRepository.AddNewAdminAsync(new(
                Login: validRequest.Login,
                Password: hash,
                StoreId: validRequest.StoreId), token);

            try
            {
                await _cache.ClearAndUpdateCacheAsync();
            }
            catch (Exception)
            {
                await _adminsRepository.RemoveAdminAsync(adminId);
                throw;
            }
        }

        public async Task RemoveAdminAsync(RemoveAdminRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var validRequest = request.VerificationData();

            await _adminsRepository.RemoveAdminAsync(validRequest.AdminId);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task<List<AdminDtoResponse>> AdminsAsync(CancellationToken token)
        {
            string? data = await _distributedCache.GetStringAsync(CacheKeys.AdminAdmins, token);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonSerializer.Deserialize<List<AdminDtoResponse>>(data) ?? new();
            }

            List<AdminDtoResponse> result = await _adminsRepository.GetAdminsAsync(token);

            data = JsonSerializer.Serialize<List<AdminDtoResponse>>(result);

            await _distributedCache.SetStringAsync(CacheKeys.AdminAdmins, data, token);

            return result;
        }

        public async Task ChangeAdminPasswordAsync(ChangeAdminPasswordRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var validRequest = request.VerificationData();

            string hash = _passwordService.Generate(validRequest.Password);

            await _adminsRepository.ChangeAdminPasswordAsync(validRequest.AdminId, hash, token);

            await _adminsRepository.RemoveRefreshTokenByIdAsync(validRequest.AdminId, token);
        }

        public async Task ChangeCityNameAsync(ChangeCityNameRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var validRequest = request.VerificationData();

            await _adminsRepository.ChangeCityNameAsync(validRequest, token);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task ChangeStoreInfoAsync(ChangeStoreInfoRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var validRequest = request.VerificationData();

            await _adminsRepository.ChangeStoreInfoAsync(validRequest, token);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task ChangeStoreInfoBaseAsync(HttpContext httpContext, ChangeStoreInfoBaseRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var validRequest = request.VerificationData(httpContext);

            await _adminsRepository.ChangeStoreInfoAsync(validRequest, token);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task<ReviewResponse> StoreReviewAsync(Guid storeId, int page, CancellationToken token)
        {
            string cacheKey = $"{CacheKeys.AdminReviews}{storeId}{page}";

            string? data = await _distributedCache.GetStringAsync(cacheKey, token);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonSerializer.Deserialize<ReviewResponse>(data) ?? new();
            }

            ReviewResponse result = await _adminsRepository.GetStoreReviewWithPagination(storeId, page, token);

            data = JsonSerializer.Serialize<ReviewResponse>(result);

            await _distributedCache.SetStringAsync(cacheKey, data, token);

            return result;
        }

        public async Task RemoveStoreReviewAsync(Guid reviewId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await _adminsRepository.RemoveReviewAsync(reviewId);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task BlockReviewsAsync(long customerId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await _adminsRepository.BlockReviewsAsync(customerId, token);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task UnBlockReviewsAsync(long customerId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await _adminsRepository.UnBlockReviewsAsync(customerId, token);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task BlockOrdersAsync(long customerId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await _adminsRepository.BlockOrdersAsync(customerId, token);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task UnBlockOrdersAsync(long customerId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await _adminsRepository.UnBlockOrdersAsync(customerId, token);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task<CustomerDtoResponse?> GetCustomerByIdAsync(long customerId, CancellationToken token)
        {
            return await _adminsRepository.GetCustomerByIdAsync(customerId, token);
        }

        public async Task<CustomerDtoResponse?> GetCustomerByPhoneNumberAsync(string phoneNumber, CancellationToken token)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return null;
            }

            return await _adminsRepository.GetCustomerByPhoneNumberAsync(phoneNumber, token);
        }

        public async Task<List<ProductTypesDto>> ProductTypesAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            string? data = await _distributedCache.GetStringAsync(CacheKeys.AdminProductTypes, token);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonSerializer.Deserialize<List<ProductTypesDto>>(data) ?? new();
            }

            List<ProductTypesDto> result = await _adminsRepository.ProductTypesAsync(token);
            data = JsonSerializer.Serialize<List<ProductTypesDto>>(result);

            await _distributedCache.SetStringAsync(CacheKeys.AdminProductTypes, data, token);

            return result;
        }

        public async Task AddNewProductAsync(IFormFile file, string metadata, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Відсутній файл.");
            }

            if (string.IsNullOrEmpty(metadata))
            {
                throw new ArgumentException("Відсутні метадані.");
            }

            AddNewProductMetadata productMetadata = Newtonsoft.Json.
                JsonConvert.DeserializeObject<AddNewProductMetadata>(metadata)
                    ?? throw new ArgumentException($"Невдалось десеріалізовати метадані.");

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            AddNewProductDto validRequest = productMetadata.VerificationData(extension, file.ContentType);

            Guid productId = await _adminsRepository.CreateNewProductAsync(validRequest, token);

            try
            {
                string wwwRootPath = _environment.WebRootPath;

                string filePath = Path.Combine(wwwRootPath, "Static", $"{productId}{validRequest.PhotoExtension}");

                string directoryName = Path.GetDirectoryName(filePath) ?? string.Empty;

                if (productId == Guid.Empty ||
                    string.IsNullOrEmpty(wwwRootPath) ||
                    string.IsNullOrEmpty(filePath) ||
                    string.IsNullOrEmpty(filePath))
                {
                    throw new ApplicationException();
                }

                Directory.CreateDirectory(directoryName);

                using FileStream stream = new(filePath, FileMode.Create);
                await file.CopyToAsync(stream, token);

                await _cache.ClearAndUpdateCacheAsync();
            }
            catch (Exception)
            {
                await _adminsRepository.RemoveProductByExceptionAsync(productId);
                throw;
            }
        }

        public async Task ChangeProductAsync(IFormFile file, string metadata, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Відсутній файл.");
            }

            if (string.IsNullOrEmpty(metadata))
            {
                throw new ArgumentException("Відсутні метадані.");
            }

            ChangeProductMetadata productMetadata = Newtonsoft.Json.
                JsonConvert.DeserializeObject<ChangeProductMetadata>(metadata)
                    ?? throw new ArgumentException($"Невдалось десеріалізовати метадані.");

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            ChangeProductDto validRequest = productMetadata.VerificationData(extension, file.ContentType);

            Guid productId = await _adminsRepository.ChangeProductAsync(validRequest, token);

            string wwwRootPath = _environment.WebRootPath;

            string filePath = Path.Combine(wwwRootPath, "Static", $"{productId}{validRequest.PhotoExtension}");

            string directoryName = Path.GetDirectoryName(filePath) ?? string.Empty;

            if (string.IsNullOrEmpty(wwwRootPath) ||
                string.IsNullOrEmpty(filePath) ||
                string.IsNullOrEmpty(filePath))
            {
                throw new ApplicationException();
            }

            Directory.CreateDirectory(directoryName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using FileStream stream = new(filePath, FileMode.Create);
            await file.CopyToAsync(stream, token);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task ChangeProductWithoutFileAsync(string metadata, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(metadata))
            {
                throw new ArgumentException("Відсутні метадані.");
            }

            ChangeProductMetadata productMetadata = Newtonsoft.Json.
                JsonConvert.DeserializeObject<ChangeProductMetadata>(metadata)
                    ?? throw new ArgumentException($"Невдалось десеріалізовати метадані.");

            ChangeProductDto validRequest = productMetadata.VerificationData();

            await _adminsRepository.ChangeProductAsync(validRequest, token);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task RemoveProductAsync(Guid productId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (productId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(productId), "Поле унікальний ідентифікатор продукту пусте.");
            }

            string photoName = await _adminsRepository.RemoveProductAsync(productId);

            string wwwRootPath = _environment.WebRootPath;

            string filePath = Path.Combine(wwwRootPath, "Static", $"{photoName}");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task<ProductResponse> GetProductsAsync(Guid typeId, int page, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (page < 0)
            {
                throw new ArgumentException();
            }

            string cacheKey = $"{CacheKeys.AdminProductsWithoutStoreId}{typeId}{page}";

            string? data = await _distributedCache.GetStringAsync(cacheKey, token);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonSerializer.Deserialize<ProductResponse>(data) ?? new();
            }

            ProductResponse result = await _adminsRepository.GetProductsWithPaginationAsync(typeId, page, token);
            data = JsonSerializer.Serialize<ProductResponse>(result);

            await _distributedCache.SetStringAsync(cacheKey, data, token);

            return result;
        }

        public async Task<ProductResponse> GetProductsWithStoreAsync(HttpContext httpContext, Guid typeId, int page, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (page < 0)
            {
                throw new ArgumentException();
            }

            Guid storeId = httpContext.User.Claims.GetStoreId();

            string cacheKey = $"{CacheKeys.AdminProductsWithStoreId}{storeId}{typeId}{page}";

            string? data = await _distributedCache.GetStringAsync(cacheKey, token);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonSerializer.Deserialize<ProductResponse>(data) ?? new();
            }

            ProductResponse result = await _adminsRepository.GetProductsWithPaginationAndStoreIdAsync(storeId, typeId, page, token);
            data = JsonSerializer.Serialize<ProductResponse>(result);

            await _distributedCache.SetStringAsync(cacheKey, data, token);

            return result;
        }

        public async Task ChangeDataStoreProductStatusAsync(HttpContext httpContext, Guid productId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Guid storeId = httpContext.User.Claims.GetStoreId();

            await _adminsRepository.ChangeDataStoreProductStatusAsync(storeId, productId, token);

            _cache.ClearMemoryCache();
            await _cache.ClearRedisCacheAsync();
        }
    }
}