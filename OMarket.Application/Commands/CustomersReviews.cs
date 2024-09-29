using System.Globalization;

using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;

using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.CUSTOMERSREVIEWS)]
    public class CustomersReviews : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly IReviewRepository _reviewRepository;
        private readonly IStaticCollectionsService _staticCollections;

        public CustomersReviews(
                IUpdateManager updateManager,
                ISendResponseService response,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                IReviewRepository reviewRepository,
                IStaticCollectionsService staticCollections
            )
        {
            _updateManager = updateManager;
            _response = response;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
            _reviewRepository = reviewRepository;
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

            if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                await _response.SendCallbackAnswer(token);
            }

            if (string.IsNullOrEmpty(request.Query))
            {
                string text = $"""
                    {_i18n.T("main_menu_command_customer_reviews_button")}

                    {_i18n.T("add_review_command_select_store_address_view_review")}
                    """;

                await _response.EditLastMessage(text, token, _inlineMarkup.SelectStoreAddressForViewReview());
            }
            else
            {
                string[] queryLines = request.Query.Split('_', 2);

                if (queryLines.Length != 2)
                {
                    throw new TelegramException();
                }

                if (!Guid.TryParse(queryLines[0], out Guid storeId))
                {
                    throw new TelegramException();
                }

                if (storeId == Guid.Empty)
                {
                    throw new TelegramException();
                }

                if (!int.TryParse(queryLines[1], out int pageNumber))
                {
                    throw new TelegramException();
                }

                var review = await _reviewRepository.GetReviewWithPaginationAsync(
                    pageNumber: pageNumber,
                    storeId: storeId,
                    token: token);

                string text;

                if (review is null || review.Review is null)
                {
                    text = $"""
                        {_i18n.T("main_menu_command_customer_reviews_button")}

                        {_i18n.T("add_review_command_no_reviews_have_been")}

                        <i>{_i18n.T("add_review_command_be_the_first")}</i>
                        """;

                    await _response.EditLastMessage(text, token, _inlineMarkup.NoReviewsHaveBeenView());

                    return;
                }

                DateTime date = review.Review is not null
                    ? review.Review.CreatedAt
                    : DateTime.MinValue;

                DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(
                    dateTime: date,
                    destinationTimeZone: TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));

                string formattedDate = localDateTime.ToString("dd MMM yyyy HH:mm", new CultureInfo("uk-UA"));

                if (!_staticCollections.CitiesWithStoreAddressesDictionary.TryGetValue(
                    review.Review!.StoreId.ToString(), out var store))
                {
                    throw new TelegramException();
                }

                text = $"""
                    {_i18n.T("main_menu_command_customer_reviews_button")}

                    <b>{_i18n.T("add_review_command_store")}</b>
                    <i>{store.City} {store.Address}</i>

                    <b>{_i18n.T("add_review_command_date")}</b> <i>{formattedDate}</i>

                    <b>{_i18n.T("add_review_command_customer_review")}</b>

                    <i>{review.Review?.Text ?? string.Empty}</i>
                    """;

                InlineKeyboardMarkup buttons = _inlineMarkup.ReviewView(review);

                await _response.EditLastMessage(text, token, buttons);
            }
        }
    }
}