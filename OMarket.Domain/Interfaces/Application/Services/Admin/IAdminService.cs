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

        Task AddNewCityAsync(AddNewCityRequest request, CancellationToken token);

        Task<List<CityDto>> GetCitiesAsync(CancellationToken token);

        Task RemoveCityAsync(RemoveCityRequest request, CancellationToken token);

        Task AddNewStoreAsync(AddNewStoreRequest request, CancellationToken token);

        Task<List<StoreDtoResponse>> GetStoresAsync(CancellationToken token);

        Task RemoveStoreAsync(RemoveStoreRequest request, CancellationToken token);

        Task AddNewAdminAsync(AddNewAdminRequest request, CancellationToken token);

        Task RemoveAdminAsync(RemoveAdminRequest request, CancellationToken token);

        Task<List<AdminDtoResponse>> AdminsAsync(CancellationToken token);

        Task ChangeAdminPasswordAsync(ChangeAdminPasswordRequest request, CancellationToken token);

        Task ChangeCityNameAsync(ChangeCityNameRequest request, CancellationToken token);

        Task ChangeStoreInfoAsync(ChangeStoreInfoRequest request, CancellationToken token);

        Task<ReviewResponse> StoreReviewAsync(Guid storeId, int page, CancellationToken token);

        Task RemoveStoreReviewAsync(Guid reviewId, CancellationToken token);

        Task BlockReviewsAsync(long customerId, CancellationToken token);

        Task UnBlockReviewsAsync(long customerId, CancellationToken token);

        Task BlockOrdersAsync(long customerId, CancellationToken token);

        Task UnBlockOrdersAsync(long customerId, CancellationToken token);

        Task<CustomerDtoResponse?> GetCustomerByIdAsync(long customerId, CancellationToken token);

        Task<CustomerDtoResponse?> GetCustomerByPhoneNumberAsync(string phoneNumber, CancellationToken token);
    }
}