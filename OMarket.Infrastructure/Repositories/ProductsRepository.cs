using System.Text.Json;

using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using OMarket.Domain.DTOs;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Utilities;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

namespace OMarket.Infrastructure.Repositories
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly IDbContextFactory<AppDBContext> _contextFactory;

        private readonly IStaticCollectionsService _staticCollections;

        private readonly ILogger<CustomersRepository> _logger;

        private readonly IDistributedCache _cache;

        private readonly IMapper _mapper;

        private readonly int _pageSize = 1;

        public ProductsRepository(
                IDbContextFactory<AppDBContext> contextFactory,
                IStaticCollectionsService staticCollections,
                ILogger<CustomersRepository> logger,
                IDistributedCache cache,
                IMapper mapper
            )
        {
            _contextFactory = contextFactory;
            _staticCollections = staticCollections;
            _logger = logger;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<List<ProductDto>> GetProductWithPaginationAsync(int pageNumber, string underType, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (pageNumber <= 0)
            {
                throw new TelegramException();
            }

            if (!Guid.TryParse(underType, out Guid guid) || !_staticCollections.GuidToStringUnderTypesDictionary.ContainsKey(underType))
            {
                throw new TelegramException();
            }

            List<ProductDto>? products;

            string? productsString = await _cache.GetStringAsync($"{CacheKeys.ProductId}{underType}-{_pageSize}-{pageNumber}", token);

            if (!string.IsNullOrEmpty(productsString))
            {
                token.ThrowIfCancellationRequested();

                products = JsonSerializer.Deserialize<List<ProductDto>>(productsString);

                return products ?? throw new TelegramException();
            }

            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                var result = await context.Products
                    .AsNoTracking()
                    .Where(product => product.UnderTypeId == guid)
                    .OrderBy(product => product.Price)
                    .Skip((pageNumber - 1) * _pageSize)
                    .Take(_pageSize)
                    .Select(product => new ProductDto()
                    {
                        Id = product.Id,
                        Name = product.Name,
                        PhotoUri = product.PhotoUri,
                        TypeId = product.TypeId,
                        UnderTypeId = product.UnderTypeId,
                        BrandId = product.BrandId,
                        Price = product.Price,
                        Dimensions = product.Dimensions,
                        Description = product.Description
                    }).ToListAsync(token);

                productsString = JsonSerializer.Serialize<List<ProductDto>>(result);

                if (string.IsNullOrEmpty(productsString))
                {
                    throw new TelegramException();
                }

                await _cache.SetStringAsync($"{CacheKeys.ProductId}{underType}-{_pageSize}-{pageNumber}", productsString, token);

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (TelegramException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }


    }
}