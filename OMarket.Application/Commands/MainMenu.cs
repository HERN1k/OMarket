using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.MAINMENU)]
    public class MainMenu : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;

        public MainMenu(
                IUpdateManager updateManager,
                ISendResponseService response,
                II18nService i18n,
                IInlineMarkupService inlineMarkup
            )
        {
            _updateManager = updateManager;
            _response = response;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            InlineKeyboardMarkup buttons = await _inlineMarkup
                .MainMenu(token);

            if (StringHelper.IsBackCommand(_updateManager.Update))
            {
                await _response.EditMessageMarkup(buttons, token);

                return;
            }

            await _response.SendMessageAnswer(_i18n.T("generic_main_manu_title"), token, buttons);
        }
    }
}