using AutoMapper;

using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.SAVECITY)]
    public class SaveCity : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly ICustomersRepository _customersRepository;
        private readonly II18nService _i18n;
        private readonly IMapper _mapper;
        private readonly IDataProcessorService _dataProcessor;
        private readonly IInlineMarkupService _inlineMarkup;

        public SaveCity(
                IUpdateManager updateManager,
                ISendResponseService response,
                ICustomersRepository customersRepository,
                II18nService i18n,
                IMapper mapper,
                IDataProcessorService dataProcessor,
                IInlineMarkupService inlineMarkup
            )
        {
            _updateManager = updateManager;
            _response = response;
            _customersRepository = customersRepository;
            _i18n = i18n;
            _mapper = mapper;
            _dataProcessor = dataProcessor;
            _inlineMarkup = inlineMarkup;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (_updateManager.Update.Type == UpdateType.CallbackQuery)
                {
                    await _response.SendCallbackAnswer(token);
                }

                RequestInfo request = await _dataProcessor.MapRequestData(token);

                if (request is null || string.IsNullOrEmpty(request.Query))
                {
                    await RemoveCustomerAndEditLastMessage(token);

                    return;
                }

                string city = StringHelper.GetCityNameFromQuery(request.Query);

                await _customersRepository.SaveCityAsync(
                    id: request.Customer.Id,
                    city: city,
                    token: token);

                string text = $"""
                    {_i18n.T("save_city_command_city_is_saved_1")}

                    {_i18n.T("save_city_command_city_is_saved_2")}
                    """;

                InlineKeyboardMarkup buttons = await _inlineMarkup
                    .MainMenu(token);

                await _response.EditLastMessage(text, token, buttons);
            }
            catch (Exception)
            {
                await RemoveCustomerAndEditLastMessage(token);

                return;
            }
        }

        private async Task RemoveCustomerAndEditLastMessage(CancellationToken token)
        {
            CustomerDto customer = _mapper.Map<CustomerDto>(_updateManager.Update);

            await _customersRepository.RemoveCustomerAsync(customer.Id);

            await _response.EditLastMessage($"""
                {_i18n.T("exception_main")}

                {_i18n.T("exception_main_later")}

                {_i18n.T("exception_main_we_are_very_sorry")}
                """, token, _inlineMarkup.Empty);
        }
    }
}