using Telegram.Bot.Types;

namespace OMarket.Domain.Interfaces.Application.Services.TgUpdate
{
    public interface IUpdateManager
    {
        Update Update { get; set; }

        CallbackQuery CallbackQuery { get; }

        Message Message { get; }
    }
}