using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Bot;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Services.SendResponse
{
    public class SendResponseService : ISendResponseService
    {
        private readonly ITelegramBotClient _client;

        private readonly IUpdateManager _updateManager;

        public SendResponseService(
                IBotService bot,
                IUpdateManager updateManager
            )
        {
            _client = bot.Client;
            _updateManager = updateManager;
        }

        public async Task<Message> SendMessageAnswer(string text, CancellationToken token, IReplyMarkup? buttons = null, ParseMode? parseMode = ParseMode.Html)
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

            return await _client.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                replyMarkup: buttons,
                parseMode: parseMode,
                cancellationToken: token);
        }

        public async Task SendCallbackAnswer(string text, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_updateManager.Update.CallbackQuery is null)
            {
                throw new TelegramException();
            }

            await _client.AnswerCallbackQueryAsync(
                callbackQueryId: _updateManager.Update.CallbackQuery.Id,
                text: text,
                cancellationToken: token);
        }

        public async Task SendCallbackAnswer(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_updateManager.Update.CallbackQuery is null)
            {
                throw new TelegramException();
            }

            await _client.AnswerCallbackQueryAsync(
                callbackQueryId: _updateManager.Update.CallbackQuery.Id,
                cancellationToken: token);
        }

        public async Task SendCallbackAnswerAlert(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_updateManager.Update.CallbackQuery is null)
            {
                throw new TelegramException();
            }

            await _client.AnswerCallbackQueryAsync(
                callbackQueryId: _updateManager.Update.CallbackQuery.Id,
                text: _updateManager.Update.CallbackQuery.Data ?? "🤩",
                showAlert: false,
                cacheTime: 0,
                cancellationToken: token);
        }

        public async Task SendCallbackAnswerAlert(string text, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_updateManager.Update.CallbackQuery is null)
            {
                throw new TelegramException();
            }

            await _client.AnswerCallbackQueryAsync(
                callbackQueryId: _updateManager.Update.CallbackQuery.Id,
                text: text ?? "🤩",
                showAlert: false,
                cacheTime: 0,
                cancellationToken: token);
        }

        public async Task<Message> EditLastMessage(string text, CancellationToken token, InlineKeyboardMarkup? buttons = null, ParseMode? parseMode = ParseMode.Html)
        {
            token.ThrowIfCancellationRequested();

            long chatId;
            int messageId;
            if (_updateManager.Update.Type == UpdateType.Message)
            {
                token.ThrowIfCancellationRequested();

                if (_updateManager.Update.Message is null)
                {
                    throw new TelegramException();
                }

                chatId = _updateManager.Update.Message.Chat.Id;
                messageId = _updateManager.Update.Message.MessageId;
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
                messageId = _updateManager.Update.CallbackQuery.Message.MessageId;
            }
            else
            {
                throw new TelegramException();
            }

            return await _client.EditMessageTextAsync(
                chatId: chatId,
                messageId: messageId,
                text: text,
                replyMarkup: buttons,
                parseMode: parseMode,
                cancellationToken: token);
        }

        public async Task<Message> EditMessageMarkup(InlineKeyboardMarkup buttons, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            long chatId;
            int messageId;
            if (_updateManager.Update.Type == UpdateType.Message)
            {
                token.ThrowIfCancellationRequested();

                if (_updateManager.Update.Message is null)
                {
                    throw new TelegramException();
                }

                chatId = _updateManager.Update.Message.Chat.Id;
                messageId = _updateManager.Update.Message.MessageId;
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
                messageId = _updateManager.Update.CallbackQuery.Message.MessageId;
            }
            else
            {
                throw new TelegramException();
            }

            return await _client.EditMessageReplyMarkupAsync(
                chatId: chatId,
                messageId: messageId,
                replyMarkup: buttons,
                cancellationToken: token);
        }

        public async Task<Message> EditMessageMarkup(Message message, InlineKeyboardMarkup buttons, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return await _client.EditMessageReplyMarkupAsync(
                chatId: message.Chat.Id,
                messageId: message.MessageId,
                replyMarkup: buttons,
                cancellationToken: token);
        }

        public async Task<Message> SendPhotoWithTextAndButtons(string text, Uri photoUri, IReplyMarkup buttons, CancellationToken token, ParseMode? parseMode = ParseMode.Html)
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

            await _client.SendChatActionAsync(chatId, ChatAction.UploadPhoto, cancellationToken: token);

            return await _client.SendPhotoAsync(
                chatId: chatId,
                photo: new InputFileUrl(photoUri),
                caption: text,
                replyMarkup: buttons,
                parseMode: parseMode,
                cancellationToken: token);
        }

        public async Task RemoveLastMessage(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            long chatId;
            int messageId;
            if (_updateManager.Update.Type == UpdateType.Message)
            {
                token.ThrowIfCancellationRequested();

                if (_updateManager.Update.Message is null)
                {
                    throw new TelegramException();
                }

                chatId = _updateManager.Update.Message.Chat.Id;
                messageId = _updateManager.Update.Message.MessageId;
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
                messageId = _updateManager.Update.CallbackQuery.Message.MessageId;
            }
            else
            {
                throw new TelegramException();
            }

            await _client.DeleteMessageAsync(
                chatId: chatId,
                messageId: messageId,
                cancellationToken: token);
        }

        public async Task RemoveMessageById(int messageId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (messageId <= 0)
            {
                throw new TelegramException();
            }

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

            await _client.DeleteMessageAsync(
                chatId: chatId,
                messageId: messageId,
                cancellationToken: token);
        }

        public async Task SendLocation(double latitude, double longitude, CancellationToken token, InlineKeyboardMarkup? buttons = null)
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

            await _client.SendLocationAsync(
                chatId: chatId,
                latitude: latitude,
                longitude: longitude,
                replyMarkup: buttons);
        }
    }
}