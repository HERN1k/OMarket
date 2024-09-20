using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;

using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.DEV)]
    public class Dev : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly II18nService _i18n;

        public Dev(
                IUpdateManager updateManager,
                ISendResponseService response,
                II18nService i18n
            )
        {
            _updateManager = updateManager;
            _response = response;
            _i18n = i18n;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                await _response.SendCallbackAnswer(token);
            }

            await _response.SendMessageAnswer(_i18n.T("exception_dev"), token);
        }
    }
}