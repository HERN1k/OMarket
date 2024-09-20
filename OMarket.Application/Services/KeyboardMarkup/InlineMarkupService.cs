using AutoMapper;

using Microsoft.Extensions.Caching.Distributed;

using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Services.KeyboardMarkup
{
    public class InlineMarkupService : IInlineMarkupService
    {
        public InlineKeyboardMarkup Empty { get => new(Array.Empty<InlineKeyboardButton>()); }

        private readonly II18nService _i18n;

        private readonly IDistributedCache _cache;

        private readonly IUpdateManager _updateManager;

        private readonly IMapper _mapper;

        public InlineMarkupService(
                II18nService i18n,
                IDistributedCache cache,
                IUpdateManager updateManager,
                IMapper mapper
            )
        {
            _i18n = i18n;
            _cache = cache;
            _updateManager = updateManager;
            _mapper = mapper;
        }

        public InlineKeyboardMarkup SelectCity(List<CityDto> cities, LanguageCode? code = null)
        {
            List<InlineKeyboardButton[]> buttons = new();

            foreach (CityDto city in cities)
            {
                buttons.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(city.CityName, $"/savecity_{StringHelper.GetQueryFromCityName(city.CityName)}"),
                });
            }

            return new InlineKeyboardMarkup(buttons);
        }

        public async Task<InlineKeyboardMarkup> MainMenu(CancellationToken token, LanguageCode? code = null)
        {
            token.ThrowIfCancellationRequested();

            CustomerDto customer = _mapper.Map<CustomerDto>(_updateManager.Update);

            List<InlineKeyboardButton[]> buttons = new();

            if (string.IsNullOrEmpty(await _cache.GetStringAsync($"{CacheKeys.CustomerCartId}{customer.Id}", token)))
            {
                buttons.Add([
                    InlineKeyboardButton.WithCallbackData(_i18n.T("main_menu_command_make_order_button", code), "/dev")]);
            }
            else
            {
                buttons.Add([
                    InlineKeyboardButton.WithCallbackData(_i18n.T("main_menu_command_cart_button", code), "/dev")]);
            }

            buttons.AddRange(
            [
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_product_catalog_button", code), "/catalogmenu"),
                ],
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_product_search_by_name", code), "/dev"),
                ],
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_history_of_orders_button", code), "/dev"),
                ],
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_find_store_button", code), "/dev"),
                ],
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_customer_reviews_button", code), "/dev"),
                ],
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_consultation_button", code), "/dev"),

                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_leave_review_button", code), "/dev"),
                ],
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_profile_button", code), "/dev"),

                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_contacts_button", code), "/dev"),
                ],
            ]);

            return new InlineKeyboardMarkup(buttons);
        }

        public InlineKeyboardMarkup CatalogMenu(LanguageCode? code = null)
        {
            var buttons = new InlineKeyboardButton[][]
            {
                new[] { InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_beer", code), "/dev") },
                new[] { InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_cider", code), "/dev") },
                new[] { InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_wine", code), "/dev") },
                new[] { InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_strong_drinks", code), "/dev") },
                new[] { InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_snacks", code), "/dev") },
                new[] { InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_energy", code), "/dev") },
                new[] { InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_low_alcohol_drinks", code), "/dev") },
                new[] { InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_non_alcoholic_drinks", code), "/dev") },
                new[] { InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_mineral_and_drinking_water", code), "/dev") },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_back", code), "/mainmenu_back"),
                    InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_to_main_menu", code), "/mainmenu_back")
                }
            };

            return new InlineKeyboardMarkup(buttons);
        }
    }
}