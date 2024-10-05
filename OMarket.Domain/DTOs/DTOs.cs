using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Domain.Entities;

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

    public class ProductResponse
    {
        public List<ProductDtoResponse> Products { get; set; } = new();

        public int PageCount { get; set; } = 0;

        public int TotalQuantity { get; set; } = 0;
    }

    public class ProductDtoResponse
    {
        public Guid Id { get; init; }
        public string Name { get; set; } = string.Empty;
        public string PhotoUri { get; set; } = string.Empty;
        public Guid TypeId { get; set; }
        public string Type { get; set; } = string.Empty;
        public Guid UnderTypeId { get; set; }
        public string UnderType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Dimensions { get; set; }
        public string? Description { get; set; }
        public bool Status { get; set; }
    }

    public class ChangeProductDto
    {
        public Guid ProductId { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public string? Dimensions { get; set; }
        public string? Description { get; set; }
        public string PhotoExtension { get; set; } = string.Empty;
    }

    public class ChangeProductMetadata
    {
        public string ProductId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Price { get; set; }
        public string? Dimensions { get; set; }
        public string? Description { get; set; }
    }

    public class AddNewProductDto
    {
        public Guid TypeId { get; set; }
        public Guid UnderTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Dimensions { get; set; }
        public string? Description { get; set; }
        public string PhotoExtension { get; set; } = string.Empty;
    }

    public class AddNewProductMetadata
    {
        public string TypeId { get; set; } = string.Empty;
        public string UnderTypeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string Dimensions { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ProductUnderTypesDto
    {
        public string UnderType { get; set; } = string.Empty;
        public Guid UnderTypeId { get; set; }
    }

    public class ProductTypesDto
    {
        public Guid TypeId { get; set; }
        public string Type { get; set; } = string.Empty;
        public List<ProductUnderTypesDto> UnderTypes { get; set; } = new();
    }

    public class CustomerDtoResponse
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsBot { get; set; }
        public string StoreAddress { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public bool BlockedOrders { get; set; }
        public bool BlockedReviews { get; set; }
    }

    public class ReviewResponse
    {
        public List<ReviewDto> Reviews { get; set; } = new();

        public int PageCount { get; set; } = 0;

        public int TotalQuantity { get; set; } = 0;
    }

    public record ChangeStoreInfoRequest(
        string StoreId,
        string? Address,
        string? PhoneNumber,
        string? Longitude,
        string? Latitude,
        string? TgChatId);

    public record ChangeStoreInfoRequestDto(
        Guid StoreId,
        string? Address,
        string? PhoneNumber,
        decimal? Longitude,
        decimal? Latitude,
        long? TgChatId);

    public record ChangeCityNameRequest(
        string CityId,
        string CityName);

    public record ChangeCityNameRequestDto(
        Guid CityId,
        string CityName);

    public record ChangeAdminPasswordRequest(
        string AdminId,
        string Password);

    public record ChangeAdminPasswordRequestDto(
        Guid AdminId,
        string Password);

    public class AdminDtoResponse : IEquatable<AdminDtoResponse>
    {
        public Guid Id { get; set; }

        public string Login { get; set; } = string.Empty;

        public string Permission { get; set; } = string.Empty;

        public Guid? StoreId { get; set; }

        public string StoreName { get; set; } = string.Empty;

        public long? TgAccountId { get; set; }

        public bool Equals(AdminDtoResponse? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is AdminDtoResponse otherDto)
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

    public record RemoveAdminRequest(
        string AdminId);

    public record RemoveAdminRequestDto(
        Guid AdminId);

    public record AddNewAdminRequest(
        string Login,
        string Password,
        string StoreId);

    public record AddNewAdminRequestDto(
        string Login,
        string Password,
        Guid StoreId);

    public record AddNewCityRequest(
        string CityName);

    public record RemoveCityRequest(
        string CityId);

    public record RemoveCityRequestDto(
        Guid CityId);

    public record RemoveStoreRequest(
        string StoreId);

    public record RemoveStoreRequestDto(
        Guid StoreId);

    public class StoreDtoResponse : IEquatable<StoreDtoResponse>
    {
        public Guid Id { get; set; }

        public Guid CityId { get; set; }

        public Guid? AdminId { get; set; }

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? AdminLogin { get; set; }

        public long? TgChatId { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public bool Equals(StoreDtoResponse? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is StoreDtoResponse otherDto)
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

    public record AddNewStoreRequest(
        string CityId,
        string Address,
        string Latitude,
        string Longitude,
        string PhoneNumber);

    public record AddNewStoreRequestDto(
        Guid CityId,
        string Address,
        decimal Latitude,
        decimal Longitude,
        string PhoneNumber);

    public record RegisterRequest(
        string Login,
        string Password,
        string StoreId,
        string Permission);

    public record RegisterRequestDto(
        string Login,
        string Password,
        Guid StoreId,
        string Permission);

    public record LoginRequest(
        string Login,
        string Password);

    public record LoginResponse(
        string Permission,
        string Login);

    public record ChangePasswordRequest(
        string Password,
        string NewPassword);

    public record TokenClaims(
        string Permission,
        string Login);

    public record ExceptionResult(
        string Status,
        string Message);

    public record ProductFullNameWithPrice(
        string FullName,
        decimal Price);

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

    public class ProductDto : IEquatable<ProductDto>
    {
        public Guid Id { get; init; }

        public string Name { get; set; } = string.Empty;

        public string PhotoUri { get; set; } = string.Empty;

        public Guid TypeId { get; set; }

        public Guid UnderTypeId { get; set; }

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

        public Guid CityId { get; set; }

        public Guid? AdminId { get; set; }

        public long? TgChatId { get; set; }

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

    public class OrderProductDto : IEquatable<OrderProductDto>
    {
        public Guid Id { get; set; }

        public ProductDto Product { get; set; } = null!;

        public int Quantity { get; set; }

        public bool Equals(OrderProductDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is OrderProductDto otherDto)
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

    public class CreatedOrderDto : IEquatable<CreatedOrderDto>
    {
        public Guid Id { get; init; } = IEntity.CreateUuidV7ToGuid();

        public HashSet<OrderProductDto> Products { get; set; } = new();

        public long CustomerId { get; set; }

        public Guid StoreId { get; set; }

        public long TgChatId { get; set; }

        public string Comment { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }

        public int TotalQuantity { get; set; }

        public DeliveryMethod DeliveryMethod { get; set; }

        public bool Equals(CreatedOrderDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is CreatedOrderDto otherDto)
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

    public class OrderItemDto : IEquatable<OrderItemDto>
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }

        public bool Equals(OrderItemDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is OrderItemDto otherDto)
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

    public class OrderDto : IEquatable<OrderDto>
    {
        public Guid Id { get; set; }

        public List<OrderItemDto> Products { get; set; } = new();

        public long CustomerId { get; set; }

        public Guid StoreId { get; set; }

        public decimal TotalAmount { get; set; }

        public Guid StatusId { get; set; }

        //public string DeliveryMethod { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool Equals(OrderDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is OrderDto otherDto)
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

    public class ViewOrderItemDto : IEquatable<ViewOrderItemDto>
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public int Quantity { get; set; }

        public Guid ProductId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal TotalPrice { get; set; }

        public bool Equals(ViewOrderItemDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ViewOrderItemDto otherDto)
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

    public class ViewOrderDto : IEquatable<ViewOrderDto>
    {
        public Guid Id { get; set; }

        public List<ViewOrderItemDto> Products { get; set; } = new();

        public Guid StatusId { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public int TotalQuantity { get; set; }

        public string DeliveryMethod { get; set; } = string.Empty;

        public Guid StoreId { get; set; }

        public StoreAddressWithCityDto Store { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public bool Equals(ViewOrderDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ViewOrderDto otherDto)
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

    public class AdminDto : IEquatable<AdminDto>
    {
        public Guid Id { get; set; }

        public string Login { get; set; } = string.Empty;

        public string Hash { get; set; } = string.Empty;

        public string Permission { get; set; } = string.Empty;

        public Guid? StoreId { get; set; }

        public long? TgAccountId { get; set; }

        public bool Equals(AdminDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is AdminDto otherDto)
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

    public class AdminTokenDto : IEquatable<AdminTokenDto>
    {
        public Guid Id { get; set; }

        public Guid AdminId { get; set; }

        public string RefreshToken { get; set; } = string.Empty;

        public bool Equals(AdminTokenDto? other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is AdminTokenDto otherDto)
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