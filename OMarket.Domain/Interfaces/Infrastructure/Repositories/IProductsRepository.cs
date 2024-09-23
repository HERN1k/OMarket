using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface IProductsRepository
    {
        Task<ProductDto> GetProductByIdAsync(Guid id, CancellationToken token);

        Task<ProductWithDbInfoDto?> GetProductWithPaginationAsync(int pageNumber, string underType, CancellationToken token);
    }
}