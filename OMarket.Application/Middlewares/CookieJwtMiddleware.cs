using Microsoft.AspNetCore.Http;

namespace OMarket.Application.Middlewares
{
    public class CookieJwtMiddleware
    {
        private readonly RequestDelegate _next;

        public CookieJwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue("JwtAccessToken", out var token))
            {
                context.Request.Headers.Authorization = $"Bearer {token}";
            }

            await _next(context);
        }
    }
}