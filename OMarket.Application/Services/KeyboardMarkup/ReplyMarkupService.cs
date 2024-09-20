using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Translator;

using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Services.KeyboardMarkup
{
    public class ReplyMarkupService : IReplyMarkupService
    {
        public IReplyMarkup Empty { get => new ReplyKeyboardRemove(); }

        private readonly II18nService _i18n;

        public ReplyMarkupService(II18nService i18n)
        {
            _i18n = i18n;
        }

        public ReplyKeyboardMarkup SendPhoneNumber(LanguageCode? code = null)
        {
            ReplyKeyboardMarkup markup = new(
                new[] { new KeyboardButton(_i18n.T("start_command_send_phone_number_button", code)) { RequestContact = true } })
            {
                ResizeKeyboard = true
            };

            return markup;
        }
    }
}