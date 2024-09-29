using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Cart;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;

using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.SEARCHPRODUCTADDCART)]
    public class SearchProductAddCart : ITgCommand
    {
        private readonly ISendResponseService _response;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly ICartService _cartService;

        public SearchProductAddCart(
                ISendResponseService response,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                ICartService cartService
            )
        {
            _response = response;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
            _cartService = cartService;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            RequestInfo request = await _dataProcessor.MapRequestData(token);

            if (request.Customer.StoreId == null)
            {
                await _response.SendMessageAnswer(
                    text: _i18n.T("main_menu_command_select_your_address"),
                    token: token,
                    buttons: _inlineMarkup.SelectStoreAddress("updatestoreaddress"));

                return;
            }

            string[] queryLines = request.Query.Split('_');

            if (queryLines.Length != 2)
            {
                throw new TelegramException();
            }

            if (!int.TryParse(queryLines[1], out int quantity))
            {
                throw new TelegramException();
            }

            if (!Guid.TryParse(queryLines[0], out Guid productId))
            {
                throw new TelegramException();
            }

            await _cartService.AddProductsToCartAsync(
                customerId: request.Customer.Id,
                quantity: quantity,
                productId: productId,
                token: token);

            await _response.SendCallbackAnswerAlert(_i18n.T("cart_command_successfully_added_to_cart"), token);

            await _response.RemoveLastMessage(token);

            InlineKeyboardMarkup buttons = await _inlineMarkup.MainMenu(token);

            await _response.SendMessageAnswer(_i18n.T("generic_main_manu_title"), token, buttons);
        }
    }
}