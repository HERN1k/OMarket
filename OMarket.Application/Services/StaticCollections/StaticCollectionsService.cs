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

        public FrozenDictionary<string, ReadOnlyCollection<string>> AllProductsTypesDictionary { get; private set; }

        public FrozenSet<StoreDto> StoresSet { get; private set; }

        public FrozenDictionary<string, StoreAddressWithCityDto> CitiesWithStoreAddressesDictionary { get; private set; }

        public FrozenDictionary<string, string> GuidToStringProductsTypesDictionary { get; private set; }

        public FrozenDictionary<string, string> StringToGuidProductsTypesDictionary { get; private set; }

        public FrozenDictionary<string, string> GuidToStringUnderTypesDictionary { get; private set; }

        public FrozenDictionary<string, string> StringToGuidUnderTypesDictionary { get; private set; }

        public FrozenDictionary<int, string> OrderStatusesDictionary { get; private set; }

        public FrozenDictionary<Guid, string> OrderStatusesWithGuidDictionary { get; private set; }

        public FrozenDictionary<Guid, ProductFullNameWithPrice> ProductGuidToFullNameWithPriceDictionary { get; private set; }

        private readonly IApplicationRepository _appRepository;

        private readonly ILogger<StaticCollectionsService> _logger;

        private readonly object _lock = new object();

        public StaticCollectionsService(
                IApplicationRepository appRepository,
                ILogger<StaticCollectionsService> logger
            )
        {
            _appRepository = appRepository ?? throw new ArgumentNullException(nameof(appRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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

        public async Task UpdateStaticCollectionsAsync()
        {
            try
            {
                var allProductsTypesDictionary = await MapAllProductsTypesAsync();
                var storesSet = await _appRepository.GetAllStoresAsync();
                var citiesWithStoreAddressesDictionary = await _appRepository
                    .GetAllCitiesWithStoreAddressesAsync();
                (FrozenDictionary<string, string> GuidToStringTypes, FrozenDictionary<string, string> StringToGuidTypes) =
                    await _appRepository.GetProductsTypesWithoutInclusionsAsync();
                (FrozenDictionary<string, string> GuidToStringUnderTypes, FrozenDictionary<string, string> StringToGuidUnderTypes) =
                    await _appRepository.GetProductsUnderTypesAsync();
                var orderStatusesDictionary = await _appRepository.GetAllOrderStatusesAsync();
                var orderStatusesWithGuidDictionary = await _appRepository.GetAllOrderStatusesWithGuidsAsync();

                lock (_lock)
                {
                    AllProductsTypesDictionary = allProductsTypesDictionary;
                    StoresSet = storesSet;
                    CitiesWithStoreAddressesDictionary = citiesWithStoreAddressesDictionary;
                    GuidToStringProductsTypesDictionary = GuidToStringTypes;
                    StringToGuidProductsTypesDictionary = StringToGuidTypes;
                    GuidToStringUnderTypesDictionary = GuidToStringUnderTypes;
                    StringToGuidUnderTypesDictionary = StringToGuidUnderTypes;
                    OrderStatusesDictionary = orderStatusesDictionary;
                    OrderStatusesWithGuidDictionary = orderStatusesWithGuidDictionary;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Static collections critical exception: {Message}\nStackTrace: {StackTrace}", [ex.Message, ex.StackTrace]);
                throw new ApplicationException();
            }
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

        private async Task<FrozenDictionary<string, ReadOnlyCollection<string>>> MapAllProductsTypesAsync()
        {
            List<ProductTypeDto> types = await _appRepository.GetProductTypesWithInclusionsAsync();

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