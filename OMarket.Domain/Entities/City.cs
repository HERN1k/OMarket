using OMarket.Domain.Interfaces.Domain.Entities;

namespace OMarket.Domain.Entities
{
    public class City : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public string CityName { get; set; } = string.Empty;

        public virtual ICollection<Store> Stores { get; set; } = new HashSet<Store>();
    }
}