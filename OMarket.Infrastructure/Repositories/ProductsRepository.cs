﻿using System.Text.Json;

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

        private readonly int _pageSize = 1;

        public ProductsRepository(
                IDbContextFactory<AppDBContext> contextFactory,
                IStaticCollectionsService staticCollections,
                ILogger<CustomersRepository> logger,
                IDistributedCache cache
            )
        {
            _contextFactory = contextFactory;
            _staticCollections = staticCollections;
            _logger = logger;
            _cache = cache;
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            ProductDto? product;

            string? productsString = await _cache.GetStringAsync($"{CacheKeys.ProductItemFromDbId}{id}", token);

            if (!string.IsNullOrEmpty(productsString))
            {
                token.ThrowIfCancellationRequested();

                product = JsonSerializer.Deserialize<ProductDto>(productsString);

                return product ?? throw new TelegramException();
            }

            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                product = await context.Products
                    .Where(product => product.Id == id)
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
                    })
                    .SingleOrDefaultAsync(token)
                        ?? throw new TelegramException();

                productsString = JsonSerializer.Serialize<ProductDto>(product);

                if (string.IsNullOrEmpty(productsString))
                {
                    throw new TelegramException();
                }

                await _cache.SetStringAsync($"{CacheKeys.ProductItemFromDbId}{id}", productsString, token);

                return product;
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

        public async Task<ProductWithDbInfoDto?> GetProductWithPaginationAsync(int pageNumber, string underType, Guid storeId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (pageNumber <= 0 || storeId == Guid.Empty)
            {
                throw new TelegramException();
            }

            ProductWithDbInfoDto? product;

            string? productsString = await _cache.GetStringAsync($"{CacheKeys.ProductId}{storeId}-{underType}-{_pageSize}-{pageNumber}", token);

            if (!string.IsNullOrEmpty(productsString))
            {
                token.ThrowIfCancellationRequested();

                product = JsonSerializer.Deserialize<ProductWithDbInfoDto>(productsString);

                return product ?? throw new TelegramException();
            }

            if (!Guid.TryParse(underType, out Guid underTypeGuid) || !_staticCollections.GuidToStringUnderTypesDictionary.ContainsKey(underType))
            {
                throw new TelegramException();
            }

            if (underTypeGuid == Guid.Empty)
            {
                throw new TelegramException();
            }

            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                var products = await (
                    from productTemp in context.Products.AsNoTracking()
                    join dataStoreProduct in context.DataStoreProducts.AsNoTracking()
                    on new { ProductId = productTemp.Id, StoreId = storeId } equals new { dataStoreProduct.ProductId, dataStoreProduct.StoreId }
                    into storeGroup
                    from storeProduct in storeGroup.DefaultIfEmpty()
                    where productTemp.UnderTypeId == underTypeGuid
                    orderby productTemp.Price
                    select new ProductDto
                    {
                        Id = productTemp.Id,
                        Name = productTemp.Name,
                        PhotoUri = productTemp.PhotoUri,
                        TypeId = productTemp.TypeId,
                        UnderTypeId = productTemp.UnderTypeId,
                        BrandId = productTemp.BrandId,
                        Price = productTemp.Price,
                        Dimensions = productTemp.Dimensions,
                        Description = productTemp.Description,
                        Status = storeProduct != null && storeProduct.Status
                    })
                    .Skip((pageNumber - 1) * _pageSize)
                    .Take(_pageSize)
                    .ToListAsync(token);

                int maxPageNumber = await context.Products
                    .AsNoTracking()
                    .Where(product => product.UnderTypeId == underTypeGuid)
                    .CountAsync(token);

                product = products
                    .Select(product => new ProductWithDbInfoDto()
                    {
                        Id = product.Id,
                        Product = product,
                        TypeId = product.TypeId.ToString(),
                        PageNumber = pageNumber,
                        MaxNumber = maxPageNumber
                    })
                    .ToArray()
                    .ElementAtOrDefault(0);

                if (product is null)
                {
                    return null;
                }

                productsString = JsonSerializer.Serialize<ProductWithDbInfoDto>(product);

                if (string.IsNullOrEmpty(productsString))
                {
                    throw new TelegramException();
                }

                await _cache.SetStringAsync($"{CacheKeys.ProductId}{storeId}-{underType}-{_pageSize}-{pageNumber}", productsString, token);

                return product;
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

        public async Task<List<ProductDto>> GetProductsByNameAsync(string name, Guid productTypeId, Guid storeId, CancellationToken token)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new TelegramException();
            }

            if (productTypeId == Guid.Empty || storeId == Guid.Empty)
            {
                throw new TelegramException();
            }

            List<ProductDto>? products;
            string cacheKey = $"{CacheKeys.SearchProductsByNameId}{productTypeId}{storeId}{name}";

            string? productsString = await _cache.GetStringAsync(cacheKey, token);

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

                string normalizedName = name.ToLower();

                products = await (
                    from productTemp in context.Products
                    join dataStoreProduct in context.DataStoreProducts
                    on new { ProductId = productTemp.Id, StoreId = storeId } equals new { dataStoreProduct.ProductId, dataStoreProduct.StoreId }
                    into storeGroup
                    from storeProduct in storeGroup.DefaultIfEmpty()
#pragma warning disable CA1862
                    where productTemp.Name.ToLower().Contains(normalizedName)
#pragma warning restore CA1862
                    orderby productTemp.Price
                    select new ProductDto
                    {
                        Id = productTemp.Id,
                        Name = productTemp.Name,
                        PhotoUri = productTemp.PhotoUri,
                        TypeId = productTemp.TypeId,
                        UnderTypeId = productTemp.UnderTypeId,
                        BrandId = productTemp.BrandId,
                        Price = productTemp.Price,
                        Dimensions = productTemp.Dimensions,
                        Description = productTemp.Description,
                        Status = storeProduct != null && storeProduct.Status
                    })
                    .Take(5)
                    .ToListAsync(token);

                productsString = JsonSerializer.Serialize<List<ProductDto>>(products);

                if (string.IsNullOrEmpty(productsString))
                {
                    throw new TelegramException();
                }

                await _cache.SetStringAsync(cacheKey, productsString, token);

                return products;
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

        public async Task<bool> CheckingAvailabilityOfProductsInTheStore(List<CartItemDto> cart, Guid storeId, CancellationToken token)
        {
            if (cart.Count <= 0 || storeId == Guid.Empty)
            {
                return false;
            }

            if (cart.Any(item => item.Product == null))
            {
                return false;
            }

            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                foreach (var item in cart)
                {
                    if (item.Product is null || item.Quantity <= 0)
                    {
                        return false;
                    }
                }

                List<Guid> productIds = cart
                    .Select(item => item.Product!.Id)
                    .ToList();

                if (productIds.Count == 0)
                {
                    return false;
                }

                var availableProductIds = await context.DataStoreProducts
                    .AsNoTracking()
                    .Where(product =>
                        product.StoreId == storeId &&
                        product.Status == true &&
                        productIds.Contains(product.ProductId))
                    .Select(product => product.ProductId)
                    .ToListAsync(token);

                return productIds.All(id => availableProductIds.Contains(id));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                return false;
            }
        }
    }
}