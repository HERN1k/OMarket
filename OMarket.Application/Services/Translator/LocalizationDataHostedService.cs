using Microsoft.Extensions.Hosting;

using OMarket.Domain.Interfaces.Application.Services.Translator;

namespace OMarket.Application.Services.Translator
{
    public class LocalizationDataHostedService : IHostedService
    {
        private readonly ILocalizationData _localizationData;

        public LocalizationDataHostedService(ILocalizationData localizationData)
        {
            _localizationData = localizationData;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _localizationData.MapLocalizationData(cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}