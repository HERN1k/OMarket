using OMarket.Domain.Interfaces.Domain.Entities;

namespace OMarket.Domain.Entities
{
    public class ProductUnderType : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public string UnderTypeName { get; set; } = string.Empty;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
