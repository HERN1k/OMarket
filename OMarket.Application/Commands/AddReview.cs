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
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.ADDREVIEW)]
    public class AddReview : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly IReviewRepository _reviewsRepository;
        private readonly IDistributedCache _distributedCache;

        public AddReview(
                IUpdateManager updateManager,
                ISendResponseService response,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                IReviewRepository reviewsRepository,
                IDistributedCache distributedCache
            )
        {
            _updateManager = updateManager;
            _response = response;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
            _reviewsRepository = reviewsRepository;
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

            if (request.Customer.BlockedReviews)
            {
                string text = $"""
                    {_i18n.T("main_menu_command_leave_review_button")}

                    <b>{_i18n.T("add_review_command_cannot_send_review_blocked")}</b>
                    """;

                await _response.EditLastMessage(text, token, _inlineMarkup.ToMainMenuBack());

                return;
            }

            string cacheKey = $"{CacheKeys.CustomerFreeInputId}{request.Customer.Id}";

            string? messageIdString = await _distributedCache.GetStringAsync(cacheKey, token);

            if (string.IsNullOrEmpty(messageIdString))
            {
                throw new TelegramException();
            }

            string[] tempLines = messageIdString.Split('_', 2);

            if (tempLines.Length != 2)
            {
                await _distributedCache.RemoveAsync(cacheKey, token);

                throw new TelegramException();
            }

            if (tempLines[0] != "/67108864")
            {
                await _distributedCache.RemoveAsync(cacheKey, token);

                throw new TelegramException();
            }

            string[] queryLines = tempLines[1].Split('=', 2);

            if (queryLines.Length == 1)
            {
                if (_updateManager.Update.Type == UpdateType.CallbackQuery)
                {
                    await _response.SendCallbackAnswer(token);
                }

                int messageId = _updateManager.CallbackQuery.Message?.MessageId
                    ?? throw new TelegramException();

                await _distributedCache.SetStringAsync(
                    cacheKey,
                    $"/67108864_{messageId}={request.Query}", token);

                string text = $"""
                    {_i18n.T("main_menu_command_leave_review_button")}

                    <b>{_i18n.T("add_review_command_send_your_feedback")}</b>

                    <i>{_i18n.T("add_review_command_platform_limitations")}

                    {_i18n.T("add_review_command_all_reviews_are_anonymous")}</i>
                    """;

                await _response.EditLastMessage(text, token, _inlineMarkup.Empty);
            }
            else
            {
                if (queryLines.Length != 2)
                {
                    await _distributedCache.RemoveAsync(cacheKey, token);

                    throw new TelegramException();
                }

                if (!int.TryParse(queryLines[0], out int messageId))
                {
                    await _distributedCache.RemoveAsync(cacheKey, token);

                    throw new TelegramException();
                }

                if (!Guid.TryParse(queryLines[1], out Guid storeId))
                {
                    await ThrowIfError(cacheKey, request.Query, messageId, token);

                    return;
                }

                if (storeId == Guid.Empty)
                {
                    await ThrowIfError(cacheKey, request.Query, messageId, token);

                    return;
                }

                string? review = _updateManager.Update.Message?.Text;

                if (string.IsNullOrEmpty(review))
                {
                    await ThrowIfError(cacheKey, request.Query, messageId, token);

                    return;
                }

                string formattedReview = review
                    .Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&#39;");

                if (formattedReview.Length >= 256)
                {
                    formattedReview = formattedReview[..256];
                }

                try
                {
                    await _reviewsRepository.AddNewReviewAsync(
                        id: request.Customer.Id,
                        storeId: storeId,
                        text: formattedReview,
                        token: token);
                }
                catch (Exception)
                {
                    await ThrowIfError(cacheKey, request.Query, messageId, token);

                    return;
                }

                await _distributedCache.RemoveAsync(cacheKey, token);

                string text = $"""
                    {_i18n.T("add_review_command_thank_you_review_saved")}

                    {_i18n.T("generic_main_manu_title")}
                    """;

                InlineKeyboardMarkup buttons = await _inlineMarkup.MainMenu(token);

                await _response.RemoveMessageById(messageId, token);
                await _response.RemoveLastMessage(token);
                await _response.SendMessageAnswer(text, token, buttons);
            }
        }

        private async Task ThrowIfError(string cacheKey, string storeId, int messageId, CancellationToken token)
        {
            if (string.IsNullOrEmpty(cacheKey) || string.IsNullOrEmpty(storeId))
            {
                throw new TelegramException();
            }

            try
            {
                await _response.RemoveMessageById(messageId, token);
                await _response.RemoveLastMessage(token);
            }
            finally
            {
                Message message = await _response.SendMessageAnswer(
                    text: _i18n.T("exception_main_please_try_again"),
                    token: token,
                    buttons: _inlineMarkup.ToMainMenuBack());

                await _distributedCache.SetStringAsync(cacheKey, $"/67108864_{message.MessageId}={storeId}", token);
            }
        }
    }
}