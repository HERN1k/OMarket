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
    [TgCommand(TgCommands.VIEWSEARCHPRODUCT)]
    public class ViewSearchProduct : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly IProductsRepository _productsRepository;

        public ViewSearchProduct(
                IUpdateManager updateManager,
                ISendResponseService response,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                IProductsRepository productsRepository
            )
        {
            _updateManager = updateManager;
            _response = response;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
            _productsRepository = productsRepository;
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

            if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                await _response.SendCallbackAnswer(token);
            }

            string[] queryLines = request.Query.Split('_');

            if (queryLines.Length != 2)
            {
                throw new TelegramException();
            }

            if (!Guid.TryParse(queryLines[0], out Guid productId))
            {
                throw new TelegramException();
            }

            if (!int.TryParse(queryLines[1], out int quantity))
            {
                throw new TelegramException();
            }

            if (productId == Guid.Empty)
            {
                throw new TelegramException();
            }

            ProductDto product = await _productsRepository.GetProductByIdAsync(productId, token);

            InlineKeyboardMarkup buttons = _inlineMarkup.ProductViewBySearch(product, quantity);

            string text = $"""
                <b>{product.Name}</b>, <i>{product.Dimensions}</i>

                💵 {product.Price} грн.

                <i>{product.Description}</i> 
                """;

            var appUri = Environment.GetEnvironmentVariable("HTTPS_APPLICATION_URL");

            if (string.IsNullOrEmpty(appUri))
            {
                throw new ArgumentNullException("HTTPS_APPLICATION_URL", "The 'HTTPS_APPLICATION_URL' string environment variable is not set.");
            }

            if (_updateManager.CallbackQuery.Message != null &&
                _updateManager.CallbackQuery.Message.Photo != null &&
                _updateManager.CallbackQuery.Message.Photo.Length != 0)
            {
                await _response.EditMessageMarkup(buttons, token);
            }
            else
            {
                Uri uri = new(appUri + product.PhotoUri);

                await _response.RemoveLastMessage(token);

                await _response.SendPhotoWithTextAndButtons(
                    text: text,
                    photoUri: uri,
                    buttons: buttons,
                    token: token);
            }
        }
    }
}