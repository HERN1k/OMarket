using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;

namespace OMarket.Application.Services.StaticCollections
{
    public class StaticCollectionsService : IStaticCollectionsService
    {
        public FrozenDictionary<TgCommands, Type> CommandsDictionary { get; init; }

        public FrozenDictionary<string, ReadOnlyCollection<string>> AllProductsTypesDictionary { get; init; }

        public FrozenSet<StoreDto> StoresSet { get; init; }

        public FrozenDictionary<string, StoreAddressWithCityDto> CitiesWithStoreAddressesDictionary { get; init; }

        public FrozenDictionary<string, string> GuidToStringProductsTypesDictionary { get; init; }

        public FrozenDictionary<string, string> StringToGuidProductsTypesDictionary { get; init; }

        public FrozenDictionary<string, string> GuidToStringUnderTypesDictionary { get; init; }

        public FrozenDictionary<string, string> StringToGuidUnderTypesDictionary { get; init; }

        public FrozenDictionary<int, string> OrderStatusesDictionary { get; init; }

        public FrozenDictionary<Guid, string> OrderStatusesWithGuidDictionary { get; init; }

        public FrozenDictionary<Guid, ProductFullNameWithPrice> ProductGuidToFullNameWithPriceDictionary { get; init; }

        private readonly IApplicationRepository _appRepository;

        private readonly ILogger<StaticCollectionsService> _logger;

        public StaticCollectionsService(
                IApplicationRepository appRepository,
                ILogger<StaticCollectionsService> logger
            )
        {
            _appRepository = appRepository;
            _logger = logger;

            _logger.LogInformation("Starting initialization static collections...");

            CommandsDictionary = MapCommands();

            AllProductsTypesDictionary = MapAllProductsTypes();

            StoresSet = _appRepository.GetAllStores();

            CitiesWithStoreAddressesDictionary = _appRepository.GetAllCitiesWithStoreAddresses();

            (FrozenDictionary<string, string> GuidToStringTypes, FrozenDictionary<string, string> StringToGuidTypes) =
                _appRepository.GetProductsTypesWithoutInclusions();

            GuidToStringProductsTypesDictionary = GuidToStringTypes;
            StringToGuidProductsTypesDictionary = StringToGuidTypes;

            (FrozenDictionary<string, string> GuidToStringUnderTypes, FrozenDictionary<string, string> StringToGuidUnderTypes) =
                _appRepository.GetProductsUnderTypes();
            GuidToStringUnderTypesDictionary = GuidToStringUnderTypes;
            StringToGuidUnderTypesDictionary = StringToGuidUnderTypes;

            OrderStatusesDictionary = _appRepository.GetAllOrderStatuses();

            OrderStatusesWithGuidDictionary = _appRepository.GetAllOrderStatusesWithGuids();

            ProductGuidToFullNameWithPriceDictionary = _appRepository.GetAllProductGuidWithFullNameAndPrice();

            _logger.LogInformation("Static collections have been successfully initialized.");
        }

        private FrozenDictionary<TgCommands, Type> MapCommands()
        {
            IEnumerable<Type> commandTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetInterfaces().Contains(typeof(ITgCommand)))
                .Where(t => t.GetCustomAttribute<TgCommandAttribute>() != null);

            _logger.LogInformation("Found {Count} command types.", commandTypes.Count());

            Dictionary<TgCommands, Type> commandMap = new();

            foreach (Type type in commandTypes)
            {
                TgCommandAttribute? attribute = type.GetCustomAttribute<TgCommandAttribute>();

                if (attribute is not null)
                {
                    commandMap.TryAdd(attribute.Command, type);
                }
            }

            return commandMap.ToFrozenDictionary();
        }

        private FrozenDictionary<string, ReadOnlyCollection<string>> MapAllProductsTypes()
        {
            List<ProductTypeDto> types = _appRepository.GetProductTypesWithInclusions();

            Dictionary<string, ReadOnlyCollection<string>> typesDictionary = new();

            foreach (var type in types)
            {
                typesDictionary.Add(type.TypeName, type.ProductUnderTypes
                    .Select(e => e.UnderTypeName)
                    .ToArray()
                    .AsReadOnly());
            }

            return typesDictionary.ToFrozenDictionary();
        }
    }
}