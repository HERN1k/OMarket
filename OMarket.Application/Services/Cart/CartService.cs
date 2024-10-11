using OMarket.Domain.DTOs;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Cache;
using OMarket.Domain.Interfaces.Application.Services.Cart;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Utilities;

namespace OMarket.Application.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly ICacheService _cache;

        private readonly IProductsRepository _productsRepository;

        public CartService(
                ICacheService cache,
                IProductsRepository productsRepository
            )
        {
            _cache = cache;
            _productsRepository = productsRepository;
        }

        public async Task<Guid> AddProductsToCartAsync(long customerId, int quantity, Guid productId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (quantity <= 0)
            {
                throw new TelegramException();
            }

            List<CartItemDto>? cart;

            string cacheKey = $"{CacheKeys.CustomerCartId}{customerId}";

            cart = await _cache.GetCacheAsync<List<CartItemDto>>(cacheKey);

            ProductDto product = await _productsRepository.GetProductByIdAsync(productId, token);

            if (cart is null)
            {
                cart = new()
                {
                    new CartItemDto()
                    {
                        Id = product.Id,
                        Product = product,
                        Quantity = quantity,
                    }
                };

                await _cache.SetCacheAsync(cacheKey, cart);

                return product.UnderTypeId;
            }
            else
            {
                CartItemDto? item = cart.SingleOrDefault(e => e.Id == product.Id);

                if (item is not null)
                {
                    item.Quantity += quantity;
                }
                else
                {
                    cart.Add(new CartItemDto()
                    {
                        Id = product.Id,
                        Product = product,
                        Quantity = quantity,
                    });
                }

                await _cache.SetCacheAsync(cacheKey, cart);

                return product.UnderTypeId;
            }
        }

        public async Task<List<CartItemDto>> GatCustomerCartAsync(long id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            List<CartItemDto>? cart = await _cache.GetCacheAsync<List<CartItemDto>>($"{CacheKeys.CustomerCartId}{id}");

            if (cart is null)
            {
                return new();
            }

            return cart;
        }

        public async Task<List<CartItemDto>> SetQuantityProductAsync(long customerId, Guid productId, int quantity, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (quantity < 0)
            {
                throw new TelegramException();
            }

            List<CartItemDto>? cart;

            string cacheKey = $"{CacheKeys.CustomerCartId}{customerId}";

            cart = await _cache.GetCacheAsync<List<CartItemDto>>(cacheKey);

            if (cart is null)
            {
                return new();
            }

            if (cart.Count <= 0)
            {
                return new();
            }

            CartItemDto item = cart.SingleOrDefault(e => e.Id == productId)
                ?? throw new TelegramException();

            if (quantity > 0)
            {
                item.Quantity = quantity;
            }
            else
            {
                cart.Remove(item);
            }

            if (cart.Count <= 0)
            {
                await _cache.RemoveCacheAsync(cacheKey);

                return new();
            }

            await _cache.SetCacheAsync(cacheKey, cart);

            return cart;
        }

        public async Task RemoveCartAsync(long customerId, CancellationToken token)
        {
            await _cache.RemoveCacheAsync($"{CacheKeys.CustomerCartId}{customerId}");
        }
    }
}