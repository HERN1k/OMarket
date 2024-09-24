using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Application.Services.Cart
{
    public interface ICartService
    {
        Task<Guid> AddProductsToCartAsync(long customerId, int quantity, Guid productId, CancellationToken token);

        Task<List<CartItemDto>> GatCustomerCartAsync(long id, CancellationToken token);

        Task<List<CartItemDto>> SetQuantityProductAsync(long customerId, Guid productId, int quantity, CancellationToken token);
    }
}