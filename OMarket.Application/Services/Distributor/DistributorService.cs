using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Distributor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OMarket.Application.Services.Distributor
{
    public class DistributorService : IDistributorService
    {
        private readonly IUpdateManager _updateManager;

        private readonly ISendResponseService _response;

        private readonly IServiceProvider _serviceProvider;

        private readonly IStaticCollectionsService _staticCollections;

        private readonly IDistributedCache _distributedCache;

        private readonly ILogger<DistributorService> _logger;

        public DistributorService(
                IUpdateManager updateManager,
                ISendResponseService response,
                IServiceProvider serviceProvider,
                IStaticCollectionsService staticCollections,
                IDistributedCache distributedCache,
                ILogger<DistributorService> logger
            )
        {
            _updateManager = updateManager;
            _response = response;
            _serviceProvider = serviceProvider;
            _staticCollections = staticCollections;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task Distribute(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_staticCollections.CommandsDictionary == null || _staticCollections.CommandsDictionary.Count == 0)
            {
                _logger.LogCritical("Static Collection 'CommandMap' is null or empty!");
                throw new TelegramException();
            }

            if (_updateManager.Update.Message?.Contact != null && _updateManager.Update.Type == UpdateType.Message)
            {
                if (!_staticCollections.CommandsDictionary.TryGetValue(TgCommands.SAVECONTACT, out var commandType))
                {
                    throw new TelegramException();
                }

                token.ThrowIfCancellationRequested();

                if (typeof(ITgCommand).IsAssignableFrom(commandType))
                {
                    token.ThrowIfCancellationRequested();

                    ITgCommand command = (ITgCommand)_serviceProvider.GetRequiredService(commandType);

                    await command.InvokeAsync(token);
                }
                else
                {
                    throw new TelegramException();
                }
            }
            else if (_updateManager.Update.Message is not null &&
                    _updateManager.Update.Message.Text is not null &&
                    !_updateManager.Update.Message.Text.StartsWith('/') &&
                    _updateManager.Update.Type == UpdateType.Message &&
                    _updateManager.Update.Message.Type == MessageType.Text)
            {
                if (_updateManager.Update.Message is null ||
                    _updateManager.Update.Message.From is null)
                {
                    throw new TelegramException();
                }

                string? customerFreeInputString = await _distributedCache
                    .GetStringAsync($"{CacheKeys.CustomerFreeInputId}{_updateManager.Update.Message.From.Id}", token);

                if (string.IsNullOrEmpty(customerFreeInputString))
                {
                    throw new TelegramException();
                }

                if (!customerFreeInputString.StartsWith('/'))
                {
                    throw new TelegramException();
                }

                string[] queryLines = customerFreeInputString.Split('_', 2);

                if (queryLines.Length < 2)
                {
                    throw new TelegramException();
                }

                TgCommands messageCommand = TgCommandExtensions
                    .GetTelegramCommand(queryLines[0][1..]);

                token.ThrowIfCancellationRequested();

                if (messageCommand == TgCommands.NONE)
                {
                    throw new TelegramException();
                }

                if (!_staticCollections.CommandsDictionary.TryGetValue(messageCommand, out var commandType))
                {
                    throw new TelegramException();
                }

                token.ThrowIfCancellationRequested();

                if (typeof(ITgCommand).IsAssignableFrom(commandType))
                {
                    token.ThrowIfCancellationRequested();

                    ITgCommand command = (ITgCommand)_serviceProvider.GetRequiredService(commandType);

                    await command.InvokeAsync(token);
                }
                else
                {
                    throw new TelegramException();
                }
            }
            else if (_updateManager.Update.Type == UpdateType.Message || _updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                token.ThrowIfCancellationRequested();

                if (!IsCommand(token))
                {
                    token.ThrowIfCancellationRequested();

                    if (_updateManager.Update.Type == UpdateType.Message)
                    {
                        throw new TelegramException();
                    }
                    else if (_updateManager.Update.Type == UpdateType.CallbackQuery)
                    {
                        await _response.SendCallbackAnswerAlert(token);

                        return;
                    }
                    else
                    {
                        throw new TelegramException();
                    }
                }

                string[] rawCommand = GetCommandsAndArguments(token);

                TgCommands messageCommand = TgCommandExtensions
                    .GetTelegramCommand(rawCommand[0]);

                token.ThrowIfCancellationRequested();

                if (messageCommand == TgCommands.NONE)
                {
                    throw new TelegramException();
                }

                if (!_staticCollections.CommandsDictionary.TryGetValue(messageCommand, out var commandType))
                {
                    throw new TelegramException();
                }

                token.ThrowIfCancellationRequested();

                if (typeof(ITgCommand).IsAssignableFrom(commandType))
                {
                    token.ThrowIfCancellationRequested();

                    ITgCommand command = (ITgCommand)_serviceProvider.GetRequiredService(commandType);

                    await command.InvokeAsync(token);
                }
                else
                {
                    throw new TelegramException();
                }
            }
            else
            {
                throw new TelegramException();
            }
        }

        private bool IsCommand(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (_updateManager.Update.Type == UpdateType.Message)
            {
                token.ThrowIfCancellationRequested();

                Message message = _updateManager.Message;

                if (string.IsNullOrEmpty(message.Text))
                {
                    throw new TelegramException();
                }

                return message.Text.StartsWith('/');
            }
            else if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                token.ThrowIfCancellationRequested();

                CallbackQuery query = _updateManager.CallbackQuery;

                if (string.IsNullOrEmpty(query.Data))
                {
                    throw new TelegramException();
                }

                return query.Data.StartsWith('/');
            }
            else
            {
                return false;
            }
        }

        private string[] GetCommandsAndArguments(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            string[] rawCommand;
            if (_updateManager.Update.Type == UpdateType.Message)
            {
                token.ThrowIfCancellationRequested();

                Message message = _updateManager.Message;

                if (string.IsNullOrEmpty(message.Text))
                {
                    throw new TelegramException();
                }

                if (!message.Text.StartsWith('/'))
                {
                    throw new TelegramException();
                }

                rawCommand = message.Text[1..]
                  .ToUpper()
                  .Split('_');
            }
            else if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                token.ThrowIfCancellationRequested();

                CallbackQuery query = _updateManager.CallbackQuery;

                if (string.IsNullOrEmpty(query.Data))
                {
                    throw new TelegramException();
                }

                if (!query.Data.StartsWith('/'))
                {
                    throw new TelegramException();
                }

                rawCommand = query.Data[1..]
                  .ToUpper()
                  .Split('_');
            }
            else
            {
                throw new TelegramException();
            }

            if (rawCommand is null)
            {
                throw new TelegramException();
            }

            return rawCommand;
        }
    }
}