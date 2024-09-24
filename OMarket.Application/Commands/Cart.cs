using System.Text;

using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.Cart;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;

using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.CART)]
    public class Cart : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IProductsRepository _productsRepository;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly ICartService _cartService;

        public Cart(
                IUpdateManager updateManager,
                ISendResponseService response,
                IProductsRepository productsRepository,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                ICartService cartService
            )
        {
            _updateManager = updateManager;
            _response = response;
            _productsRepository = productsRepository;
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

            List<CartItemDto> cart = await _cartService
                .GatCustomerCartAsync(request.Customer.Id, token);

            if (cart.Count == 0)
            {
                await _response.SendCallbackAnswerAlert(_i18n.T("cart_command_your_cart_is_empty"), token);

                return;
            }

            int quantity = 0;
            decimal totalPrice = decimal.Zero;
            StringBuilder sb = new();

            sb.AppendLine(_i18n.T("cart_command_title"));
            sb.AppendLine();
            foreach (var item in cart)
            {
                decimal price = item.Product?.Price * item.Quantity ?? decimal.Zero;
                sb.AppendLine($"📌 <b>{item.Product?.Name}</b>, {item.Product?.Dimensions}");
                sb.AppendLine($"💵 <b>{item.Product?.Price}</b> * <b>{item.Quantity}</b> шт. = <b>{price}</b> грн.");
                sb.AppendLine();

                quantity += item.Quantity;
                totalPrice += price;
            }
            sb.AppendLine($"<i>{_i18n.T("cart_command_quantity")}</i> <b>{quantity}</b> <i>{_i18n.T("cart_command_for_amount")}</i> <b>{totalPrice}</b> <i>грн.</i>");

            await _response.EditLastMessage(sb.ToString(), token, _inlineMarkup.Cart());
        }
    }
}