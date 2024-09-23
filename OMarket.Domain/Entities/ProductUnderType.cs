using OMarket.Domain.Interfaces.Domain.Entities;

namespace OMarket.Domain.Entities
{
    public class ProductUnderType : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public string UnderTypeName { get; set; } = string.Empty;

        public Guid ProductTypeId { get; set; }

#nullable disable

        public virtual ProductType ProductType { get; set; }

        public virtual ICollection<ProductBrand> ProductBrands { get; set; } = new List<ProductBrand>();

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        public virtual ICollection<DataStoreProduct> DataStoreProducts { get; set; } = new List<DataStoreProduct>();
    }
}