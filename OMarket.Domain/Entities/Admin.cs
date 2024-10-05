using OMarket.Domain.Interfaces.Domain.Entities;

namespace OMarket.Domain.Entities
{
    public class Admin : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public Guid PermissionId { get; set; }

        public long? TgAccountId { get; set; }

        public Guid? StoreId { get; set; }

        public virtual Store? Store { get; set; }

        public virtual AdminsPermission AdminsPermission { get; set; } = null!;

        public virtual AdminsCredentials AdminsCredentials { get; set; } = null!;
    }
}