using Microsoft.AspNetCore.Http;

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

        private readonly ICacheService _cache;

        private readonly IJwtService _jwtService;

        public AdminService(
                IPasswordService passwordService,
                IAdminsRepository adminsRepository,
                ICacheService cache,
                IJwtService jwtService
            )
        {
            _passwordService = passwordService;
            _adminsRepository = adminsRepository;
            _cache = cache;
            _jwtService = jwtService;
        }

        public async Task RegisterAsync(RegisterRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            RegisterRequestDto validRequest = request.VerificationData();

            string hash = _passwordService.Generate(validRequest.Password);

            await _adminsRepository.SaveNewAdminAsync(validRequest, hash, token);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, HttpContext httpContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            LoginRequest validRequest = request.VerificationData();

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

            ChangePasswordRequest validRequest = request.VerificationData();

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

        public async Task RemoveAdminAsync(RemoveAdminRequest request, HttpContext httpContext, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            RemoveAdminRequest validRequest = request.VerificationData();

            TokenClaims claims = httpContext.User.Claims.GetTokenClaims();

            await _adminsRepository.RemoveAdminAsync(
                superAdminLogin: claims.Login,
                password: validRequest.Password,
                removedLogin: validRequest.Login,
                token: token);

            await _adminsRepository.RemoveRefreshTokenAsync(validRequest.Login, token);
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
            return await _adminsRepository.GetCitiesAsync(token);
        }

        public async Task RemoveCityAsync(RemoveCityRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            RemoveCityRequestDto validRequest = request.VerificationData();

            await _adminsRepository.RemoveCityByIdAsync(validRequest.CityId, token);

            await _cache.ClearAndUpdateCacheAsync();
        }

        public async Task AddNewStoreAsync(AddNewStoreRequest request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            AddNewStoreRequestDto validRequest = request.VerificationData();

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
            return await _adminsRepository.GetStoresAsync(token);
        }
    }
}