using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Domain.Exceptions.Telegram
{
    public class TelegramException : Exception
    {
        #region Public properties
        public string ExceptionMessage { get => _exceptionMessage; }

        public InlineKeyboardMarkup? Buttons { get => _buttons; }
        #endregion

        #region Private properties
        private string _exceptionMessage = string.Empty;

        private InlineKeyboardMarkup? _buttons;
        #endregion

        #region Constructors
        public TelegramException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
            _exceptionMessage = message;
        }

        public TelegramException(string message, InlineKeyboardMarkup buttons, Exception? innerException = null)
            : base(message, innerException)
        {
            _exceptionMessage = message;
            _buttons = buttons;
        }

        public TelegramException(Exception? innerException = null)
            : base(string.Empty, innerException)
        { }
        #endregion
    }
}