using System.Globalization;
using System.Text;

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

using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.PROFILE)]
    public class Profile : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly II18nService _i18n;
        private readonly IDataProcessorService _dataProcessor;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly IStaticCollectionsService _staticCollections;

        public Profile(
                IUpdateManager updateManager,
                ISendResponseService response,
                II18nService i18n,
                IDataProcessorService dataProcessor,
                IInlineMarkupService inlineMarkup,
                IStaticCollectionsService staticCollections
            )
        {
            _updateManager = updateManager;
            _response = response;
            _i18n = i18n;
            _dataProcessor = dataProcessor;
            _inlineMarkup = inlineMarkup;
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

            if (request.Customer.StoreId == null || request.Customer.StoreId == Guid.Empty)
            {
                throw new TelegramException();
            }

            if (!_staticCollections.CitiesWithStoreAddressesDictionary.TryGetValue(
                key: request.Customer.StoreId.ToString()!,
                value: out var store))
            {
                throw new TelegramException();
            }

            if (!request.Customer.CreatedAt.HasValue)
            {
                throw new TelegramException();
            }

            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(
                dateTime: request.Customer.CreatedAt.Value,
                destinationTimeZone: TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));

            StringBuilder sb = new();

            sb.AppendLine(_i18n.T("main_menu_command_profile_button"));
            sb.AppendLine();
            sb.Append("<b>");
            sb.Append(_i18n.T("profile_command_username"));
            sb.Append("</b> <i>");
            sb.Append(request.Customer.Username);
            sb.Append("</i>\n");
            sb.AppendLine();
            sb.Append("<b>");
            sb.Append(_i18n.T("profile_command_name"));
            sb.Append("</b> <i>");
            sb.Append(request.Customer.FirstName);
            sb.Append(' ');
            sb.Append(request.Customer.LastName);
            sb.Append("</i>\n");
            sb.AppendLine();
            sb.Append("<b>");
            sb.Append(_i18n.T("profile_command_phone_number"));
            sb.Append("</b> <i>");
            sb.Append(request.Customer.PhoneNumber);
            sb.Append("</i>\n");
            sb.AppendLine();
            sb.Append("<b>");
            sb.Append(_i18n.T("profile_command_store_address"));
            sb.Append("</b>\n<i>");
            sb.Append(store.City);
            sb.Append(' ');
            sb.Append(store.Address);
            sb.Append("</i>\n");
            sb.AppendLine();
            sb.Append("<b>");
            sb.Append(_i18n.T("profile_command_creation_date"));
            sb.Append("</b>\n<i>");
            sb.Append(localDateTime.ToString("D", new CultureInfo("uk-UA")));
            sb.Append(' ');
            sb.Append(localDateTime.ToString("t", new CultureInfo("uk-UA")));
            sb.Append("</i>\n");

            await _response.EditLastMessage(sb.ToString(), token, _inlineMarkup.Profile());
        }
    }
}