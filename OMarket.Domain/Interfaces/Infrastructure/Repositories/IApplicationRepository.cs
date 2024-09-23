using System.Collections.Frozen;

using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface IApplicationRepository
    {
        List<ProductTypeDto> GetProductTypesWithInclusions();

        Task<List<ProductTypeDto>> GetProductTypesWithInclusionsAsync(CancellationToken token);

        FrozenDictionary<string, StoreAddressWithCityDto> GetAllCitiesWithStoreAddresses();

        (FrozenDictionary<string, string> GuidToString, FrozenDictionary<string, string> StringToGuid) GetProductsUnderTypes();

        (FrozenDictionary<string, string> GuidToString, FrozenDictionary<string, string> StringToGuid) GetProductsTypesWithoutInclusions();
    }
}