using Microsoft.Extensions.Hosting;

using OMarket.Domain.Interfaces.Application.Services.StaticCollections;

namespace OMarket.Application.Services.StaticCollections
{
    public class StaticCollectionsInit : IHostedService
    {
        private readonly IStaticCollectionsService _staticCollections;
        private readonly IHostApplicationLifetime _appLifetime;

        public StaticCollectionsInit(IStaticCollectionsService staticCollections, IHostApplicationLifetime appLifetime)
        {
            _staticCollections = staticCollections ?? throw new ArgumentNullException(nameof(staticCollections));
            _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(() =>
            {
                _staticCollections.Initialize();
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}