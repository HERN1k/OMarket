using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
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
        private readonly IStaticCollectionsService _staticCollections;
        private readonly IProductsRepository _productsRepository;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;

        public MenuProductsList(
                IUpdateManager updateManager,
                ISendResponseService response,
                IStaticCollectionsService staticCollections,
                IProductsRepository productsRepository,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup
            )
        {
            _updateManager = updateManager;
            _response = response;
            _staticCollections = staticCollections;
            _productsRepository = productsRepository;
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

            if (request.Customer.CityId == null || request.Customer.StoreAddressId == null)
            {
                await _response.EditLastMessage(
                    text: _i18n.T("main_menu_command_select_your_address"),
                    token: token,
                    buttons: _inlineMarkup.SelectStoreAddress("updatestoreaddress"));

                return;
            }

            List<ProductDto> products = await _productsRepository
                .GetProductWithPaginationAsync(1, request.Query, token);

            if (products.Count < 1)
            {
                throw new TelegramException();
            }

            InlineKeyboardMarkup buttons = _inlineMarkup.ProductView(3);

            UriBuilder uriBuilder = new UriBuilder(products[0].PhotoUri);

            //uriBuilder.Path = Uri.EscapeDataString(uriBuilder.Path);
            Console.WriteLine($"{uriBuilder.Uri}");
            await _response.SendPhotoWithTextAndButtons(
                text: products[0].Description ?? "Пусто :)",
                photoUri: uriBuilder.Uri,
                //photoUri: new Uri(Uri.EscapeDataString(products[0].PhotoUri)),//new Uri(products[0].PhotoUri),
                //photoUri: new Uri("https://omarket.ua/image/cache/webp/catalog/products/beer/obolon-light-1l-700x700.webp"),
                buttons: buttons,
                token: token);
        }
    }
}