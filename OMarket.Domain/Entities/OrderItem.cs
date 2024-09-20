using OMarket.Domain.Interfaces.Domain.Entities;

#nullable disable

namespace OMarket.Domain.Entities
{
    public class OrderItem : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public virtual Order Order { get; set; }

        public virtual Product Product { get; set; }
    }
}