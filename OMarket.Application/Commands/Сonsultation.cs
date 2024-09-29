using System.Globalization;

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

using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.CONSULTATION)]
    public class Сonsultation : ITgCommand
    {
        private readonly ISendResponseService _response;
        private readonly IUpdateManager _updateManager;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly IStoreRepository _storeRepository;

        public Сonsultation(
                ISendResponseService response,
                IUpdateManager updateManager,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                IStoreRepository storeRepository
            )
        {
            _response = response;
            _updateManager = updateManager;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
            _storeRepository = storeRepository;
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

            if (request.Customer.BlockedReviews || request.Customer.BlockedOrders)
            {
                string blockedText = $"""
                    <b>{_i18n.T("consultation_command_cannot_get_consultation_blocked")}</b>
                    
                    {_i18n.T("main_menu_command_consultation_button")}
                    """;

                InlineKeyboardMarkup blockedButtons = await _inlineMarkup.MainMenu(token);

                await _response.EditLastMessage(blockedText, token, blockedButtons);

                return;
            }

            if (string.IsNullOrEmpty(request.Query))
            {
                string text = $"""
                    {_i18n.T("main_menu_command_consultation_button")}

                    <b>{_i18n.T("consultation_command_select_store_you_want_contact")}</b>

                    <i>{_i18n.T("consultation_command_please_note_consultant_receives_notification")}</i>
                    """;

                await _response.EditLastMessage(text, token, _inlineMarkup.SelectStoreAddressForConsultation());
            }
            else
            {
                if (!Guid.TryParse(request.Query, out Guid storeId))
                {
                    throw new TelegramException();
                }

                if (storeId == Guid.Empty)
                {
                    throw new TelegramException();
                }

                long storeTgChatId = await _storeRepository
                    .GetStoreChatIdAsync(storeId, token)
                        ?? throw new TelegramException();

                DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(
                    dateTime: DateTime.UtcNow,
                    destinationTimeZone: TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));

                string date = localDateTime.ToString("dd MMM yyyy HH:mm", new CultureInfo("uk-UA"));

                string textStoreChat = $"""
                    {_i18n.T("admins_command_customer_consultation_has_been_ordered")}

                    <b>{_i18n.T("admins_command_date")}</b> <i>{date}</i>

                    <b>{_i18n.T("admins_command_customer_name")}</b> <i>{request.Customer.FirstName + ' ' + request.Customer.LastName}</i>

                    <b>{_i18n.T("admins_command_customer_phone_number")}</b> <i>{request.Customer.PhoneNumber}</i>
                    """;

                await _response.SendMessageAnswerByChatId(storeTgChatId, textStoreChat, token, _inlineMarkup.RemoveThisMessage());

                string textCustomer = $"""
                    {_i18n.T("consultation_command_consultant_will_contact_you_soon")}
                    
                    {_i18n.T("generic_main_manu_title")}
                    """;

                InlineKeyboardMarkup buttonsCustomer = await _inlineMarkup.MainMenu(token);

                await _response.EditLastMessage(textCustomer, token, buttonsCustomer);
            }
        }
    }
}