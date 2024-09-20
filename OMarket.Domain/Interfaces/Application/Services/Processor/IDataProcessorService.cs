using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Application.Services.Processor
{
    public interface IDataProcessorService
    {
        Task<RequestInfo> MapRequestData(CancellationToken token);

        Task<RequestInfo> MapRequestData();
    }
}