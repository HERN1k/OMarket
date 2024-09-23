
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
    [TgCommand(TgCommands.UPDATESTOREADDRESS)]
    public class UpdateStoreAddress : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IDataProcessorService _dataProcessor;
        private readonly IStaticCollectionsService _staticCollections;
        private readonly ICustomersRepository _customersRepository;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;

        public UpdateStoreAddress(
                IUpdateManager updateManager,
                ISendResponseService response,
                IDataProcessorService dataProcessor,
                IStaticCollectionsService staticCollections,
                ICustomersRepository customersRepository,
                II18nService i18n,
                IInlineMarkupService inlineMarkup
            )
        {
            _updateManager = updateManager;
            _response = response;
            _dataProcessor = dataProcessor;
            _staticCollections = staticCollections;
            _customersRepository = customersRepository;
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

            if (request is null || string.IsNullOrEmpty(request.Query))
            {
                throw new TelegramException();
            }

            if (!_staticCollections.CitiesWithStoreAddressesDictionary.TryGetValue(request.Query, out var storeAddress))
            {
                throw new TelegramException();
            }

            await _customersRepository.SaveStoreAddressAsync(
                id: request.Customer.Id,
                city: storeAddress.City,
                address: storeAddress.Address,
                token: token);

            await _response.EditLastMessage(_i18n.T("update_store_address_command_address_is_saved"), token, _inlineMarkup.Empty);

            InlineKeyboardMarkup buttons = await _inlineMarkup.MainMenu(token);

            await _response.SendMessageAnswer(_i18n.T("generic_main_manu_title"), token, buttons);
        }
    }
}