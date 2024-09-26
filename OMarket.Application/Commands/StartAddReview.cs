using Microsoft.Extensions.Caching.Distributed;

using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.STARTADDREVIEW)]
    public class StartAddReview : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly IDistributedCache _distributedCache;

        public StartAddReview(
                IUpdateManager updateManager,
                ISendResponseService response,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                IDistributedCache distributedCache
            )
        {
            _updateManager = updateManager;
            _response = response;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
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

            string text;
            if (request.Customer.BlockedReviews)
            {
                text = $"""
                    {_i18n.T("main_menu_command_leave_review_button")}

                    <b>{_i18n.T("add_review_command_cannot_send_review_blocked")}</b>
                    """;

                await _response.EditLastMessage(text, token, _inlineMarkup.ToMainMenuBack());

                return;
            }

            text = $"""
                {_i18n.T("main_menu_command_leave_review_button")}

                {_i18n.T("add_review_command_select_store_address_leave_review")}
                """;

            int messageId = _updateManager.CallbackQuery.Message?.MessageId
                    ?? throw new TelegramException();

            await _distributedCache.SetStringAsync(
                $"{CacheKeys.CustomerFreeInputId}{request.Customer.Id}",
                $"/67108864_{messageId}", token);

            await _response.EditLastMessage(text, token, _inlineMarkup.SelectStoreAddressForAddReview());
        }
    }
}