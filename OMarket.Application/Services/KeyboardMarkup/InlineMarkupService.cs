using AutoMapper;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Domain.Interfaces.Application.Services.TgUpdate;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Helpers.Extensions;
using OMarket.Helpers.Utilities;

using Telegram.Bot.Types.ReplyMarkups;

namespace OMarket.Application.Services.KeyboardMarkup
{
    public class InlineMarkupService : IInlineMarkupService
    {
        public InlineKeyboardMarkup Empty { get => new(Array.Empty<InlineKeyboardButton>()); }

        private readonly II18nService _i18n;

        private readonly IDistributedCache _distributedCache;

        private readonly IMemoryCache _memoryCache;

        private readonly IUpdateManager _updateManager;

        private readonly IStaticCollectionsService _staticCollections;

        private readonly IMapper _mapper;

        private MemoryCacheEntryOptions _memoryCacheOptions { get; init; } =
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2.0D) };

        public InlineMarkupService(
                II18nService i18n,
                IDistributedCache distributedCache,
                IMemoryCache memoryCache,
                IUpdateManager updateManager,
                IStaticCollectionsService staticCollections,
                IMapper mapper
            )
        {
            _i18n = i18n;
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
            _updateManager = updateManager;
            _staticCollections = staticCollections;
            _mapper = mapper;
        }

        public InlineKeyboardMarkup SelectStoreAddress(string command, LanguageCode? code = null)
        {
            if (string.IsNullOrEmpty(command) || string.IsNullOrWhiteSpace(command))
            {
                throw new TelegramException();
            }

            if (_memoryCache.TryGetValue($"{CacheKeys.KeyboardMarkupSelectStoreAddress}{command}", out InlineKeyboardMarkup? result))
            {
                return result ?? throw new TelegramException();
            }

            List<InlineKeyboardButton[]> tempButtons = new();

            foreach (var item in _staticCollections.CitiesWithStoreAddressesDictionary)
            {
                tempButtons.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: $"{item.Value.City} {item.Value.Address}",
                        callbackData: $"/{command}_{item.Key}"),
                });
            }

            result = new(tempButtons);

            _memoryCache.Set($"{CacheKeys.KeyboardMarkupSelectStoreAddress}{command}", result, _memoryCacheOptions);

            return result;
        }

        public async Task<InlineKeyboardMarkup> MainMenu(CancellationToken token, LanguageCode? code = null)
        {
            token.ThrowIfCancellationRequested();

            CustomerDto customer = _mapper.Map<CustomerDto>(_updateManager.Update);

            List<InlineKeyboardButton[]> buttons = new();

            if (string.IsNullOrEmpty(await _distributedCache.GetStringAsync($"{CacheKeys.CustomerCartId}{customer.Id}", token)))
            {
                buttons.Add([
                    InlineKeyboardButton.WithCallbackData(_i18n.T("main_menu_command_make_order_button", code), "/menuproducttypes")]);
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
                        .WithCallbackData(_i18n.T("main_menu_command_product_catalog_button", code), "/menuproducttypes"),
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

        public InlineKeyboardMarkup MenuProductTypes(LanguageCode? code = null)
        {
            if (_memoryCache.TryGetValue(CacheKeys.KeyboardMarkupMenuProductTypes, out InlineKeyboardMarkup? result))
            {
                return result ?? throw new TelegramException();
            }

            List<InlineKeyboardButton> items = new();

            foreach (var item in _staticCollections.GuidToStringProductsTypesDictionary)
            {
                items.Add(InlineKeyboardButton.WithCallbackData(item.Value, $"/128_{item.Key}"));
            }

            List<InlineKeyboardButton[]> buttons = new(items
                .OrderBy(button => button.Text.Length)
                .Select(button => new[] { button }));

            buttons.Add([
                InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_back", code), "/mainmenu_back"),
                InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_to_main_menu", code), "/mainmenu_back")]);

            result = new(buttons);

            _memoryCache.Set(CacheKeys.KeyboardMarkupMenuProductTypes, result, _memoryCacheOptions);

            return result;
        }

        public (InlineKeyboardMarkup Markup, string CategoryType) MenuProductUnderTypes(string query, LanguageCode? code = null)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrWhiteSpace(query))
            {
                throw new TelegramException();
            }

            if (_memoryCache.TryGetValue($"{CacheKeys.KeyboardMarkupMenuProductUnderTypes}{query.ConvertToBase64()}", out (InlineKeyboardMarkup Markup, string CategoryType)? result))
            {
                return result ?? throw new TelegramException();
            }

            if (!_staticCollections.GuidToStringProductsTypesDictionary.TryGetValue(query, out var type))
            {
                throw new TelegramException();
            }

            if (!_staticCollections.AllProductsTypesDictionary.TryGetValue(type, out var underTypes))
            {
                throw new TelegramException();
            }

            List<InlineKeyboardButton[]> buttons = new();

            if (underTypes.Count > 0)
            {
                foreach (var item in underTypes)
                {
                    if (!_staticCollections.StringToGuidUnderTypesDictionary.TryGetValue(item, out var guid))
                    {
                        throw new TelegramException();
                    }

                    buttons.Add([
                        InlineKeyboardButton.WithCallbackData(item,  $"/512_{guid}")]);
                }

                buttons.Add([
                    InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_back", code), "/menuproducttypes"),
                    InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_to_main_menu", code), "/mainmenu_back")]);
            }
            else
            {
                buttons.Add([InlineKeyboardButton
                    .WithCallbackData(_i18n.T("generic_menu_null_item", code), _i18n.T("generic_menu_null_item", code))]);

                buttons.Add([
                    InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_back", code), "/menuproducttypes"),
                    InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_to_main_menu", code), "/mainmenu_back")]);
            }

            result = (Markup: new(buttons), CategoryType: type);

            _memoryCache.Set($"{CacheKeys.KeyboardMarkupMenuProductUnderTypes}{query.ConvertToBase64()}", result, _memoryCacheOptions);

            return ((InlineKeyboardMarkup Markup, string CategoryType))result;
        }

        public InlineKeyboardMarkup ProductView(int quantity, LanguageCode? code = null)
        {
            List<InlineKeyboardButton[]> buttons = new();

            buttons.Add([
                InlineKeyboardButton.WithCallbackData(_i18n.T("product_view_previous_button", code), "/dev"),
                InlineKeyboardButton.WithCallbackData(_i18n.T("product_view_to_cart_button", code), "/dev"),
                InlineKeyboardButton.WithCallbackData(_i18n.T("product_view_next_button", code), "/dev")]);

            if (quantity > 0)
            {
                buttons.Add([
                    InlineKeyboardButton.WithCallbackData(_i18n.T("product_view_minus_button", code), "/dev"),
                    InlineKeyboardButton.WithCallbackData($"{_i18n.T("product_view_quantity_button", code)} {quantity}", "/dev"),
                    InlineKeyboardButton.WithCallbackData(_i18n.T("product_view_plus_button", code), "/dev")]);
            }

            buttons.Add([
                InlineKeyboardButton.WithCallbackData(_i18n.T("generic_product_view_cart_button", code), "/dev"),
                InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_to_main_menu", code), "/mainmenu_back")]);

            return new(buttons);
        }
    }
}