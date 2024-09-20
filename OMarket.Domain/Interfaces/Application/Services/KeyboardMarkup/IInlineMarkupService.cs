using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;

using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup
{
    public interface IInlineMarkupService
    {
        InlineKeyboardMarkup Empty { get; }

        InlineKeyboardMarkup SelectCity(List<CityDto> cities, LanguageCode? code = null);

        Task<InlineKeyboardMarkup> MainMenu(CancellationToken token, LanguageCode? code = null);

        InlineKeyboardMarkup CatalogMenu(LanguageCode? code = null);
    }
}