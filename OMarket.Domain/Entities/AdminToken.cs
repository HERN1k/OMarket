using OMarket.Domain.Interfaces.Domain.Entities;

namespace OMarket.Domain.Entities
{
    public class AdminToken : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public Guid AdminId { get; set; }

        public string RefreshToken { get; set; } = string.Empty;
    }
}
