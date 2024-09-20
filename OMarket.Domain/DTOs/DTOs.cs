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

    public class CityDto
    {
        public Guid Id { get; set; }

        public string CityName { get; set; } = string.Empty;
    }

    public class CustomerDto
    {
        public long Id { get; set; }

        public string? Username { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public Guid? CityId { get; set; }

        public bool IsBot { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}