using OMarket.Domain.Interfaces.Domain.Entities;

namespace OMarket.Domain.Entities
{
    public class Review : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public string Text { get; set; } = string.Empty;

        public long CustomerId { get; set; }

        public Guid StoreId { get; set; }

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

#nullable disable
        public virtual Customer Customer { get; set; }

        public virtual Store Store { get; set; }
    }
}