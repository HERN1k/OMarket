using System.Text;

using Microsoft.Extensions.Caching.Distributed;

using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Cart;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.CREATEORDER)]
    public class CreateOrder : ITgCommand
    {
        private readonly ISendResponseService _response;
        private readonly IUpdateManager _updateManager;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly ICartService _cartService;
        private readonly IStaticCollectionsService _staticCollections;
        private readonly IDistributedCache _distributedCache;

        public CreateOrder(
                ISendResponseService response,
                IUpdateManager updateManager,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                ICartService cartService,
                IStaticCollectionsService staticCollections,
                IDistributedCache distributedCache
            )
        {
            _response = response;
            _updateManager = updateManager;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
            _cartService = cartService;
            _staticCollections = staticCollections;
            _distributedCache = distributedCache;
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

            string cacheKey = $"{CacheKeys.CustomerFreeInputId}{request.Customer.Id}";

            if (request.Customer.BlockedOrders)
            {
                string blockedText = $"""
                    {_i18n.T("order_command_your_order_title")}

                    <b>{_i18n.T("order_command_cannot_create_order_blocked")}</b>
                    """;

                await _distributedCache.RemoveAsync(cacheKey, token);

                await _cartService.RemoveCartAsync(request.Customer.Id, token);

                await _response.EditLastMessage(blockedText, token, _inlineMarkup.ToMainMenuBack());

                return;
            }

            if (!DateTimeHelper.IsTimeAllowed())
            {
                string badTimeText = $"""
                    {_i18n.T("order_command_your_order_title")}

                    <b>{_i18n.T("order_command_unable_create_order")}</b>
                    """;

                await _response.EditLastMessage(badTimeText, token, _inlineMarkup.ToMainMenuBack());

                return;
            }

            List<CartItemDto> cart = await _cartService
                .GatCustomerCartAsync(request.Customer.Id, token);

            if (cart.Count <= 0)
            {
                throw new TelegramException("exception_main_please_try_again");
            }

            if (!_staticCollections.CitiesWithStoreAddressesDictionary.TryGetValue(
                    request.Customer.StoreId.ToString()!, out var storeAddress))
            {
                throw new TelegramException("exception_main_please_try_again");
            }

            StoreDto store = _staticCollections.StoresSet
                .SingleOrDefault(store => store.Id == request.Customer.StoreId)
                    ?? throw new TelegramException();

            string text = BuildMessageText(store, storeAddress, cart);

            if (string.IsNullOrEmpty(text))
            {
                throw new TelegramException("exception_main_please_try_again");
            }

            await _distributedCache.RemoveAsync(cacheKey, token);

            await _response.EditLastMessage(text, token, _inlineMarkup.CreateOrder());
        }

        private string BuildMessageText(StoreDto store, StoreAddressWithCityDto storeAddress, List<CartItemDto> cart)
        {
            if (store is null ||
                storeAddress is null ||
                cart is null ||
                cart.Count <= 0 ||
                string.IsNullOrEmpty(store.PhoneNumber) ||
                string.IsNullOrEmpty(storeAddress.City) ||
                string.IsNullOrEmpty(storeAddress.Address))
            {
                return string.Empty;
            }

            int quantity = 0;
            decimal totalPrice = decimal.Zero;
            StringBuilder sb = new();

            sb.AppendLine(_i18n.T("order_command_your_order_title"));
            sb.AppendLine();

            foreach (var item in cart)
            {
                if (item.Product is null)
                {
                    continue;
                }

                decimal price = item.Product.Price * item.Quantity;

                sb.Append("📌 <b>");
                sb.Append(item.Product.Name);
                sb.Append("</b><i>, ");
                sb.Append(item.Product.Dimensions);
                sb.Append("</i> - <b>");
                sb.Append(item.Quantity);
                sb.Append(" шт.</b>\n");
                sb.AppendLine();

                quantity += item.Quantity;
                totalPrice += price;
            }

            sb.Append("<i>");
            sb.Append(_i18n.T("order_command_quantity"));
            sb.Append("</i> <b>");
            sb.Append(quantity);
            sb.Append("</b> <i>");
            sb.Append(_i18n.T("order_command_for_amount"));
            sb.Append("</i> <b>");
            sb.Append(totalPrice);
            sb.Append("</b> <i>грн.</i>\n");
            sb.AppendLine();

            sb.AppendLine(_i18n.T("order_command_store"));
            sb.Append("<i>");
            sb.Append(storeAddress.City);
            sb.Append(' ');
            sb.Append(storeAddress.Address);
            sb.Append("</i>\n");
            sb.AppendLine();

            sb.Append(_i18n.T("order_command_store_phone_number"));
            sb.Append(" <i>");
            sb.Append(store.PhoneNumber);
            sb.Append("</i>\n");
            sb.AppendLine();

            sb.Append(" <i>");
            sb.Append(_i18n.T("order_command_cost_packag_will_be_added"));
            sb.Append("</i>\n");
            sb.AppendLine();

            sb.Append("<i>");
            sb.Append(_i18n.T("order_command_if_store_is_not_suitable"));
            sb.Append("</i>\n");
            sb.AppendLine();

            sb.Append("<i>");
            sb.Append(_i18n.T("order_command_can_choose_delivery_method"));
            sb.Append("</i>\n");

            return sb.ToString();
        }
    }
}