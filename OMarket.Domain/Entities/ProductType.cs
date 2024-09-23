using OMarket.Domain.Interfaces.Domain.Entities;

namespace OMarket.Domain.Entities
{
    public class ProductType : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public string TypeName { get; set; } = string.Empty;

        public virtual ICollection<ProductUnderType> ProductUnderTypes { get; set; } = new List<ProductUnderType>();

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}