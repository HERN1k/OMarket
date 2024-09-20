using System.Collections.Frozen;

namespace OMarket.Domain.Interfaces.Application.Services.Translator
{
    public interface ILocalizationData
    {
        FrozenDictionary<string, string> Uk { get; }
        void MapLocalizationData(CancellationToken cancellationToken);
    }
}