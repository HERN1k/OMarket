#nullable disable

namespace OMarket.Domain.Settings
{
    public record WebhookSettings
    {
        public WebhookSettings() { }

        public WebhookSettings(
            string url,
            bool dropPendingUpdates
          )
        {
            ArgumentException.ThrowIfNullOrEmpty(url);
            ArgumentException.ThrowIfNullOrWhiteSpace(url);

            Url = url;
            DropPendingUpdates = dropPendingUpdates;
        }

        public string Url { get; init; }
        public bool DropPendingUpdates { get; init; }
    }

    public record DatabaseInitialDataSettings
    {
        public DatabaseInitialDataSettings() { }

        public DatabaseInitialDataSettings(
                bool initialize,
                List<string> cities,
                List<StoreAddresses> storeAddresses,
                List<string> adminsPermissions,
                List<Admins> admins,
                List<string> orderStatuses,
                List<Stores> stores,
                List<StoreTelegramChats> storeTelegramChats,
                List<string> typesProducts,
                List<string> productUnderTypes,
                List<string> brandsProducts
            )
        {
            if (cities.Count <= 0)
                throw new ArgumentNullException(nameof(cities), "The initial database data is incorrect.");

            if (storeAddresses.Count <= 0)
                throw new ArgumentNullException(nameof(storeAddresses), "The initial database data is incorrect.");

            if (adminsPermissions.Count <= 0)
                throw new ArgumentNullException(nameof(adminsPermissions), "The initial database data is incorrect.");

            if (admins.Count <= 0)
                throw new ArgumentNullException(nameof(admins), "The initial database data is incorrect.");

            if (orderStatuses.Count <= 0)
                throw new ArgumentNullException(nameof(orderStatuses), "The initial database data is incorrect.");

            if (stores.Count <= 0)
                throw new ArgumentNullException(nameof(stores), "The initial database data is incorrect.");

            if (storeTelegramChats.Count <= 0)
                throw new ArgumentNullException(nameof(storeTelegramChats), "The initial database data is incorrect.");

            if (typesProducts.Count <= 0)
                throw new ArgumentNullException(nameof(typesProducts), "The initial database data is incorrect.");

            if (productUnderTypes.Count <= 0)
                throw new ArgumentNullException(nameof(productUnderTypes), "The initial database data is incorrect.");

            if (brandsProducts.Count <= 0)
                throw new ArgumentNullException(nameof(brandsProducts), "The initial database data is incorrect.");

            Initialize = initialize;
            Cities = cities;
            StoreAddresses = storeAddresses;
            AdminsPermissions = adminsPermissions;
            Admins = admins;
            OrderStatuses = orderStatuses;
            Stores = stores;
            StoreTelegramChats = storeTelegramChats;
            TypesProducts = typesProducts;
            ProductUnderTypes = productUnderTypes;
            BrandsProducts = brandsProducts;
        }

        public bool Initialize { get; init; }
        public List<string> Cities { get; init; }
        public List<StoreAddresses> StoreAddresses { get; init; }
        public List<string> AdminsPermissions { get; init; }
        public List<Admins> Admins { get; init; }
        public List<string> OrderStatuses { get; init; }
        public List<Stores> Stores { get; init; }
        public List<StoreTelegramChats> StoreTelegramChats { get; init; }
        public List<string> TypesProducts { get; init; }
        public List<string> ProductUnderTypes { get; init; }
        public List<string> BrandsProducts { get; init; }
    }

    public record StoreAddresses(
            string Address,
            decimal Latitude,
            decimal Longitude
        );

    public record Admins(
            string Address,
            string Permission,
            string Login,
            string Hash
        );

    public record StoreTelegramChats(
            string Address,
            long ChatId
        );

    public record Stores(
            string Address,
            string City,
            string PhoneNumber
        );
}