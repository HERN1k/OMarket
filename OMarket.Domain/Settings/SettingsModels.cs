#nullable disable

namespace OMarket.Domain.Settings
{
    public record WebhookSettings
    {
        public WebhookSettings() { }

        public WebhookSettings(
            bool dropPendingUpdates
          )
        {
            DropPendingUpdates = dropPendingUpdates;
        }

        public string Url { get; init; }
        public bool DropPendingUpdates { get; init; }
    }

    public record DatabaseInitialDataSettings
    {
        public DatabaseInitialDataSettings() { }

        public DatabaseInitialDataSettings(
                List<string> adminsPermissions,
                List<Admins> admins,
                List<string> orderStatuses,
                List<string> typesProducts,
                Dictionary<string, List<string>> productUnderTypes
            )
        {
            if (adminsPermissions.Count <= 0)
                throw new ArgumentNullException(nameof(adminsPermissions), "The initial database data is incorrect.");

            if (admins.Count <= 0)
                throw new ArgumentNullException(nameof(admins), "The initial database data is incorrect.");

            if (orderStatuses.Count <= 0)
                throw new ArgumentNullException(nameof(orderStatuses), "The initial database data is incorrect.");

            if (typesProducts.Count <= 0)
                throw new ArgumentNullException(nameof(typesProducts), "The initial database data is incorrect.");

            if (productUnderTypes.Count <= 0)
                throw new ArgumentNullException(nameof(productUnderTypes), "The initial database data is incorrect.");

            AdminsPermissions = adminsPermissions;
            Admins = admins;
            OrderStatuses = orderStatuses;
            TypesProducts = typesProducts;
            ProductUnderTypes = productUnderTypes;
        }

        public List<string> AdminsPermissions { get; init; }
        public List<Admins> Admins { get; init; }
        public List<string> OrderStatuses { get; init; }
        public List<string> TypesProducts { get; init; }
        public Dictionary<string, List<string>> ProductUnderTypes { get; init; }
    }

    public record Admins(
            string Login,
            string Hash
        );

    public record JwtSettings
    {
        public JwtSettings() { }

        public JwtSettings(
                string issuer,
                string audience,
                int clockSkewSeconds,
                int expiresInMinutesAccess,
                int expiresInMinutesRefresh
        )
        {
            ArgumentException.ThrowIfNullOrEmpty(issuer);
            ArgumentException.ThrowIfNullOrWhiteSpace(issuer);

            ArgumentException.ThrowIfNullOrEmpty(audience);
            ArgumentException.ThrowIfNullOrWhiteSpace(audience);

            Issuer = issuer;
            Audience = audience;
            ClockSkewSeconds = clockSkewSeconds;
            ExpiresInMinutesAccess = expiresInMinutesAccess;
            ExpiresInMinutesRefresh = expiresInMinutesRefresh;
        }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int ClockSkewSeconds { get; set; }

        public int ExpiresInMinutesAccess { get; set; }

        public int ExpiresInMinutesRefresh { get; set; }
    }
}