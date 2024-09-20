using Telegram.Bot;

namespace OMarket.Domain.Interfaces.Application.Services.Bot
{
    public interface IBotService
    {
        ITelegramBotClient Client { get; }
        Task InitializeBotAsync(CancellationToken cancellationToken);
    }
}