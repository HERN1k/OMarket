using System.Collections.Frozen;
using OMarket.Domain.Enums;

namespace OMarket.Domain.Interfaces.Application.Services.StaticCollections
{
    public interface IStaticCollectionsService
    {
        FrozenDictionary<TgCommands, Type> CommandsMap { get; init; }
    }
}