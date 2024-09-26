using OMarket.Domain.Enums;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OMarket.Domain.DTOs
{
    public record RequestInfo(
            Update Update,
            Message Message,
            string Query,
            CustomerDto CustomerFromUpdate,
            CustomerDto Customer,
            UpdateType UpdateType,
            LanguageCode LanguageCode
        );

    public class CityDto : IEquatable<CityDto>
    {
        public Guid Id { get; set; }

        public string CityName { get; set; } = string.Empty;

        public bool Equals(CityDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is CityDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class StoreAddressDto : IEquatable<StoreAddressDto>
    {
        public Guid Id { get; set; }

        public string Address { get; set; } = string.Empty;

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }


        public bool Equals(StoreAddressDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is StoreAddressDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class StoreAddressWithCityDto : IEquatable<StoreAddressWithCityDto>
    {
        public Guid Id { get; set; }

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public Guid StoreId { get; set; }

        public bool Equals(StoreAddressWithCityDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is StoreAddressWithCityDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class CustomerDto : IEquatable<CustomerDto>
    {
        public long Id { get; set; }

        public string? Username { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public bool IsBot { get; set; }

        public Guid? StoreId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public bool BlockedOrders { get; set; }

        public bool BlockedReviews { get; set; }

        public bool Equals(CustomerDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is CustomerDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class ProductTypeDto : IEquatable<ProductTypeDto>
    {
        public Guid Id { get; set; }

        public string TypeName { get; set; } = string.Empty;

        public List<ProductUnderTypeDto> ProductUnderTypes { get; set; } = new();

        public bool Equals(ProductTypeDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ProductTypeDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class ProductUnderTypeDto : IEquatable<ProductUnderTypeDto>
    {
        public Guid Id { get; set; }

        public string UnderTypeName { get; set; } = string.Empty;

        public Guid ProductTypeId { get; set; }

        public List<ProductBrandDto> ProductBrands { get; set; } = new();

        public bool Equals(ProductUnderTypeDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ProductUnderTypeDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class ProductBrandDto : IEquatable<ProductBrandDto>
    {
        public Guid Id { get; set; }

        public string BrandName { get; set; } = string.Empty;

        public bool Equals(ProductBrandDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ProductBrandDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class ProductDto : IEquatable<ProductDto>
    {
        public Guid Id { get; init; }

        public string Name { get; set; } = string.Empty;

        public string PhotoUri { get; set; } = string.Empty;

        public Guid TypeId { get; set; }

        public Guid UnderTypeId { get; set; }

        public Guid BrandId { get; set; }

        public decimal Price { get; set; }

        public string? Dimensions { get; set; }

        public string? Description { get; set; }

        public bool Status { get; set; }

        public bool Equals(ProductDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ProductDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class ProductWithDbInfoDto : IEquatable<ProductWithDbInfoDto>
    {
        public Guid Id { get; set; }

        public ProductDto? Product { get; set; }

        public string TypeId { get; set; } = null!;

        public int PageNumber { get; set; }

        public int MaxNumber { get; set; }

        public bool Equals(ProductWithDbInfoDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ProductWithDbInfoDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class CartItemDto : IEquatable<CartItemDto>
    {
        public Guid Id { get; set; }

        public ProductDto? Product { get; set; }

        public int Quantity { get; set; }

        public bool Equals(CartItemDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is CartItemDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class StoreDto : IEquatable<StoreDto>
    {
        public Guid Id { get; set; }

        public Guid AddressId { get; set; }

        public Guid CityId { get; set; }

        public Guid AdminId { get; set; }

        public Guid StoreTelegramChatId { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public bool Equals(StoreDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is StoreDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class ReviewDto : IEquatable<ReviewDto>
    {
        public Guid Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public long CustomerId { get; set; }

        public Guid StoreId { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool Equals(ReviewDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ReviewDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class ReviewWithDbInfoDto : IEquatable<ReviewWithDbInfoDto>
    {
        public Guid Id { get; set; }

        public ReviewDto? Review { get; set; }

        public int PageNumber { get; set; }

        public int MaxNumber { get; set; }

        public bool Equals(ReviewWithDbInfoDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ReviewWithDbInfoDto otherDto)
            {
                return Equals(otherDto);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}