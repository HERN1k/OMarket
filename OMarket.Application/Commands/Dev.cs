using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.DEV)]
    public class Dev : ITgCommand
    {
        private readonly ISendResponseService _response;
        private readonly II18nService _i18n;

        public Dev(
                ISendResponseService response,
                II18nService i18n
            )
        {
            _response = response;
            _i18n = i18n;
        }

        public async Task InvokeAsync(CancellationToken token) =>
            await _response.SendCallbackAnswerAlert(_i18n.T("exception_dev"), token);
    }
}