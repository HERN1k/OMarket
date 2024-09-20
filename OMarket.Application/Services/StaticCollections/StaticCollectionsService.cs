using System.Collections.Frozen;
using System.Reflection;

using Microsoft.Extensions.Logging;

using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Domain.Interfaces.Domain.TgCommand;

namespace OMarket.Application.Services.StaticCollections
{
    public class StaticCollectionsService : IStaticCollectionsService
    {
        public FrozenDictionary<TgCommands, Type> CommandsMap { get; init; }

        private readonly ILogger<StaticCollectionsService> _logger;

        public StaticCollectionsService(ILogger<StaticCollectionsService> logger)
        {
            _logger = logger;

            _logger.LogInformation("Starting initialization of command types...");

            CommandsMap = MapCommands();

            _logger.LogInformation("Command types have been successfully initialized.");
        }

        private FrozenDictionary<TgCommands, Type> MapCommands()
        {
            IEnumerable<Type> commandTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetInterfaces().Contains(typeof(ITgCommand)))
                .Where(t => t.GetCustomAttribute<TgCommandAttribute>() != null);

            _logger.LogInformation("Found {Count} command types.", commandTypes.Count());

            Dictionary<TgCommands, Type> commandMap = new();

            foreach (Type type in commandTypes)
            {
                TgCommandAttribute? attribute = type.GetCustomAttribute<TgCommandAttribute>();

                if (attribute is not null)
                {
                    commandMap.TryAdd(attribute.Command, type);
                }
            }

            return commandMap.ToFrozenDictionary();
        }
    }
}