using OMarket.Domain.Interfaces.Domain.Entities;

namespace OMarket.Domain.Entities
{
    public class Product : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public string Name { get; set; } = string.Empty;

        public string PhotoUri { get; set; } = string.Empty;

        public Guid TypeId { get; set; }

        public Guid UnderTypeId { get; set; }

        public decimal Price { get; set; }

        public string? Dimensions { get; set; }

        public string? Description { get; set; }

#nullable disable

        public virtual ProductType ProductType { get; set; }

        public virtual ProductUnderType ProductUnderType { get; set; }

        public virtual ICollection<DataStoreProduct> DataStoreProducts { get; set; } = new List<DataStoreProduct>();

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}