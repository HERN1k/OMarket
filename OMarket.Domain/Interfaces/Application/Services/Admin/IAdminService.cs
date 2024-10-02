using Microsoft.AspNetCore.Http;

using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Application.Services.Admin
{
    public interface IAdminService
    {
        Task RegisterAsync(RegisterRequest request, CancellationToken token);

        Task<LoginResponse> LoginAsync(LoginRequest request, HttpContext httpContext, CancellationToken token);

        Task LogoutAsync(HttpContext httpContext, CancellationToken token);

        Task RefreshTokenAsync(HttpContext httpContext, CancellationToken token);

        Task ChangePasswordAsync(ChangePasswordRequest request, HttpContext httpContext, CancellationToken token);

        Task RemoveAdminAsync(RemoveAdminRequest request, HttpContext httpContext, CancellationToken token);

        Task AddNewCityAsync(AddNewCityRequest request, CancellationToken token);

        Task<List<CityDto>> GetCitiesAsync(CancellationToken token);

        Task RemoveCityAsync(RemoveCityRequest request, CancellationToken token);

        Task AddNewStoreAsync(AddNewStoreRequest request, CancellationToken token);

        Task<List<StoreDtoResponse>> GetStoresAsync(CancellationToken token);
    }
}