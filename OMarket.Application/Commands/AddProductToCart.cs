using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Cart;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;

using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.ADDPRODUCTTOCART)]
    public class AddProductToCart : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IStaticCollectionsService _staticCollections;
        private readonly IProductsRepository _productsRepository;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly ICartService _cartService;

        public AddProductToCart(
                IUpdateManager updateManager,
                ISendResponseService response,
                IStaticCollectionsService staticCollections,
                IProductsRepository productsRepository,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                ICartService cartService
            )
        {
            _updateManager = updateManager;
            _response = response;
            _staticCollections = staticCollections;
            _productsRepository = productsRepository;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
            _cartService = cartService;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            RequestInfo request = await _dataProcessor.MapRequestData(token);

            if (request.Customer.CityId == null || request.Customer.StoreAddressId == null)
            {
                await _response.SendMessageAnswer(
                    text: _i18n.T("main_menu_command_select_your_address"),
                    token: token,
                    buttons: _inlineMarkup.SelectStoreAddress("updatestoreaddress"));

                return;
            }

            string[] queryLines = request.Query.Split('_');

            if (queryLines.Length != 3)
            {
                throw new TelegramException();
            }

            if (!int.TryParse(queryLines[1], out int quantity))
            {
                throw new TelegramException();
            }

            if (!Guid.TryParse(queryLines[0], out Guid productId))
            {
                throw new TelegramException();
            }

            if (!int.TryParse(queryLines[2], out int pageNumber))
            {
                throw new TelegramException();
            }

            Guid underTypeId = await _cartService.AddProductsToCartAsync(
                customerId: request.Customer.Id,
                quantity: quantity,
                productId: productId,
                token: token);

            await _response.SendCallbackAnswerAlert("Товар успешно добавлен в корзину!", token);
            //$"/1024_{dto.Product.UnderTypeId}_{dto.PageNumber}_0")

            ProductWithDbInfoDto? dto = await _productsRepository
                .GetProductWithPaginationAsync(pageNumber, underTypeId.ToString(), token)
                    ?? throw new TelegramException();

            InlineKeyboardMarkup buttons = _inlineMarkup.ProductView(dto, 0);

            await _response.EditMessageMarkup(buttons, token);
        }
    }
}