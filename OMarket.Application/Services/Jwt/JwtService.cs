using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.Jwt;
using OMarket.Domain.Settings;

namespace OMarket.Application.Services.Jwt
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        private readonly SigningCredentials _jwtCredentials;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;

            string? jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("JWT_KEY", "The JWT key string environment variable is not set.");
            }

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey));

            _jwtCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public string Generate(AdminDto admin, TokenType type)
        {
            Claim[] claims = new[]
            {
                new Claim(ClaimTypes.Role, admin.Permission),
                new Claim(ClaimTypes.Name, admin.Login),
                new Claim(ClaimTypes.Locality, admin.StoreId.ToString() ?? Guid.Empty.ToString())
            };

            DateTime expiration = type == TokenType.Access
                ? DateTime.Now.AddMinutes(_jwtSettings.ExpiresInMinutesAccess)
                : DateTime.Now.AddMinutes(_jwtSettings.ExpiresInMinutesRefresh);

            JwtSecurityToken token = new(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: _jwtCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public TokenClaims Verify(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Поле токен пусте.");
            }

            JwtSecurityTokenHandler tokenHandler = new();

            byte[] key = Encoding.ASCII.GetBytes(
                Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new UnauthorizedAccessException());

            try
            {
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(_jwtSettings.ClockSkewSeconds)
                }, out var validatedToken);

                string? permission = principal.FindFirst(ClaimTypes.Role)?.Value;
                string? login = principal.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(permission) || string.IsNullOrEmpty(login))
                {
                    throw new UnauthorizedAccessException("Токен недійсний або відсутні обов'язкові поля.");
                }

                return new(permission, login);
            }
            catch (SecurityTokenExpiredException)
            {
                throw new UnauthorizedAccessException("В токена минув строк придатності.");
            }
            catch (SecurityTokenException)
            {
                throw new UnauthorizedAccessException("Неправильний токен.");
            }
            catch (Exception)
            {
                throw new UnauthorizedAccessException();
            }
        }

        public void SetCookies(HttpContext httpContext, string accessToken, string refreshToken)
        {
            httpContext.Response.Cookies.Append("JwtAccessToken", accessToken, new()
            {
                HttpOnly = true,
                Secure = true,
                MaxAge = TimeSpan.FromMinutes(_jwtSettings.ExpiresInMinutesAccess),
                SameSite = SameSiteMode.Strict
            });

            httpContext.Response.Cookies.Append("JwtRefreshToken", refreshToken, new()
            {
                HttpOnly = true,
                Secure = true,
                MaxAge = TimeSpan.FromMinutes(_jwtSettings.ExpiresInMinutesRefresh),
                SameSite = SameSiteMode.Strict
            });
        }

        public void SetAccessTokenInCookies(HttpContext httpContext, string token)
        {
            httpContext.Response.Cookies.Append("JwtAccessToken", token, new()
            {
                HttpOnly = true,
                Secure = true,
                MaxAge = TimeSpan.FromMinutes(_jwtSettings.ExpiresInMinutesAccess),
                SameSite = SameSiteMode.Strict
            });
        }

        public void RemoveCookies(HttpContext httpContext)
        {
            httpContext.Response.Cookies.Append("JwtAccessToken", string.Empty, new()
            {
                HttpOnly = true,
                Secure = true,
                MaxAge = TimeSpan.FromSeconds(0),
                SameSite = SameSiteMode.Strict
            });

            httpContext.Response.Cookies.Append("JwtRefreshToken", string.Empty, new()
            {
                HttpOnly = true,
                Secure = true,
                MaxAge = TimeSpan.FromSeconds(0),
                SameSite = SameSiteMode.Strict
            });
        }
    }
}