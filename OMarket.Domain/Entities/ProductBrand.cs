using OMarket.Domain.Interfaces.Domain.Entities;

namespace OMarket.Domain.Entities
{
    public class ProductBrand : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public string BrandName { get; set; } = string.Empty;

#nullable disable

        public virtual ICollection<ProductUnderType> ProductUnderTypes { get; set; } = new List<ProductUnderType>();

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}