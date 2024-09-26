using OMarket.Domain.Interfaces.Domain.Entities;

namespace OMarket.Domain.Entities
{
    public class Customer : ICustomer
    {
        public long Id { get; init; }

        public string? Username { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public bool IsBot { get; set; }

        public Guid? StoreId { get; set; }

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public bool BlockedOrders { get; set; } = false;

        public bool BlockedReviews { get; set; } = false;

        public virtual Store? Store { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}