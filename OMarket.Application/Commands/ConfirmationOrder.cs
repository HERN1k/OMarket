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

            Message awaitMessage = await _response.SendMessageAnswer(_i18n.T("order_command_please_wait"), token);

            List<CartItemDto> cart = await _cartService.GatCustomerCartAsync(request.Customer.Id, token);

            if (cart.Count <= 0)
            {
                await ThrowIfError(cacheKey, request.Customer.Id, messageId, awaitMessage.MessageId, token);

                return;
            }

            if (request.Customer.StoreId == Guid.Empty)
            {
                await ThrowIfError(cacheKey, request.Customer.Id, messageId, awaitMessage.MessageId, token);

                return;
            }

            if (!_staticCollections.StoresSet.Any(store => store.Id == request.Customer.StoreId))
            {
                await ThrowIfError(cacheKey, request.Customer.Id, messageId, awaitMessage.MessageId, token);

                return;
            }

            string? orderComment = _updateManager.Update.Message?.Text;

            if (string.IsNullOrEmpty(orderComment))
            {
                await ThrowIfError(cacheKey, request.Customer.Id, messageId, awaitMessage.MessageId, token);
                return;
            }

            string formattedOrderComment = orderComment
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");

            if (formattedOrderComment.Length >= 128)
            {
                formattedOrderComment = formattedOrderComment[..120];
            }

            DeliveryMethod deliveryMethod = DeliveryMethodExtensions.GetDeliveryMethod(queryLines[1].ToUpper());

            if (deliveryMethod == DeliveryMethod.NONE)
            {
                await ThrowIfError(cacheKey, request.Customer.Id, messageId, awaitMessage.MessageId, token);

                return;
            }

            bool isReliableData = await _productsRepository.CheckingAvailabilityOfProductsInTheStore(
                cart: cart,
                storeId: (Guid)request.Customer.StoreId,
                token: token);

            if (!isReliableData)
            {
                await ThrowIfError(cacheKey, request.Customer.Id, messageId, awaitMessage.MessageId, token);

                return;
            }

            CreatedOrderDto? order = SetOrderProducts(cart);

            if (order is null)
            {
                await ThrowIfError(cacheKey, request.Customer.Id, messageId, awaitMessage.MessageId, token);

                return;
            }

            order.StoreId = (Guid)request.Customer.StoreId;
            order.CustomerId = request.Customer.Id;
            order.DeliveryMethod = deliveryMethod;
            order.Comment = formattedOrderComment;

            Console.WriteLine($"Id: {order.Id}");
            Console.WriteLine($"StoreId: {order.StoreId}");
            Console.WriteLine($"CustomerId: {order.CustomerId}");
            Console.WriteLine($"DeliveryMethod: {order.DeliveryMethod}");
            Console.WriteLine($"Comment: {order.Comment}");
            Console.WriteLine($"TotalQuantity: {order.TotalQuantity}");
            Console.WriteLine($"TotalPrice: {order.TotalPrice}");
            foreach (var item in order.Products)
            {
                Console.WriteLine($"ProductName: {item.Product.Name}");
            }




        }

        private async Task ThrowIfError(string cacheKey, long customerId, int messageId, int awaitMessageId, CancellationToken token)
        {
            if (customerId <= 0 || messageId <= 0 || awaitMessageId <= 0 || string.IsNullOrEmpty(cacheKey))
            {
                throw new TelegramException("exception_main_please_try_again");
            }

            try
            {
                await _distributedCache.RemoveAsync(cacheKey, token);
                await _cartService.RemoveCartAsync(customerId, token);

                await _response.RemoveMessageById(awaitMessageId, token);
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
    }
}