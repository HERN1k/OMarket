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

using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.SENDSTORECONTACTS)]
    public class SendStoreContacts : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IDataProcessorService _dataProcessor;
        private readonly IStaticCollectionsService _staticCollections;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;

        public SendStoreContacts(
                IUpdateManager updateManager,
                ISendResponseService response,
                IDataProcessorService dataProcessor,
                IStaticCollectionsService staticCollections,
                II18nService i18n,
                IInlineMarkupService inlineMarkup
            )
        {
            _updateManager = updateManager;
            _response = response;
            _dataProcessor = dataProcessor;
            _staticCollections = staticCollections;
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

            if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                await _response.SendCallbackAnswer(token);
            }

            string[] queryLines = request.Query.Split('_');

            if (queryLines.Length != 1)
            {
                throw new TelegramException();
            }

            if (!Guid.TryParse(queryLines[0], out Guid storeId))
            {
                throw new TelegramException();
            }

            StoreDto store = _staticCollections.StoresSet
                .FirstOrDefault(store => store.Id == storeId)
                    ?? throw new TelegramException();

            if (!_staticCollections.CitiesWithStoreAddressesDictionary
                    .TryGetValue(queryLines[0], out var address))
            {
                throw new TelegramException();
            }

            string text = $"""
                {_i18n.T("main_menu_command_contacts_button")}

                <b>{_i18n.T("store_contacts_command_store_city")}</b> <i>{address.City}</i>

                <b>{_i18n.T("store_contacts_command_store_address")}</b> <i>{address.Address}</i>

                <b>{_i18n.T("store_contacts_command_phone_number")}</b> <i>{store.PhoneNumber}</i>
                """;

            await _response.EditLastMessage(text, token, _inlineMarkup.ToMainMenuBack());
        }
    }
}
