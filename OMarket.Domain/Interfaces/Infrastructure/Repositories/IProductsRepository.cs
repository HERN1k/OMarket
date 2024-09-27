using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface IProductsRepository
    {
        Task<ProductDto> GetProductByIdAsync(Guid id, CancellationToken token);

        Task<ProductWithDbInfoDto?> GetProductWithPaginationAsync(int pageNumber, string underType, Guid storeId, CancellationToken token);

        Task<List<ProductDto>> GetProductsByNameAsync(string name, Guid productTypeId, Guid storeId, CancellationToken token);

        Task<bool> CheckingAvailabilityOfProductsInTheStore(List<CartItemDto> cart, Guid storeId, CancellationToken token);
    }
}