namespace OMarket.Domain.Interfaces.Application.Services.Distributor
{
    public interface IDistributorService
    {
        Task Distribute(CancellationToken token);
    }
}