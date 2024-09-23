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

                cart.Add(new CartItemDto() // добавить проверку на количество одинаковых продуктов
                {
                    Id = product.Id,
                    Product = product,
                    Quantity = quantity,
                });

                cartString = JsonSerializer.Serialize<List<CartItemDto>>(cart);

                await _distributedCache.SetStringAsync($"{CacheKeys.CustomerCartId}{customerId}", cartString, token);

                return product.UnderTypeId;
            }
        }
    }
}