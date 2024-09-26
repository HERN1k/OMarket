using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Caching.Distributed;

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
using OMarket.Helpers.Extensions;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.UPDATEPHONENUMBER)]
    public class UpdatePhoneNumber : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly ICustomersRepository _customersRepository;
        private readonly IDistributedCache _distributedCache;
        private readonly IStaticCollectionsService _staticCollections;

        public UpdatePhoneNumber(
                IUpdateManager updateManager,
                ISendResponseService response,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                ICustomersRepository customersRepository,
                IDistributedCache distributedCache,
                IStaticCollectionsService staticCollections
            )
        {
            _updateManager = updateManager;
            _response = response;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
            _customersRepository = customersRepository;
            _distributedCache = distributedCache;
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

            string? messageIdString = await _distributedCache.GetStringAsync(cacheKey, token);

            if (string.IsNullOrEmpty(messageIdString))
            {
                if (_updateManager.Update.Type == UpdateType.CallbackQuery)
                {
                    await _response.SendCallbackAnswer(token);
                }

                int messageId = _updateManager.CallbackQuery.Message?.MessageId
                    ?? throw new TelegramException();

                await _distributedCache.SetStringAsync(
                    $"{CacheKeys.CustomerFreeInputId}{request.Customer.Id}",
                    $"/33554432_{messageId}", token);

                string text = $"""
                    {_i18n.T("main_menu_command_profile_button")}

                    <b>{_i18n.T("profile_update_command_enter_new_phone_number")}</b>

                    <i>📌 +00(000) 000 0000
                    📌 +00(000)-000-0000
                    📌 +00(000)0000000
                    📌 +00 000 000 0000
                    📌 +000000000000
                    📌 000000000000</i>
                    """;

                await _response.EditLastMessage(text, token, _inlineMarkup.Empty);
            }
            else
            {
                string[] queryLines = messageIdString.Split('_', 2);

                if (queryLines.Length != 2)
                {
                    await _distributedCache.RemoveAsync(cacheKey, token);

                    throw new TelegramException();
                }

                if (queryLines[0] != "/33554432")
                {
                    await _distributedCache.RemoveAsync(cacheKey, token);

                    throw new TelegramException();
                }

                if (!int.TryParse(queryLines[1], out int messageId))
                {
                    await _distributedCache.RemoveAsync(cacheKey, token);

                    throw new TelegramException();
                }

                string? phoneNumber = _updateManager.Update.Message?.Text;

                if (string.IsNullOrEmpty(phoneNumber))
                {
                    await ThrowIfRegexError(cacheKey, messageId, token);

                    return;
                }

                string formattedPhoneNumber = Regex.Replace(
                    input: phoneNumber,
                    pattern: RegexPatterns.PhoneNumberFormattingPattern,
                    replacement: string.Empty);

                formattedPhoneNumber = '+' + formattedPhoneNumber;

                if (formattedPhoneNumber.Length <= 32 &&
                    !formattedPhoneNumber.RegexIsMatch(RegexPatterns.PhoneNumber))
                {
                    await ThrowIfRegexError(cacheKey, messageId, token);

                    return;
                }

                try
                {
                    await _customersRepository.SaveContactsAsync(request.Customer.Id, formattedPhoneNumber, token);
                }
                catch (Exception)
                {
                    await ThrowIfRegexError(cacheKey, messageId, token);

                    return;
                }

                await _distributedCache.RemoveAsync(cacheKey, token);

                request = await _dataProcessor.MapRequestData(token);

                if (!_staticCollections.CitiesWithStoreAddressesDictionary.TryGetValue(
                    key: request.Customer.StoreId.ToString()!,
                    value: out var store))
                {
                    await _distributedCache.RemoveAsync(cacheKey, token);

                    throw new TelegramException();
                }

                if (!request.Customer.CreatedAt.HasValue)
                {
                    await _distributedCache.RemoveAsync(cacheKey, token);

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

                await _response.RemoveMessageById(messageId, token);
                await _response.RemoveLastMessage(token);
                await _response.SendMessageAnswer(sb.ToString(), token, _inlineMarkup.Profile());
            }
        }

        private async Task ThrowIfRegexError(string cacheKey, int messageId, CancellationToken token)
        {
            if (messageId < 1 || string.IsNullOrEmpty(cacheKey))
            {
                throw new TelegramException();
            }

            await _response.RemoveMessageById(messageId, token);
            await _response.RemoveLastMessage(token);
            Message message = await _response.SendMessageAnswer(
                text: _i18n.T("profile_update_command_phone_number_is_not_correct"),
                token: token,
                buttons: _inlineMarkup.ToMainMenuBack());

            await _distributedCache.SetStringAsync(cacheKey, $"/33554432_{message.MessageId}", token);
        }
    }
}