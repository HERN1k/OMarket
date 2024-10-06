using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using OMarket.Domain.Interfaces.Application.Services.Bot;
using OMarket.Domain.Settings;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace OMarket.Application.Services.Bot
{
    public class BotService : IBotService
    {
        public ITelegramBotClient Client { get; init; }

        private readonly WebhookSettings _webhookSettings;

        private readonly ILogger<BotHostedService> _logger;

        public BotService(
                IOptions<WebhookSettings> webhookSettings,
                ITelegramBotClient botClient,
                ILogger<BotHostedService> logger
            )
        {
            _webhookSettings = webhookSettings.Value;
            Client = botClient;
            _logger = logger;
        }

        public async Task InitializeBotAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Starting bot configuration and launch...");

            _logger.LogInformation("Webhook is being installed...");

            var webhookUrl = Environment.GetEnvironmentVariable("WEBHOOK_URL");

            if (string.IsNullOrEmpty(webhookUrl))
            {
                throw new ArgumentNullException("WEBHOOK_URL", "The WEBHOOK_URL string environment variable is not set.");
            }

            await Client.SetWebhookAsync(
                    url: webhookUrl,
                    dropPendingUpdates: _webhookSettings.DropPendingUpdates,
                    cancellationToken: cancellationToken
                );

            _logger.LogInformation("Webhook url: {Url}", webhookUrl);
            _logger.LogInformation("Drop pending updates: {DropPendingUpdates}", _webhookSettings.DropPendingUpdates);
            _logger.LogInformation("Webhook has been installed.");

            _logger.LogInformation("Commands are being added...");

            BotCommand[] commands = new BotCommand[]
            {
                new() { Command = "start", Description = "Почнемо 🍻" },
                new() { Command = "mainmenu", Description = "Головне меню" }
            };

            await Client.SetMyCommandsAsync(commands, cancellationToken: cancellationToken);

            foreach (BotCommand command in commands)
            {
                _logger.LogInformation("Command: /{Command}", command.Command);
            }

            _logger.LogInformation("Commands have been added.");

            User me = await Client.GetMeAsync(cancellationToken);

            _logger.LogInformation("Bot information:");

            _logger.LogInformation("Bot ID: {Id}", me.Id);

            if (!string.IsNullOrEmpty(me.FirstName))
            {
                _logger.LogInformation("Bot Name: {FirstName}", me.FirstName);
            }

            if (!string.IsNullOrEmpty(me.Username))
            {
                _logger.LogInformation("Bot Username: @{Username}", me.Username);
                _logger.LogInformation("Bot link: https://t.me/{Username}", me.Username);
            }

            _logger.LogInformation("The bot was configured and launched.");
        }
    }
}