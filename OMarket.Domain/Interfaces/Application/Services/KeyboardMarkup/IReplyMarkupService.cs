using OMarket.Domain.Enums;

using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup
{
    public interface IReplyMarkupService
    {
        IReplyMarkup Empty { get; }

        ReplyKeyboardMarkup SendPhoneNumber(LanguageCode? code = null);
    }
}