using System.Collections.Frozen;

using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using OMarket.Domain.DTOs;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

namespace OMarket.Infrastructure.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly IDbContextFactory<AppDBContext> _contextFactory;

        private readonly ILogger<ApplicationRepository> _logger;

        private readonly IDistributedCache _cache;

        private readonly IMapper _mapper;

        public ApplicationRepository(
            IDbContextFactory<AppDBContext> contextFactory,
            ILogger<ApplicationRepository> logger,
            IDistributedCache cache,
            IMapper mapper
          )
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _cache = cache;
            _mapper = mapper;
        }

        public List<ProductTypeDto> GetProductTypesWithInclusions()
        {
            try
            {
                using AppDBContext context = _contextFactory.CreateDbContext();

                return context.ProductTypes
                    .AsNoTracking()
                    .Select(type => new ProductTypeDto()
                    {
                        Id = type.Id,
                        TypeName = type.TypeName,
                        ProductUnderTypes = type.ProductUnderTypes
                            .Select(underTypes => new ProductUnderTypeDto()
                            {
                                Id = underTypes.Id,
                                UnderTypeName = underTypes.UnderTypeName,
                                ProductTypeId = underTypes.ProductTypeId,
                                ProductBrands = underTypes.ProductBrands
                                    .Select(productBrand => new ProductBrandDto()
                                    {
                                        Id = productBrand.Id,
                                        BrandName = productBrand.BrandName,
                                    }).ToList()
                            }).ToList()
                    }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }

        public async Task<List<ProductTypeDto>> GetProductTypesWithInclusionsAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                return await context.ProductTypes
                    .AsNoTracking()
                    .Select(type => new ProductTypeDto()
                    {
                        Id = type.Id,
                        TypeName = type.TypeName,
                        ProductUnderTypes = type.ProductUnderTypes
                            .Select(underTypes => new ProductUnderTypeDto()
                            {
                                Id = underTypes.Id,
                                UnderTypeName = underTypes.UnderTypeName,
                                ProductTypeId = underTypes.ProductTypeId,
                                ProductBrands = underTypes.ProductBrands
                                    .Select(productBrand => new ProductBrandDto()
                                    {
                                        Id = productBrand.Id,
                                        BrandName = productBrand.BrandName,
                                    }).ToList()
                            }).ToList()
                    }).ToListAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }

        public FrozenDictionary<string, StoreAddressWithCityDto> GetAllCitiesWithStoreAddresses()
        {
            try
            {
                using AppDBContext context = _contextFactory.CreateDbContext();

                var result = context.StoreAddresses
                    .AsNoTracking()
                    .Select(storeAddress => new
                    {
                        Guid = storeAddress.Store.Id.ToString(),
                        GuidId = storeAddress.Id,
                        Address = storeAddress.Address,
                        City = storeAddress.Store.City.CityName,
                        Latitude = storeAddress.Latitude,
                        Longitude = storeAddress.Longitude,
                        StoreId = storeAddress.Store.Id
                    }).ToArray();

                return result.ToFrozenDictionary(
                    item => item.Guid,
                    item => new StoreAddressWithCityDto()
                    {
                        Id = item.GuidId,
                        City = item.City,
                        Address = item.Address,
                        Latitude = item.Latitude,
                        Longitude = item.Longitude,
                        StoreId = item.StoreId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }

        public (FrozenDictionary<string, string> GuidToString, FrozenDictionary<string, string> StringToGuid) GetProductsUnderTypes()
        {
            try
            {
                using AppDBContext context = _contextFactory.CreateDbContext();

                var result = context.ProductUnderTypes
                    .AsNoTracking()
                    .Select(underType => new
                    {
                        Guid = underType.Id.ToString(),
                        String = underType.UnderTypeName
                    }).ToArray();

                FrozenDictionary<string, string> GuidToString = result.ToFrozenDictionary(
                    underType => underType.Guid,
                    underType => underType.String);

                FrozenDictionary<string, string> StringToGuid = result.ToFrozenDictionary(
                    underType => underType.String,
                    underType => underType.Guid);

                return (GuidToString, StringToGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }

        public (FrozenDictionary<string, string> GuidToString, FrozenDictionary<string, string> StringToGuid) GetProductsTypesWithoutInclusions()
        {
            try
            {
                using AppDBContext context = _contextFactory.CreateDbContext();

                var result = context.ProductTypes
                    .AsNoTracking()
                    .Select(type => new
                    {
                        Guid = type.Id.ToString(),
                        String = type.TypeName
                    }).ToArray();

                FrozenDictionary<string, string> GuidToString = result.ToFrozenDictionary(
                    underType => underType.Guid,
                    underType => underType.String);

                FrozenDictionary<string, string> StringToGuid = result.ToFrozenDictionary(
                    underType => underType.String,
                    underType => underType.Guid);

                return (GuidToString, StringToGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }

        public FrozenSet<StoreDto> GetAllStores()
        {
            try
            {
                using AppDBContext context = _contextFactory.CreateDbContext();

                return context.Stores
                    .AsNoTracking()
                    .Select(store => new StoreDto()
                    {
                        Id = store.Id,
                        AddressId = store.AddressId,
                        CityId = store.CityId,
                        AdminId = store.AdminId,
                        StoreTelegramChatId = store.StoreTelegramChatId,
                        PhoneNumber = store.PhoneNumber
                    }).ToFrozenSet();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }
    }
}