using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.SAVECHATFORSTORE)]
    public class SaveChatForStore : ITgCommand
    {
        private readonly ISendResponseService _response;
        private readonly IUpdateManager _updateManager;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IAdminsRepository _adminsRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IStaticCollectionsService _staticCollections;

        public SaveChatForStore(
                ISendResponseService response,
                IUpdateManager updateManager,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IAdminsRepository adminsRepository,
                IStoreRepository storeRepository,
                IStaticCollectionsService staticCollections
            )
        {
            _response = response;
            _updateManager = updateManager;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _adminsRepository = adminsRepository;
            _storeRepository = storeRepository;
            _staticCollections = staticCollections;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            RequestInfo request = await _dataProcessor.MapRequestData(token);

            Guid? adminId = await _adminsRepository
                .VerifyAdminByIdAsync(request.Customer.Id, token);

            if (adminId == null || adminId == Guid.Empty)
            {
                throw new TelegramException();
            }

            long? chatId = (_updateManager.Update.Message?.Chat.Id)
                ?? throw new TelegramException();

            string storeId = await _storeRepository
                .SetStoreChatIdAsync((Guid)adminId, (long)chatId, token);

            if (!_staticCollections.CitiesWithStoreAddressesDictionary
                    .TryGetValue(storeId, out var storeDto))
            {
                throw new TelegramException();
            }

            string text = $"""
                {_i18n.T("admins_command_chat_saved_for_store_at")}

                <b>{storeDto.City} {storeDto.Address}</b>

                <i>{_i18n.T("admins_command_if_address_is_not_correct")}</i>
                """;

            await _response.EditLastMessage(text, token);
        }
    }
}