using System.Text.RegularExpressions;

using AutoMapper;

using Microsoft.Extensions.Caching.Distributed;

using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Extensions;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.SAVECONTACT)]
    public class SaveContact : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly ICustomersRepository _customersRepository;
        private readonly II18nService _i18n;
        private readonly IMapper _mapper;
        private readonly IDataProcessorService _dataProcessor;
        private readonly IReplyMarkupService _replyMarkup;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly IDistributedCache _distributedCache;

        public SaveContact(
                IUpdateManager updateManager,
                ISendResponseService response,
                ICustomersRepository customersRepository,
                II18nService i18n,
                IMapper mapper,
                IDataProcessorService dataProcessor,
                IReplyMarkupService replyMarkup,
                IInlineMarkupService inlineMarkup,
                IDistributedCache distributedCache
            )
        {
            _updateManager = updateManager;
            _response = response;
            _customersRepository = customersRepository;
            _i18n = i18n;
            _mapper = mapper;
            _dataProcessor = dataProcessor;
            _replyMarkup = replyMarkup;
            _inlineMarkup = inlineMarkup;
            _distributedCache = distributedCache;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                RequestInfo request = await _dataProcessor.MapRequestData(token);

                if (request.Message == null || request.Message.Contact == null)
                {
                    await RemoveCustomerAndSendExceptionMessage(token);

                    return;
                }

                Contact contact = request.Message.Contact;

                if (string.IsNullOrEmpty(contact.PhoneNumber) || string.IsNullOrWhiteSpace(contact.PhoneNumber))
                {
                    await RemoveCustomerAndSendExceptionMessage(token);

                    return;
                }

                string formattedPhoneNumber = Regex.Replace(
                    input: contact.PhoneNumber.Trim(),
                    pattern: RegexPatterns.PhoneNumberFormattingPattern,
                    replacement: string.Empty);

                formattedPhoneNumber = '+' + formattedPhoneNumber;

                if (formattedPhoneNumber.Length <= 32 &&
                    !formattedPhoneNumber.RegexIsMatch(RegexPatterns.PhoneNumber))
                {
                    await RemoveCustomerAndSendExceptionMessage(token);
                    return;
                }

                string? lastMessageIdString = await _distributedCache
                    .GetStringAsync($"{CacheKeys.CustomerFirstMessageId}{request.Customer.Id}", token);

                if (string.IsNullOrEmpty(lastMessageIdString))
                {
                    await RemoveCustomerAndSendExceptionMessage(token);
                    return;
                }

                if (!int.TryParse(lastMessageIdString, out int lastMessageId))
                {
                    await RemoveCustomerAndSendExceptionMessage(token);
                    return;
                }

                await _distributedCache.RemoveAsync($"{CacheKeys.CustomerFirstMessageId}{request.Customer.Id}", token);

                await _customersRepository.SaveContactsAsync(
                    id: request.Customer.Id,
                    phoneNumber: formattedPhoneNumber,
                    firstName: contact.FirstName,
                    lastName: contact.LastName,
                    token: token);

                await _response.RemoveMessageById(lastMessageId, token);

                Message clearReplyMarkup = await _response.SendMessageAnswer(".", token, _replyMarkup.Empty);

                await _response.RemoveMessageById(clearReplyMarkup.MessageId, token);

                await _response.RemoveLastMessage(token);

                string text = $"""
                    {_i18n.T("save_contact_command_phone_number_is_saved")}

                    {_i18n.T("save_contact_command_select_your_address_1")}

                    {_i18n.T("save_contact_command_select_your_address_2")}
                    """;

                await _response.SendMessageAnswer(text, token, _inlineMarkup.SelectStoreAddress("savestoreaddress"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                await RemoveCustomerAndSendExceptionMessage(token);

                return;
            }
        }

        private async Task RemoveCustomerAndSendExceptionMessage(CancellationToken token)
        {
            CustomerDto customer = _mapper.Map<CustomerDto>(_updateManager.Update);

            await _customersRepository.RemoveCustomerAsync(customer.Id);

            await _response.SendMessageAnswer($"""
                {_i18n.T("exception_main")}

                {_i18n.T("exception_main_later")}

                {_i18n.T("exception_main_we_are_very_sorry")}
                """, token, _replyMarkup.Empty);
        }
    }
}