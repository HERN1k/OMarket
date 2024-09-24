using System.Text;

using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Cart;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;

using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.EDITCART)]
    public class EditCart : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly ICartService _cartService;

        public EditCart(
                IUpdateManager updateManager,
                ISendResponseService response,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                ICartService cartService
            )
        {
            _updateManager = updateManager;
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

            if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                await _response.SendCallbackAnswer(token);
            }

            if (string.IsNullOrEmpty(request.Query))
            {
                List<CartItemDto> cart = await _cartService
                    .GatCustomerCartAsync(request.Customer.Id, token);

                if (cart.Count == 0)
                {
                    await _response.SendCallbackAnswerAlert(_i18n.T("cart_command_your_cart_is_empty"), token);

                    return;
                }

                int index = 0;
                StringBuilder sb = new();

                sb.AppendLine(_i18n.T("edit_cart_command_editing_the_cart"));
                sb.AppendLine();
                foreach (var item in cart)
                {
                    index++;

                    sb.AppendLine($"<b>№ {index}</b>: <b>{item.Product?.Name}</b><i>, {item.Product?.Dimensions}</i>");
                    sb.AppendLine();
                }

                await _response.EditLastMessage(sb.ToString(), token, _inlineMarkup.EditCart(cart));

                return;
            }
            else
            {
                string[] queryLines = request.Query.Split('_');

                if (queryLines.Length != 2)
                {
                    throw new TelegramException();
                }

                if (!Guid.TryParse(queryLines[0], out Guid productId))
                {
                    throw new TelegramException();
                }

                if (!int.TryParse(queryLines[1], out int newQuantity))
                {
                    throw new TelegramException();
                }

                List<CartItemDto> cart = await _cartService
                    .SetQuantityProductAsync(request.Customer.Id, productId, newQuantity, token);

                if (cart.Count == 0)
                {
                    string text = $"""
                        {_i18n.T("edit_cart_command_editing_the_cart")}

                        {_i18n.T("cart_command_your_cart_is_empty")}
                        """;

                    await _response.EditLastMessage(text, token, _inlineMarkup.CartIsEmpty());

                    return;
                }

                int index = 0;
                StringBuilder sb = new();

                sb.AppendLine(_i18n.T("edit_cart_command_editing_the_cart"));
                sb.AppendLine();
                foreach (var item in cart)
                {
                    index++;

                    sb.AppendLine($"<b>№ {index}</b>: <b>{item.Product?.Name}</b><i>, {item.Product?.Dimensions}</i>");
                    sb.AppendLine();
                }

                await _response.EditLastMessage(sb.ToString(), token, _inlineMarkup.EditCart(cart));

                return;
            }
        }
    }
}