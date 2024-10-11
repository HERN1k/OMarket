using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Cache;
using OMarket.Domain.Interfaces.Application.Services.Cart;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.DELIVERYORDER)]
    public class DeliveryOrder : ITgCommand
    {
        private readonly ISendResponseService _response;
        private readonly IUpdateManager _updateManager;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly ICartService _cartService;
        private readonly ICacheService _cache;

        public DeliveryOrder(
                ISendResponseService response,
                IUpdateManager updateManager,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                ICartService cartService,
                ICacheService cache
            )
        {
            _response = response;
            _updateManager = updateManager;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
            _cartService = cartService;
            _cache = cache;
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

                await _cache.RemoveCacheAsync(cacheKey);

                await _cartService.RemoveCartAsync(request.Customer.Id, token);

                await _response.EditLastMessage(blockedText, token, _inlineMarkup.ToMainMenuBack());

                return;
            }

            if (string.IsNullOrEmpty(request.Query))
            {
                await _cache.RemoveCacheAsync(cacheKey);

                throw new TelegramException("exception_main_please_try_again");
            }

            string text = $"""
                {_i18n.T("order_command_your_order_title")}

                <b>{_i18n.T("order_command_send_comment")}</b>

                <i>{_i18n.T("order_command_please_note_automatically_confirmed_order")}</i>
                """;

            Message message = await _response.EditLastMessage(text, token, _inlineMarkup.ToMainMenuBack());

            await _cache.SetStringCacheAsync(cacheKey, $"/1000000100_{message.MessageId}={request.Query}");
        }
    }
}