using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface IProductsRepository
    {
        Task<List<ProductDto>> GetProductWithPaginationAsync(int pageNumber, string underType, CancellationToken token);


    }
}