using OMarket.Domain.Enums;

using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup
{
    public interface IInlineMarkupService
    {
        InlineKeyboardMarkup Empty { get; }

        InlineKeyboardMarkup SelectStoreAddress(string command, LanguageCode? code = null);

        Task<InlineKeyboardMarkup> MainMenu(CancellationToken token, LanguageCode? code = null);

        InlineKeyboardMarkup MenuProductTypes(LanguageCode? code = null);

        (InlineKeyboardMarkup Markup, string CategoryType) MenuProductUnderTypes(string type, LanguageCode? code = null);

        InlineKeyboardMarkup ProductView(int quantity, LanguageCode? code = null);
    }
}