using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface IAdminsRepository
    {
        Task SaveNewAdminAsync(RegisterRequestDto request, string hash, CancellationToken token);

        Task<Guid?> VerifyAdminByIdAsync(long id, CancellationToken token);

        Task<AdminDto> GetAdminByLoginAsync(string login, CancellationToken token);

        Task SaveOrUpdateRefreshTokenAsync(string token, Guid adminId, CancellationToken cancellationToken);

        Task RemoveRefreshTokenAsync(string login, CancellationToken token);

        Task RemoveRefreshTokenByTokenValueAsync(string token, CancellationToken cancellationToken);

        Task<string> ValidateLoginAndGetRefreshTokenAsync(string login, CancellationToken token);

        Task ChangePasswordAsync(string login, string hash, CancellationToken token);

        Task RemoveAdminAsync(string superAdminLogin, string password, string removedLogin, CancellationToken token);

        Task AddNewCityAsync(string name, CancellationToken token);

        Task RemoveCityAsync(string name);

        Task<List<CityDto>> GetCitiesAsync(CancellationToken token);

        Task RemoveCityByIdAsync(Guid cityId, CancellationToken token);

        Task<Guid> AddNewStoreAsync(AddNewStoreRequestDto request, CancellationToken token);

        Task RemoveStoreAsync(Guid storeId);

        Task<List<StoreDtoResponse>> GetStoresAsync(CancellationToken token);
    }
}