using System.Text.Json;

using Microsoft.Extensions.Caching.Distributed;

using OMarket.Domain.DTOs;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Cart;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Utilities;

namespace OMarket.Application.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly IDistributedCache _distributedCache;

        private readonly IProductsRepository _productsRepository;

        public CartService(
                IDistributedCache distributedCache,
                IProductsRepository productsRepository
            )
        {
            _distributedCache = distributedCache;
            _productsRepository = productsRepository;
        }

        public async Task<Guid> AddProductsToCartAsync(long customerId, int quantity, Guid productId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (customerId <= 0 || quantity <= 0)
            {
                throw new TelegramException();
            }

            List<CartItemDto> cart;

            string? cartString = await _distributedCache.GetStringAsync($"{CacheKeys.CustomerCartId}{customerId}", token);

            ProductDto product = await _productsRepository.GetProductByIdAsync(productId, token);

            if (string.IsNullOrEmpty(cartString))
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

                cartString = JsonSerializer.Serialize<List<CartItemDto>>(cart);

                await _distributedCache.SetStringAsync($"{CacheKeys.CustomerCartId}{customerId}", cartString, token);

                return product.UnderTypeId;
            }
            else
            {
                cart = JsonSerializer.Deserialize<List<CartItemDto>>(cartString) ?? throw new TelegramException();

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

                cartString = JsonSerializer.Serialize<List<CartItemDto>>(cart);

                await _distributedCache.SetStringAsync($"{CacheKeys.CustomerCartId}{customerId}", cartString, token);

                return product.UnderTypeId;
            }
        }

        public async Task<List<CartItemDto>> GatCustomerCartAsync(long id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (id <= 0)
            {
                throw new TelegramException();
            }

            string? cartString = await _distributedCache.GetStringAsync($"{CacheKeys.CustomerCartId}{id}", token);

            if (string.IsNullOrEmpty(cartString))
            {
                return new();
            }

            return JsonSerializer.Deserialize<List<CartItemDto>>(cartString) ?? new();
        }

        public async Task<List<CartItemDto>> SetQuantityProductAsync(long customerId, Guid productId, int quantity, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (customerId <= 0 || quantity < 0)
            {
                throw new TelegramException();
            }

            List<CartItemDto>? cart;

            string? cartString = await _distributedCache.GetStringAsync($"{CacheKeys.CustomerCartId}{customerId}", token);

            if (string.IsNullOrEmpty(cartString))
            {
                return new();
            }

            cart = JsonSerializer.Deserialize<List<CartItemDto>>(cartString);

            if (cart is null || cart.Count <= 0)
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
                await _distributedCache.RemoveAsync($"{CacheKeys.CustomerCartId}{customerId}", token);

                return new();
            }

            cartString = JsonSerializer.Serialize<List<CartItemDto>>(cart);

            await _distributedCache.SetStringAsync($"{CacheKeys.CustomerCartId}{customerId}", cartString, token);

            return cart;
        }

        public async Task RemoveCartAsync(long customerId, CancellationToken token) =>
            await _distributedCache.RemoveAsync($"{CacheKeys.CustomerCartId}{customerId}", token);
    }
}