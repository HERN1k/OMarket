using System.Globalization;
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
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.CONFIRMATIONORDER)]
    public class ConfirmationOrder : ITgCommand
    {
        private readonly ISendResponseService _response;
        private readonly IUpdateManager _updateManager;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly ICartService _cartService;
        private readonly IDistributedCache _distributedCache;
        private readonly IProductsRepository _productsRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IStaticCollectionsService _staticCollections;

        public ConfirmationOrder(
                ISendResponseService response,
                IUpdateManager updateManager,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                ICartService cartService,
                IDistributedCache distributedCache,
                IProductsRepository productsRepository,
                IStoreRepository storeRepository,
                IOrdersRepository ordersRepository,
                IStaticCollectionsService staticCollections
            )
        {
            _response = response;
            _updateManager = updateManager;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
            _cartService = cartService;
            _distributedCache = distributedCache;
            _productsRepository = productsRepository;
            _storeRepository = storeRepository;
            _ordersRepository = ordersRepository;
            _staticCollections = staticCollections;
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

            string cacheKey = $"{CacheKeys.CustomerFreeInputId}{request.Customer.Id}";

            if (request.Customer.BlockedOrders)
            {
                string blockedText = $"""
                    {_i18n.T("order_command_your_order_title")}

                    <b>{_i18n.T("order_command_cannot_create_order_blocked")}</b>
                    """;

                await _distributedCache.RemoveAsync(cacheKey, token);

                await _cartService.RemoveCartAsync(request.Customer.Id, token);

                await _response.SendMessageAnswer(blockedText, token, _inlineMarkup.ToMainMenuBack());

                return;
            }

            (int messageId, string deliveryMethodString) = await GetQuery(cacheKey, token);

            await _response.RemoveMessageById(messageId, token);
            await _response.RemoveLastMessage(token);
            Message awaitMessage = await _response.SendMessageAnswer(_i18n.T("order_command_please_wait"), token);

            DeliveryMethod deliveryMethod = DeliveryMethodExtensions.GetDeliveryMethod(deliveryMethodString);

            string orderComment = GetOrderComment();

            long? storeTgChatId = await _storeRepository
                .GetStoreChatIdAsync((Guid)request.Customer.StoreId, token);

            CreatedOrderDto? createdOrder = await CreatOrderDto(
                request: request,
                orderComment: orderComment,
                method: deliveryMethod,
                storeTgChatId: storeTgChatId,
                token: token);

            if (createdOrder is null)
            {
                await ThrowIfError(cacheKey, request.Customer.Id, awaitMessage.MessageId, token);

                return;
            }

            OrderDto? order = await _ordersRepository.SaveNewOrderAsync(createdOrder, token);

            if (order is null)
            {
                await ThrowIfError(cacheKey, request.Customer.Id, awaitMessage.MessageId, token);
                return;
            }

            string text = GetStoreText(request, order, createdOrder);

            InlineKeyboardMarkup buttonsForStoreChat = _inlineMarkup.MarkupOrderForStoreChat(
                status: "Взято в обробку",
                orderId: order.Id);

            await _response.SendMessageAnswerByChatId(createdOrder.TgChatId, text, token, buttonsForStoreChat);

            await _cartService.RemoveCartAsync(request.Customer.Id, token);

            InlineKeyboardMarkup buttons = await _inlineMarkup.MainMenu(token);

            string answerText = $"""
                {_i18n.T("order_command_thank_you")}

                {_i18n.T("order_command_order_being_processed_in_store")}

                {_i18n.T("generic_main_manu_title")}
                """;

            await _response.RemoveMessageById(awaitMessage.MessageId, token);
            await _response.EditLastMessage(answerText, token, buttons);
        }

        private string GetStoreText(RequestInfo request, OrderDto order, CreatedOrderDto createdOrder)
        {
            string date = GetFormattedDate(order.CreatedAt);
            string deliveryMethod = GetFormattedDeliveryMethod(createdOrder.DeliveryMethod);
            StringBuilder sb = new();
            int index = 0;

            sb.AppendLine($"<b>{_i18n.T("admins_command_new_order")}</b>");
            sb.AppendLine();
            sb.AppendLine($"{_i18n.T("order_command_order")} {order.Id.GetHashCode().ToString()[1..]}");
            sb.AppendLine();
            sb.AppendLine($"<b>{_i18n.T("admins_command_date")}</b> <i>{date}</i>");
            sb.AppendLine();
            sb.AppendLine($"<b>{_i18n.T("admins_command_customer_name")}</b> <i>{request.Customer.FirstName + ' ' + request.Customer.LastName}</i>");
            sb.AppendLine();
            sb.AppendLine($"<b>{_i18n.T("admins_command_customer_phone_number")}</b> <i>{request.Customer.PhoneNumber}</i>");
            sb.AppendLine();
            sb.AppendLine($"<b>{_i18n.T("admins_command_delivery_method")}</b> <i>{deliveryMethod}</i>");
            sb.AppendLine();
            sb.AppendLine($"<b>{_i18n.T("admins_command_customer_comment")}</b> ");
            sb.AppendLine();
            sb.AppendLine($"<i>{createdOrder.Comment}</i>");
            sb.AppendLine();
            sb.AppendLine($"<b>{_i18n.T("admins_command_products")}</b> ");
            sb.AppendLine();
            foreach (var item in createdOrder.Products)
            {
                sb.AppendLine($"<b>№{++index}</b> <i>{item.Product.Name}, {item.Product.Dimensions}</i><b> - {item.Quantity} шт.</b>");
                sb.AppendLine();
            }
            sb.AppendLine($"{_i18n.T("admins_command_quantity")} <b>{createdOrder.TotalQuantity}</b> {_i18n.T("admins_command_for_amount")} <b>{createdOrder.TotalPrice} грн.</b>");

            if (createdOrder.DeliveryMethod == DeliveryMethod.DELIVERY)
            {
                sb.AppendLine();
                sb.AppendLine($"<b>{_i18n.T("admins_command_order_does_not_include_the_cost_of_delivery")}</b>");
            }

            return sb.ToString();
        }

        private string GetFormattedDeliveryMethod(DeliveryMethod deliveryMethod)
        {
            return deliveryMethod == DeliveryMethod.DELIVERY
                ? "Доставка"
                : "Самовивіз";
        }

        private string GetFormattedDate(DateTime date)
        {
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(
                dateTime: date,
                destinationTimeZone: TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));

            return localDateTime.ToString("dd MMM yyyy HH:mm:ss", new CultureInfo("uk-UA"));
        }

        private async Task<(int messageId, string deliveryMethodString)> GetQuery(string cacheKey, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(cacheKey))
            {
                await _distributedCache.RemoveAsync(cacheKey, token);

                throw new TelegramException("exception_main_please_try_again");
            }

            string? messageIdString = await _distributedCache.GetStringAsync(cacheKey, token);

            if (string.IsNullOrEmpty(messageIdString))
            {
                throw new TelegramException("exception_main_please_try_again");
            }

            string[] tempLines = messageIdString.Split('_', 2);

            if (tempLines.Length != 2)
            {
                await _distributedCache.RemoveAsync(cacheKey, token);

                throw new TelegramException("exception_main_please_try_again");
            }

            if (tempLines[0] != "/1000000100")
            {
                await _distributedCache.RemoveAsync(cacheKey, token);

                throw new TelegramException("exception_main_please_try_again");
            }

            string[] queryLines = tempLines[1].Split('=', 2);

            if (queryLines.Length != 2)
            {
                await _distributedCache.RemoveAsync(cacheKey, token);

                throw new TelegramException("exception_main_please_try_again");
            }

            if (!int.TryParse(queryLines[0], out int messageId))
            {
                await _distributedCache.RemoveAsync(cacheKey, token);

                throw new TelegramException("exception_main_please_try_again");
            }

            return (messageId: messageId, deliveryMethodString: queryLines[1].ToUpper());
        }

        private string GetOrderComment()
        {
            if (_updateManager.Update.Message is null || string.IsNullOrEmpty(_updateManager.Update.Message.Text))
            {
                return string.Empty;
            }

            string text = _updateManager.Update.Message.Text;

            string formattedText = text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");

            if (formattedText.Length >= 128)
            {
                formattedText = formattedText[..120];
            }

            return formattedText;
        }

        private bool IsValidStoreId(RequestInfo request)
        {
            if (request.Customer.StoreId == Guid.Empty)
            {
                return false;
            }

            if (!_staticCollections.StoresSet.Any(store => store.Id == request.Customer.StoreId))
            {
                return false;
            }

            return true;
        }

        private async Task<CreatedOrderDto?> CreatOrderDto(RequestInfo request, string orderComment, DeliveryMethod method, long? storeTgChatId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(request.Customer.PhoneNumber))
            {
                return null;
            }

            if (!IsValidStoreId(request))
            {
                return null;
            }

            if (string.IsNullOrEmpty(orderComment))
            {
                return null;
            }

            if (method == DeliveryMethod.NONE)
            {
                return null;
            }

            if (storeTgChatId is null)
            {
                return null;
            }

            List<CartItemDto> cart = await _cartService.GatCustomerCartAsync(request.Customer.Id, token);

            if (cart.Count <= 0)
            {
                return null;
            }

            bool isReliableData = await _productsRepository.CheckingAvailabilityOfProductsInTheStore(
                cart: cart,
                storeId: (Guid)request.Customer.StoreId!,
                token: token);

            if (!isReliableData)
            {
                return null;
            }

            CreatedOrderDto? order = SetOrderProducts(cart);

            if (order is null)
            {
                return null;
            }

            order.StoreId = (Guid)request.Customer.StoreId;
            order.TgChatId = (long)storeTgChatId;
            order.CustomerId = request.Customer.Id;
            order.DeliveryMethod = method;
            order.Comment = orderComment;

            return order;
        }

        private CreatedOrderDto? SetOrderProducts(List<CartItemDto> cart)
        {
            CreatedOrderDto order = new();

            foreach (var item in cart)
            {
                if (item.Product is null)
                {
                    return null;
                }

                decimal price = item.Product.Price * item.Quantity;

                OrderProductDto orderProduct = new()
                {
                    Id = item.Product.Id,
                    Product = item.Product,
                    Quantity = item.Quantity
                };

                order.Products.Add(orderProduct);
                order.TotalQuantity += item.Quantity;
                order.TotalPrice += price;
            }

            if (order.Products.Count <= 0)
            {
                return null;
            }

            if (cart.Count != order.Products.Count)
            {
                return null;
            }

            return order;
        }

        private async Task ThrowIfError(string cacheKey, long customerId, int messageId, CancellationToken token)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new TelegramException("exception_main_please_try_again");
            }

            try
            {
                await _distributedCache.RemoveAsync(cacheKey, token);
                await _cartService.RemoveCartAsync(customerId, token);

                await _response.RemoveMessageById(messageId, token);
                await _response.RemoveLastMessage(token);
            }
            finally
            {
                string text = $"""
                    {_i18n.T("exception_main")}

                    {_i18n.T("generic_main_manu_title")}
                    """;

                InlineKeyboardMarkup buttons = await _inlineMarkup.MainMenu(token);

                await _response.SendMessageAnswer(text, token, buttons);
            }
        }
    }
}