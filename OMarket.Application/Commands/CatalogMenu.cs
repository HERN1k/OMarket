using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Domain.TgCommand;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.CATALOGMENU)]
    public class CatalogMenu : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IInlineMarkupService _inlineMarkup;

        public CatalogMenu(
                IUpdateManager updateManager,
                ISendResponseService response,
                IInlineMarkupService inlineMarkup
            )
        {
            _updateManager = updateManager;
            _response = response;
            _inlineMarkup = inlineMarkup;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await _response.EditMessageMarkup(_inlineMarkup.CatalogMenu(), token);
        }
    }
}