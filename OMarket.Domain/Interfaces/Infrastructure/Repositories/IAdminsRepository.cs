using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface IAdminsRepository
    {
        Task SaveNewAdminAsync(RegisterRequestDto request, string hash, CancellationToken token);

        Task<AdminDto> GetAdminByLoginAsync(string login, CancellationToken token);

        Task SaveOrUpdateRefreshTokenAsync(string token, Guid adminId, CancellationToken cancellationToken);

        Task RemoveRefreshTokenAsync(string login, CancellationToken token);

        Task RemoveRefreshTokenForLogoutAsync(string login, CancellationToken token);

        Task RemoveRefreshTokenByTokenValueAsync(string token, CancellationToken cancellationToken);

        Task RemoveRefreshTokenByIdAsync(Guid adminId, CancellationToken token);

        Task<string> ValidateLoginAndGetRefreshTokenAsync(string login, CancellationToken token);

        Task ChangePasswordAsync(string login, string hash, CancellationToken token);

        Task AddNewCityAsync(string name, CancellationToken token);

        Task RemoveCityAsync(string name);

        Task<List<CityDto>> GetCitiesAsync(CancellationToken token);

        Task RemoveCityByIdAsync(Guid cityId, CancellationToken token);

        Task<Guid> AddNewStoreAsync(AddNewStoreRequestDto request, CancellationToken token);

        Task RemoveStoreAsync(Guid storeId);

        Task<List<StoreDtoResponse>> GetStoresAsync(CancellationToken token);

        Task RemoveAdminAsync(Guid adminId);

        Task<Guid> AddNewAdminAsync(AddNewAdminRequestDto request, CancellationToken token);

        Task<List<AdminDtoResponse>> GetAdminsAsync(CancellationToken token);

        Task ChangeAdminPasswordAsync(Guid adminId, string hash, CancellationToken token);

        Task ChangeCityNameAsync(ChangeCityNameRequestDto request, CancellationToken token);

        Task ChangeStoreInfoAsync(ChangeStoreInfoRequestDto request, CancellationToken token);

        Task<ReviewResponse> GetStoreReviewWithPagination(Guid storeId, int page, CancellationToken token);

        Task RemoveReviewAsync(Guid reviewId);

        Task BlockReviewsAsync(long customerId, CancellationToken token);

        Task UnBlockReviewsAsync(long customerId, CancellationToken token);

        Task BlockOrdersAsync(long customerId, CancellationToken token);

        Task UnBlockOrdersAsync(long customerId, CancellationToken token);

        Task<CustomerDtoResponse?> GetCustomerByIdAsync(long customerId, CancellationToken token);

        Task<CustomerDtoResponse?> GetCustomerByPhoneNumberAsync(string phoneNumber, CancellationToken token);

        Task<List<ProductTypesDto>> ProductTypesAsync(CancellationToken token);

        Task RemoveProductByExceptionAsync(Guid productId);

        Task<Guid> CreateNewProductAsync(AddNewProductDto request, CancellationToken token);

        Task<Guid> ChangeProductAsync(ChangeProductDto request, CancellationToken token);

        Task<string> RemoveProductAsync(Guid productId);

        Task<ProductResponse> GetProductsWithPaginationAsync(Guid typeId, int page, CancellationToken token);

        Task<ProductResponse> GetProductsWithPaginationAndStoreIdAsync(Guid storeId, Guid typeId, int page, CancellationToken token);

        Task ChangeDataStoreProductStatusAsync(Guid storeId, Guid productId, CancellationToken token);
    }
}