namespace OMarket.Domain.Interfaces.Application.Services.Cart
{
    public interface ICartService
    {
        Task<Guid> AddProductsToCartAsync(long customerId, int quantity, Guid productId, CancellationToken token);
    }
}