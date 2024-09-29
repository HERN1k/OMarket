using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;

using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.CHANGEORDERSTATUS)]
    public class ChangeOrderStatus : ITgCommand
    {
        private readonly ISendResponseService _response;
        private readonly IUpdateManager _updateManager;
        private readonly IDataProcessorService _dataProcessor;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IStaticCollectionsService _staticCollections;

        public ChangeOrderStatus(
                ISendResponseService response,
                IUpdateManager updateManager,
                IDataProcessorService dataProcessor,
                IInlineMarkupService inlineMarkup,
                IOrdersRepository ordersRepository,
                IStaticCollectionsService staticCollections
            )
        {
            _response = response;
            _updateManager = updateManager;
            _dataProcessor = dataProcessor;
            _inlineMarkup = inlineMarkup;
            _ordersRepository = ordersRepository;
            _staticCollections = staticCollections;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            RequestInfo request = await _dataProcessor.MapRequestData(token);

            if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                await _response.SendCallbackAnswer(token);
            }

            if (string.IsNullOrEmpty(request.Query))
            {
                throw new TelegramException();
            }

            string[] queryLines = request.Query.Split('_', 2);

            if (queryLines.Length == 1)
            {
                if (string.IsNullOrEmpty(queryLines[0]))
                {
                    throw new TelegramException();
                }

                InlineKeyboardMarkup buttons = _inlineMarkup.ChangeOrderStatus(queryLines[0]);

                await _response.EditMessageMarkup(buttons, token);
            }
            else if (queryLines.Length == 2)
            {
                if (string.IsNullOrEmpty(queryLines[0]) || string.IsNullOrEmpty(queryLines[1]))
                {
                    throw new TelegramException();
                }

                if (!Guid.TryParse(queryLines[0], out Guid orderId))
                {
                    throw new TelegramException();
                }

                if (orderId == Guid.Empty)
                {
                    throw new TelegramException();
                }

                if (!int.TryParse(queryLines[1], out int statusIndex))
                {
                    throw new TelegramException();
                }

                if (!_staticCollections.OrderStatusesDictionary
                        .TryGetValue(statusIndex, out var status))
                {
                    throw new TelegramException();
                }

                await _ordersRepository.ChangeOrderStatusAsync(
                    status: status,
                    orderId: orderId,
                    token: token);

                InlineKeyboardMarkup buttons = _inlineMarkup.MarkupOrderForStoreChat(
                    status: status,
                    orderId: orderId);

                await _response.EditMessageMarkup(buttons, token);
            }
            else
            {
                throw new TelegramException();
            }
        }
    }
}