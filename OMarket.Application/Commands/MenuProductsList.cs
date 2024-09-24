using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;

using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.MENUPRODUCTSLIST)]
    public class MenuProductsList : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IProductsRepository _productsRepository;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;

        public MenuProductsList(
                IUpdateManager updateManager,
                ISendResponseService response,
                IProductsRepository productsRepository,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup
            )
        {
            _updateManager = updateManager;
            _response = response;
            _productsRepository = productsRepository;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
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

            string[] queryLines = request.Query.Split('_');

            if (queryLines.Length != 2)
            {
                throw new TelegramException();
            }

            if (!int.TryParse(queryLines[1], out int pageNumber))
            {
                throw new TelegramException();
            }

            if (request.Customer.StoreId == Guid.Empty)
            {
                throw new TelegramException();
            }

            ProductWithDbInfoDto? dto = await _productsRepository
                .GetProductWithPaginationAsync(pageNumber, queryLines[0], (Guid)request.Customer.StoreId, token);

            if (dto is null || dto.Product is null)
            {
                await _response.SendCallbackAnswerAlert(_i18n.T("generic_menu_null_item"), token);

                return;
            }

            if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                await _response.SendCallbackAnswer(token);
            }

            InlineKeyboardMarkup buttons = _inlineMarkup.ProductView(dto, 0);

            string text;
            if (dto.Product.Status)
            {
                text = $"""
                    <b>{dto.Product.Name}</b>, <i>{dto.Product.Dimensions}</i>

                    💵 {dto.Product.Price} грн.

                    <i>{dto.Product.Description}</i> 
                    """;
            }
            else
            {
                text = $"""
                    <b>{dto.Product.Name}</b>, <i>{dto.Product.Dimensions}</i>

                    <b>{_i18n.T("product_view_product_is_not_available")}</b>

                    <i>{dto.Product.Description}</i> 
                    """;
            }

            var appUri = Environment.GetEnvironmentVariable("HTTPS_APPLICATION_URL");

            if (string.IsNullOrEmpty(appUri))
            {
                throw new ArgumentNullException("HTTPS_APPLICATION_URL", "The 'HTTPS_APPLICATION_URL' string environment variable is not set.");
            }

            Uri uri = new(appUri + dto.Product.PhotoUri);

            await _response.RemoveLastMessage(token);

            await _response.SendPhotoWithTextAndButtons(
                text: text,
                photoUri: uri,
                buttons: buttons,
                token: token);
        }
    }
}