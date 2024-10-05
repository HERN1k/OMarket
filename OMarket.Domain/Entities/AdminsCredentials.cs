using OMarket.Domain.Interfaces.Domain.Entities;

#nullable disable

namespace OMarket.Domain.Entities
{
    public class AdminsCredentials : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public string Login { get; set; } = string.Empty;

        public string Hash { get; set; } = string.Empty;

        public Guid AdminId { get; set; }

        public virtual Admin Admin { get; set; }
    }
}