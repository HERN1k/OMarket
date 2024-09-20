using OMarket.Domain.Interfaces.Domain.Entities;

#nullable disable

namespace OMarket.Domain.Entities
{
    public class Order : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public long CustomerId { get; set; }

        public Guid StoreId { get; set; }

        public decimal TotalAmount { get; set; }

        public Guid StatusId { get; set; }

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public virtual Customer Customer { get; set; }

        public virtual Store Store { get; set; }

        public virtual OrderStatus OrderStatus { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}