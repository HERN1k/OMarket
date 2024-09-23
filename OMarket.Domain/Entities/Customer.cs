using OMarket.Domain.Interfaces.Domain.Entities;

namespace OMarket.Domain.Entities
{
    public class Customer : ICustomer
    {
        public long Id { get; init; }

        public string? Username { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public Guid? CityId { get; set; }

        public bool IsBot { get; set; }

        public Guid? StoreAddressId { get; set; }

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public virtual City? City { get; set; }

        public virtual StoreAddress? StoreAddress { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}