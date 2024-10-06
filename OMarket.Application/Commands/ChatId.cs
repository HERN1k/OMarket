using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Domain.TgCommand;

using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.CHATID)]
    public class ChatId : ITgCommand
    {
        private readonly ISendResponseService _response;
        private readonly IUpdateManager _updateManager;

        public ChatId(ISendResponseService response, IUpdateManager updateManager)
        {
            _response = response;
            _updateManager = updateManager;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            long chatId;
            if (_updateManager.Update.Type == UpdateType.Message)
            {
                token.ThrowIfCancellationRequested();

                if (_updateManager.Update.Message is null)
                {
                    throw new TelegramException();
                }

                chatId = _updateManager.Update.Message.Chat.Id;
            }
            else if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                token.ThrowIfCancellationRequested();

                if (_updateManager.Update.CallbackQuery is null ||
                    _updateManager.Update.CallbackQuery.Message is null)
                {
                    throw new TelegramException();
                }

                chatId = _updateManager.Update.CallbackQuery.Message.Chat.Id;
            }
            else
            {
                throw new TelegramException();
            }

            await _response.SendMessageAnswer($"Telegram chat ID: {chatId}", token);
        }
    }
}