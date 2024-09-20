using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Services.TgUpdate
{
    public class UpdateManager : IUpdateManager
    {
        #region Public properties
        public Update Update
        {
            get => _update ?? throw new TelegramException();
            set => _update = value ?? throw new TelegramException();
        }

        public CallbackQuery CallbackQuery
        {
            get
            {
                if (_update is null)
                {
                    throw new InvalidOperationException(nameof(_update));
                }

                if (_update.CallbackQuery is null)
                {
                    throw new InvalidOperationException(nameof(_update));
                }

                return _update.CallbackQuery;
            }
        }

        public Message Message
        {
            get
            {
                if (_update is null)
                {
                    throw new TelegramException();
                }

                if (_update.Type == UpdateType.Message)
                {
                    if (_update.Message is null)
                    {
                        throw new TelegramException();
                    }

                    return _update.Message;
                }
                else if (_update.Type == UpdateType.CallbackQuery)
                {
                    if (_update.CallbackQuery is null ||
                        _update.CallbackQuery.Message is null)
                    {
                        throw new TelegramException();
                    }

                    return _update.CallbackQuery.Message;
                }
                else
                {
                    throw new TelegramException();
                }
            }
        }
        #endregion

        #region Private properties
        private Update? _update = null;
        #endregion
    }
}