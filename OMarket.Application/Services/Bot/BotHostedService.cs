
using Microsoft.Extensions.Hosting;

using OMarket.Domain.Interfaces.Application.Services.Bot;

namespace OMarket.Application.Services.Bot
{
    public class BotHostedService : IHostedService
    {
        private readonly IBotService _botService;

        public BotHostedService(IBotService botService)
        {
            _botService = botService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _botService.InitializeBotAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}