using System.Text;

using Microsoft.Extensions.Caching.Distributed;

using OMarket.Domain.Attributes.TgCommand;
using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.Processor;
using OMarket.Domain.Interfaces.Application.Services.SendResponse;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Domain.Interfaces.Domain.TgCommand;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.ENDSEARCH)]
    public class EndSearch : ITgCommand
    {
        private readonly IUpdateManager _updateManager;
        private readonly ISendResponseService _response;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly IDistributedCache _distributedCache;
        private readonly IProductsRepository _productsRepository;

        public EndSearch(
                IUpdateManager updateManager,
                ISendResponseService response,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                IDistributedCache distributedCache,
                IProductsRepository productsRepository
            )
        {
            _updateManager = updateManager;
            _response = response;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
            _distributedCache = distributedCache;
            _productsRepository = productsRepository;
        }

        public async Task InvokeAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            RequestInfo request = await _dataProcessor.MapRequestData(token);

            if (request.Customer.StoreId == null)
            {
                await _response.SendMessageAnswer(
                    text: _i18n.T("main_menu_command_select_your_address"),
                    token: token,
                    buttons: _inlineMarkup.SelectStoreAddress("updatestoreaddress"));

                return;
            }

            if (_updateManager.Update.Type == UpdateType.CallbackQuery)
            {
                await _response.SendCallbackAnswer(token);
            }

            string cacheKey = $"{CacheKeys.CustomerFreeInputId}{request.Customer.Id}";

            string? typeString = await _distributedCache.GetStringAsync(cacheKey, token);

            if (string.IsNullOrEmpty(typeString))
            {
                throw new TelegramException();
            }

            string[] tempLines = typeString.Split('_', 2);

            if (tempLines.Length != 2)
            {
                throw new TelegramException();
            }

            if (tempLines[0] != "/65536")
            {
                await _distributedCache.RemoveAsync(cacheKey, token);

                throw new TelegramException();
            }

            string[] queryLines = tempLines[1].Split('=');

            if (queryLines.Length != 2)
            {
                throw new TelegramException();
            }

            if (!int.TryParse(queryLines[1], out int messageId))
            {
                throw new TelegramException();
            }

            if (!Guid.TryParse(queryLines[0], out Guid typeId))
            {
                throw new TelegramException();
            }

            string name = _updateManager.Update.Message?.Text
                ?? throw new TelegramException();

            List<ProductDto> products = await _productsRepository
                .GetProductsByNameAsync(name, typeId, (Guid)request.Customer.StoreId, token);

            if (products.Count <= 0)
            {
                string text = $"""
                    {_i18n.T("main_menu_command_product_search_by_name")}

                    {_i18n.T("end_search_command_search_yielded_no_results")}

                    {_i18n.T("end_search_command_try_again")}
                    """;

                await _response.RemoveMessageById(messageId, token);
                await _response.RemoveLastMessage(token);
                Message message = await _response.SendMessageAnswer(text, token, _inlineMarkup.ToMainMenuBack());

                await _distributedCache.SetStringAsync(cacheKey, $"/65536_{queryLines[0]}={message.MessageId}", token);

                return;
            }

            await _distributedCache.RemoveAsync(cacheKey, token);

            int index = 0;
            StringBuilder sb = new();

            sb.AppendLine(_i18n.T("main_menu_command_product_search_by_name"));
            sb.AppendLine();
            foreach (var item in products)
            {
                if (!item.Status)
                {
                    continue;
                }

                index++;

                sb.AppendLine($"<b>№{index} {item.Name}</b><i>, {item.Dimensions}</i>");
                sb.AppendLine();
            }
            sb.AppendLine($"<i>{_i18n.T("end_search_command_click_product_number_below")}</i>");

            InlineKeyboardMarkup buttons = _inlineMarkup.EndSearchProducts(products);

            await _response.RemoveMessageById(messageId, token);
            await _response.RemoveLastMessage(token);
            await _response.SendMessageAnswer(sb.ToString(), token, buttons);
        }
    }
}