using OMarket.Domain.Interfaces.Domain.Entities;

#nullable disable

namespace OMarket.Domain.Entities
{
    public class Admin : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public Guid PermissionId { get; set; }

        public Guid CredentialsId { get; set; }

        public long? TgAccountId { get; set; }

        public virtual Store Store { get; set; }

        public virtual AdminsPermission AdminsPermission { get; set; }

        public virtual AdminsCredentials AdminsCredentials { get; set; }
    }
}