using AutoMapper;

using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.START)]
    public class Start : ITgCommand
    {
        private readonly ISendResponseService _response;
        private readonly IUpdateManager _updateManager;
        private readonly ICustomersRepository _repository;
        private readonly II18nService _i18n;
        private readonly IMapper _mapper;
        private readonly IReplyMarkupService _replyMarkup;
        private readonly IInlineMarkupService _inlineMarkup;

        public Start(
                ISendResponseService response,
                IUpdateManager updateManager,
                ICustomersRepository repository,
                II18nService i18n,
                IMapper mapper,
                IReplyMarkupService replyMarkup,
                IInlineMarkupService inlineMarkup
            )
        {
            _response = response;
            _updateManager = updateManager;
            _repository = repository;
            _i18n = i18n;
            _mapper = mapper;
            _replyMarkup = replyMarkup;
            _inlineMarkup = inlineMarkup;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                await _response.SendCallbackAnswer(token);
            }

            CustomerDto customer = _mapper.Map<CustomerDto>(_updateManager.Update);

            bool isOldUser = await _repository
                .AnyCustomerByIdAsync(customer.Id, token);

            if (!isOldUser)
            {
                customer = await _repository.AddNewCustomerAsync(_updateManager.Update, token);
            }
            else
            {
                customer = await _repository.GetCustomerFromIdAsync(customer.Id, token);
            }

            if (string.IsNullOrEmpty(customer.PhoneNumber))
            {
                token.ThrowIfCancellationRequested();

                string greeting = $"""
                {_i18n.T("start_command_greeting_phrase_1")
                    + (customer.FirstName != null ? " " + customer.FirstName : string.Empty)}!

                {_i18n.T("start_command_greeting_phrase_2")}

                {_i18n.T("start_command_greeting_phrase_3")}
                {_i18n.T("start_command_greeting_phrase_4")}
                {_i18n.T("start_command_greeting_phrase_5")}
                {_i18n.T("start_command_greeting_phrase_6")}
                """;

                await _response.SendMessageAnswer(greeting, token);

                greeting = $"""
                {_i18n.T("start_command_share_your_phone_number_1")}

                {_i18n.T("start_command_share_your_phone_number_2")}
                """;

                await _response.SendMessageAnswer(greeting, token, _replyMarkup.SendPhoneNumber());
            }
            else
            {
                token.ThrowIfCancellationRequested();

                string greeting = DateTimeHelper.TimeOfDayNow() switch
                {
                    TimeOfDay.Morning => _i18n.T("generic_good_morning"),
                    TimeOfDay.Day => _i18n.T("generic_good_day"),
                    TimeOfDay.Evening => _i18n.T("generic_good_evening"),
                    TimeOfDay.Night => _i18n.T("generic_good_night"),
                    TimeOfDay.None => _i18n.T("generic_good_day"),
                    _ => _i18n.T("generic_good_day")
                };

                await _response.SendMessageAnswer(greeting, token);

                InlineKeyboardMarkup buttons = await _inlineMarkup
                    .MainMenu(token);

                await _response.SendMessageAnswer(_i18n.T("generic_main_manu_title"), token, buttons);
            }
        }
    }
}