using OMarket.Domain.Interfaces.Domain.Entities;

#nullable disable

namespace OMarket.Domain.Entities
{
    public class Store : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public Guid AddressId { get; set; }

        public Guid CityId { get; set; }

        public Guid AdminId { get; set; }

        public long? TgChatId { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public virtual StoreAddress Address { get; set; }

        public virtual City City { get; set; }

        public virtual Admin Admin { get; set; }

        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        public virtual ICollection<DataStoreProduct> DataStoreProducts { get; set; } = new List<DataStoreProduct>();

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}