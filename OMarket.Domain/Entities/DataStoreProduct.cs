using OMarket.Domain.Interfaces.Domain.Entities;

#nullable disable

namespace OMarket.Domain.Entities
{
    public class DataStoreProduct : IEntity
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public Guid ProductId { get; set; }

        public Guid StoreId { get; set; }

        public Guid ProductUnderTypeId { get; set; }

        public bool Status { get; set; }

        public virtual Product Product { get; set; }

        public virtual Store Store { get; set; }

        public ProductUnderType ProductUnderType { get; set; }
    }
}