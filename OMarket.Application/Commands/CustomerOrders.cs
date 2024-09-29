using System.Globalization;
using System.Text;

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

using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Commands
{
    [TgCommand(TgCommands.CUSTOMERORDERS)]
    public class CustomerOrders : ITgCommand
    {
        private readonly ISendResponseService _response;
        private readonly IUpdateManager _updateManager;
        private readonly IDataProcessorService _dataProcessor;
        private readonly II18nService _i18n;
        private readonly IInlineMarkupService _inlineMarkup;
        private readonly IOrdersRepository _ordersRepository;

        public CustomerOrders(
                ISendResponseService response,
                IUpdateManager updateManager,
                IDataProcessorService dataProcessor,
                II18nService i18n,
                IInlineMarkupService inlineMarkup,
                IOrdersRepository ordersRepository
            )
        {
            _response = response;
            _updateManager = updateManager;
            _dataProcessor = dataProcessor;
            _i18n = i18n;
            _inlineMarkup = inlineMarkup;
            _ordersRepository = ordersRepository;
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

            List<ViewOrderDto> orders = await _ordersRepository
                .GetLastCustomerOrdersAsync(request.Customer.Id, token);

            string text = orders.Count <= 0
                ? GetAnswerTextEmpty()
                : GetAnswerText(orders);

            InlineKeyboardMarkup buttons = orders.Count <= 0
                ? _inlineMarkup.ToMainMenuBack()
                : _inlineMarkup.CustomerOrders();

            await _response.RemoveLastMessage(token);
            await _response.SendMessageAnswer(text, token, buttons);
        }

        private string GetFormattedDate(DateTime date)
        {
            DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(
                dateTime: date,
                destinationTimeZone: TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));

            return localDateTime.ToString("dd MMM yyyy HH:mm:ss", new CultureInfo("uk-UA"));
        }

        private string GetAnswerTextEmpty() =>
            $"""
                {_i18n.T("main_menu_command_history_of_orders_button")}

                {_i18n.T("order_command_orders_empty")}
                """;

        private string GetAnswerText(List<ViewOrderDto> orders)
        {
            StringBuilder sb = new();

            sb.AppendLine(_i18n.T("main_menu_command_history_of_orders_button"));
            sb.AppendLine();
            foreach (var order in orders)
            {
                sb.AppendLine(new string('―', 16));
                sb.AppendLine($"{_i18n.T("order_command_order")} {order.Id.GetHashCode().ToString()[1..]}");
                sb.AppendLine();
                sb.AppendLine($"<b>{_i18n.T("order_command_date_order")}</b> <i>{GetFormattedDate(order.CreatedAt)}</i>");
                sb.AppendLine();
                sb.AppendLine($"<b>{_i18n.T("order_command_order_status")}</b> <i>{order.Status}</i>");
                sb.AppendLine();
                sb.AppendLine($"<b>{_i18n.T("order_command_delivery_method")}</b> <i>{order.DeliveryMethod}</i>");
                sb.AppendLine();
                sb.AppendLine($"<b>{_i18n.T("order_command_store_address")}</b>");
                sb.AppendLine($"<i>{order.Store.City} {order.Store.Address}</i>");
                sb.AppendLine();
                sb.AppendLine($"<b>{_i18n.T("order_command_items")}</b>");
                sb.AppendLine();
                foreach (var product in order.Products)
                {
                    decimal price = product.Price * product.Quantity;
                    sb.AppendLine($"📌 <b>{product.FullName}</b>");
                    sb.AppendLine($"💵 <b>{product.Price}</b> грн. * <b>{product.Quantity}</b> шт. = <b>{price}</b> грн.");
                    sb.AppendLine();
                }
                sb.AppendLine($"<i>{_i18n.T("order_command_quantity")}</i> <b>{order.TotalQuantity}</b> <i>{_i18n.T("order_command_for_amount")}</i> <b>{order.TotalAmount}</b> <i>грн.</i>");
                sb.AppendLine(new string('―', 16));
                sb.AppendLine();
            }
            sb.AppendLine($"<b>{_i18n.T("order_command_your_last_orders")}</b>");

            return sb.ToString();
        }
    }
}