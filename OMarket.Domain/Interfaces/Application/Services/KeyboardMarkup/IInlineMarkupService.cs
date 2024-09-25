using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;

using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup
{
    public interface IInlineMarkupService
    {
        InlineKeyboardMarkup Empty { get; }

        InlineKeyboardMarkup ToMainMenuBack(LanguageCode? code = null);

        InlineKeyboardMarkup ToMainMenuBackDel(LanguageCode? code = null);

        InlineKeyboardMarkup SelectStoreAddress(string command, LanguageCode? code = null);

        Task<InlineKeyboardMarkup> MainMenu(CancellationToken token, LanguageCode? code = null);

        InlineKeyboardMarkup MenuProductTypes(LanguageCode? code = null);

        (InlineKeyboardMarkup Markup, string CategoryType) MenuProductUnderTypes(string type, LanguageCode? code = null);

        InlineKeyboardMarkup ProductView(ProductWithDbInfoDto dto, int quantity, LanguageCode? code = null);

        InlineKeyboardMarkup Cart(LanguageCode? code = null);

        InlineKeyboardMarkup EditCart(List<CartItemDto> cart, LanguageCode? code = null);

        InlineKeyboardMarkup CartIsEmpty(LanguageCode? code = null);

        InlineKeyboardMarkup SelectProductTypeForCustomerSearchChoice(LanguageCode? code = null);

        InlineKeyboardMarkup EndSearchProducts(List<ProductDto> products, LanguageCode? code = null);

        InlineKeyboardMarkup ProductViewBySearch(ProductDto product, int quantity, LanguageCode? code = null);

        InlineKeyboardMarkup SelectStoreAddressWithLocation(LanguageCode? code = null);

        InlineKeyboardMarkup SelectStoreAddressWithContacts(LanguageCode? code = null);
    }
}