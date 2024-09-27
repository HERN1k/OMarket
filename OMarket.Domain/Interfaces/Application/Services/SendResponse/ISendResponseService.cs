using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Domain.Interfaces.Application.Services.SendResponse
{
    public interface ISendResponseService
    {
        Task<Message> SendMessageAnswer(string text, CancellationToken token, IReplyMarkup? buttons = null, ParseMode? parseMode = ParseMode.Html);

        Task SendCallbackAnswer(string text, CancellationToken token);

        Task SendCallbackAnswer(CancellationToken token);

        Task SendCallbackAnswerAlert(CancellationToken token);

        Task SendCallbackAnswerAlert(string text, CancellationToken token);

        Task<Message> EditLastMessage(string text, CancellationToken token, InlineKeyboardMarkup? buttons = null, ParseMode? parseMode = ParseMode.Html);

        Task<Message> EditMessageMarkup(InlineKeyboardMarkup buttons, CancellationToken token);

        Task<Message> SendPhotoWithTextAndButtons(string text, Uri photoUri, IReplyMarkup buttons, CancellationToken token, ParseMode? parseMode = ParseMode.Html);

        Task RemoveLastMessage(CancellationToken token);

        Task RemoveMessageById(int messageId, CancellationToken token);

        Task SendLocation(double latitude, double longitude, CancellationToken token, InlineKeyboardMarkup? buttons = null);
    }
}