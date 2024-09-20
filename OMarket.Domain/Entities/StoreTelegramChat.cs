using OMarket.Domain.Interfaces.Domain.Entities;

#nullable disable

namespace OMarket.Domain.Entities
{
    public class StoreTelegramChat : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public long ChatId { get; set; }

        public virtual Store Store { get; set; }
    }
}