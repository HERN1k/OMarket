using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface IReviewRepository
    {
        Task AddNewReviewAsync(long id, Guid storeId, string text, CancellationToken token);

        Task<ReviewWithDbInfoDto> GetReviewWithPaginationAsync(int pageNumber, Guid storeId, CancellationToken token);
    }
}