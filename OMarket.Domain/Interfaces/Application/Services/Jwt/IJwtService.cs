using Microsoft.AspNetCore.Http;

using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;

namespace OMarket.Domain.Interfaces.Application.Services.Jwt
{
    public interface IJwtService
    {
        string Generate(AdminDto admin, TokenType type);

        TokenClaims Verify(string token);

        void SetCookies(HttpContext httpContext, string accessToken, string refreshToken);

        void SetAccessTokenInCookies(HttpContext httpContext, string token);

        void RemoveCookies(HttpContext httpContext);
    }
}