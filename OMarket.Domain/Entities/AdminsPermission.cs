using OMarket.Domain.Interfaces.Domain.Entities;

namespace OMarket.Domain.Entities
{
    public class AdminsPermission : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public string Permission { get; set; } = string.Empty;

        public virtual ICollection<Admin> Admins { get; set; } = new HashSet<Admin>();
    }
}