using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface ICityRepository
    {
        Task<List<CityDto>> GetAllCitiesAsync(CancellationToken token);
    }
}