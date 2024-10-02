using System.Collections.Frozen;

using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface IApplicationRepository
    {
        List<ProductTypeDto> GetProductTypesWithInclusions();

        Task<List<ProductTypeDto>> GetProductTypesWithInclusionsAsync();

        FrozenDictionary<string, StoreAddressWithCityDto> GetAllCitiesWithStoreAddresses();

        Task<FrozenDictionary<string, StoreAddressWithCityDto>> GetAllCitiesWithStoreAddressesAsync();

        (FrozenDictionary<string, string> GuidToString, FrozenDictionary<string, string> StringToGuid) GetProductsUnderTypes();

        Task<(FrozenDictionary<string, string> GuidToString, FrozenDictionary<string, string> StringToGuid)> GetProductsUnderTypesAsync();

        (FrozenDictionary<string, string> GuidToString, FrozenDictionary<string, string> StringToGuid) GetProductsTypesWithoutInclusions();

        Task<(FrozenDictionary<string, string> GuidToString, FrozenDictionary<string, string> StringToGuid)> GetProductsTypesWithoutInclusionsAsync();

        FrozenSet<StoreDto> GetAllStores();

        Task<FrozenSet<StoreDto>> GetAllStoresAsync();

        FrozenDictionary<int, string> GetAllOrderStatuses();

        Task<FrozenDictionary<int, string>> GetAllOrderStatusesAsync();

        FrozenDictionary<Guid, string> GetAllOrderStatusesWithGuids();

        Task<FrozenDictionary<Guid, string>> GetAllOrderStatusesWithGuidsAsync();

        FrozenDictionary<Guid, ProductFullNameWithPrice> GetAllProductGuidWithFullNameAndPrice();
    }
}