using System.Collections.Frozen;
using System.Collections.ObjectModel;

using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;

namespace OMarket.Domain.Interfaces.Application.Services.StaticCollections
{
    public interface IStaticCollectionsService
    {
        FrozenDictionary<TgCommands, Type> CommandsDictionary { get; }

        FrozenDictionary<string, ReadOnlyCollection<string>> AllProductsTypesDictionary { get; }

        FrozenSet<StoreDto> StoresSet { get; }

        FrozenDictionary<string, StoreAddressWithCityDto> CitiesWithStoreAddressesDictionary { get; }

        FrozenDictionary<string, string> GuidToStringProductsTypesDictionary { get; }

        FrozenDictionary<string, string> StringToGuidProductsTypesDictionary { get; }

        FrozenDictionary<string, string> GuidToStringUnderTypesDictionary { get; }

        FrozenDictionary<string, string> StringToGuidUnderTypesDictionary { get; }
    }
}