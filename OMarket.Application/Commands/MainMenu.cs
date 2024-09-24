using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.MAINMENU)]
    public class MainMenu : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;

        public MainMenu(
                IUpdateManager updateManager,
                ISendResponseService response,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup
            )
        {
            _updateManager = updateManager;
            _response = response;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                await _response.SendCallbackAnswer(token);
            }

            RequestInfo request = await _dataProcessor.MapRequestData(token);

            if (request.Customer.StoreId == null)
            {
                await _response.SendMessageAnswer(_i18n.T("main_menu_command_select_your_address"), token, _inlineMarkup.SelectStoreAddress("updatestoreaddress"));

                return;
            }

            InlineKeyboardMarkup buttons = await _inlineMarkup.MainMenu(token);

            if (StringHelper.IsBackCommand(_updateManager.Update, out string command))
            {
                if (StringHelper.IsDelCommand(command))
                {
                    await _response.RemoveLastMessage(token);

                    await _response.SendMessageAnswer(_i18n.T("generic_main_manu_title"), token, buttons);

                    return;
                }

                await _response.EditLastMessage(_i18n.T("generic_main_manu_title"), token, buttons);

                return;
            }

            await _response.SendMessageAnswer(_i18n.T("generic_main_manu_title"), token, buttons);
        }
    }
}