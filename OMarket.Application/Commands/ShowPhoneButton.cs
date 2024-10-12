using AutoMapper;

using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.SHOWPHONEBUTTON)]
    public class ShowPhoneButton : ITgCommand
    {
        private readonly ISendResponseService _response;
        private readonly IUpdateManager _updateManager;
        private readonly II18nService _i18n;
        private readonly IMapper _mapper;
        private readonly IReplyMarkupService _replyMarkup;
        private readonly IInlineMarkupService _inlineMarkup;

        public ShowPhoneButton(
                ISendResponseService response,
                IUpdateManager updateManager,
                II18nService i18n,
                IMapper mapper,
                IReplyMarkupService replyMarkup,
                IInlineMarkupService inlineMarkup
            )
        {
            _response = response;
            _updateManager = updateManager;
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

            string greeting = $"""
                    <b>{_i18n.T("start_command_greeting_phrase_1")
                        + (customer.FirstName != null ? " " + customer.FirstName : string.Empty)}!</b>

                    {_i18n.T("start_command_greeting_phrase_2")}

                    <b>{_i18n.T("start_command_greeting_phrase_3")}
                    {_i18n.T("start_command_greeting_phrase_4")}
                    {_i18n.T("start_command_greeting_phrase_5")}
                    {_i18n.T("start_command_greeting_phrase_6")}
                    {_i18n.T("start_command_greeting_phrase_7")}
                    {_i18n.T("start_command_greeting_phrase_8")}</b>

                    <i>{_i18n.T("start_command_share_your_phone_number_1")}
                    
                    {_i18n.T("start_command_share_your_phone_number_2")}
                    
                    {_i18n.T("start_command_show_phone")}</i>
                    """;

            await _response.RemoveLastMessage(token);

            Message message = await _response.SendMessageAnswer(greeting, token, _replyMarkup.SendPhoneNumber());

            await _response.EditMessageMarkupById(message.MessageId, _inlineMarkup.ShowPhoneButton(), token);
        }
    }
}