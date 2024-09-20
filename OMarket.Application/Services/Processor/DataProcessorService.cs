using AutoMapper;

using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Utilities;

namespace OMarket.Application.Services.Processor
{
    public class DataProcessorService : IDataProcessorService
    {
        private readonly ICustomersRepository _repository;

        private readonly IUpdateManager _updateManager;

        private readonly IMapper _mapper;

        public DataProcessorService(
                ICustomersRepository repository,
                IUpdateManager updateManager,
                IMapper mapper
            )
        {
            _repository = repository;
            _updateManager = updateManager;
            _mapper = mapper;
        }

        public async Task<RequestInfo> MapRequestData(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            string query = StringHelper.GetQueryFromCommand(_updateManager.Update);

            CustomerDto customerFromUpdate = _mapper.Map<CustomerDto>(_updateManager.Update);

            CustomerDto customerFromDB = await _repository
                .GetCustomerFromIdAsync(customerFromUpdate.Id, token);

            return new RequestInfo(
                Update: _updateManager.Update,
                Message: _updateManager.Message,
                Query: query,
                CustomerFromUpdate: customerFromUpdate,
                Customer: customerFromDB,
                UpdateType: _updateManager.Update.Type,
                LanguageCode: LanguageCode.UK);
        }

        public async Task<RequestInfo> MapRequestData()
        {
            string query = StringHelper.GetQueryFromCommand(_updateManager.Update);

            CustomerDto customerFromUpdate = _mapper.Map<CustomerDto>(_updateManager.Update);

            CustomerDto customerFromDB = await _repository
                .GetCustomerFromIdAsync(customerFromUpdate.Id);

            return new RequestInfo(
                Update: _updateManager.Update,
                Message: _updateManager.Message,
                Query: query,
                CustomerFromUpdate: customerFromUpdate,
                Customer: customerFromDB,
                UpdateType: _updateManager.Update.Type,
                LanguageCode: LanguageCode.UK);
        }
    }
}