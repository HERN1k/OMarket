using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using OMarket.Domain.DTOs;
using OMarket.Domain.Exceptions.App;
using OMarket.Helpers.Extensions;
using OMarket.Helpers.Utilities;

namespace OMarket.Application.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;

                await context.Response.WriteAsJsonAsync(new ExceptionResult(
                    ExceptionStatus.OperationCanceled, ExceptionStatus.OperationCanceled.FormattedExceptionStatus()));

                return;
            }
            catch (ForbiddenAccessException ex)
            {
                context.Response.StatusCode = StatusCodes.Status200OK;

                await context.Response.WriteAsJsonAsync(new ExceptionResult(
                    ExceptionStatus.ForbiddenAccess, ex.Message ?? ExceptionStatus.ForbiddenAccess.FormattedExceptionStatus()));

                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(new ExceptionResult(
                    ExceptionStatus.UnauthorizedAccess, ex.Message ?? ExceptionStatus.UnauthorizedAccess.FormattedExceptionStatus()));

                return;
            }
            catch (ArgumentNullException ex)
            {
                context.Response.StatusCode = StatusCodes.Status200OK;

                await context.Response.WriteAsJsonAsync(new ExceptionResult(
                    ExceptionStatus.ArgumentNull, ex.Message ?? ExceptionStatus.ArgumentNull.FormattedExceptionStatus()));

                return;
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = StatusCodes.Status200OK;

                await context.Response.WriteAsJsonAsync(new ExceptionResult(
                    ExceptionStatus.Argument, ex.Message ?? ExceptionStatus.Argument.FormattedExceptionStatus()));

                return;
            }
            catch (ApplicationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status200OK;

                await context.Response.WriteAsJsonAsync(new ExceptionResult(
                    ExceptionStatus.Application, ex.Message ?? ExceptionStatus.Application.FormattedExceptionStatus()));

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error occurred: {Message}", ex.Message);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsJsonAsync(new ExceptionResult(
                    ExceptionStatus.Critical, ExceptionStatus.Critical.FormattedExceptionStatus()));

                return;
            }
        }
    }
}