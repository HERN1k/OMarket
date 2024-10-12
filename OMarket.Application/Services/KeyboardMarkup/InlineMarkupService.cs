using AutoMapper;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

using OMarket.Domain.DTOs;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Cache;
using OMarket.Domain.Interfaces.Application.Services.KeyboardMarkup;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
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

        private readonly ICacheService _cache;

        private readonly IMemoryCache _memoryCache;

        private readonly IUpdateManager _updateManager;

        private readonly IStaticCollectionsService _staticCollections;

        private readonly IMapper _mapper;

        private MemoryCacheEntryOptions _memoryCacheOptions { get; init; } =
            new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60.0D) };

        public InlineMarkupService(
                II18nService i18n,
                ICacheService cache,
                IMemoryCache memoryCache,
                IUpdateManager updateManager,
                IStaticCollectionsService staticCollections,
                IMapper mapper
            )
        {
            _i18n = i18n;
            _cache = cache;
            _memoryCache = memoryCache;
            _updateManager = updateManager;
            _staticCollections = staticCollections;
            _mapper = mapper;
        }

        public InlineKeyboardMarkup ToMainMenuBack(LanguageCode? code = null) =>
            new(InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_to_main_menu", code), "/mainmenu_back"));

        public InlineKeyboardMarkup ToMainMenuBackDel(LanguageCode? code = null) =>
            new(InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_to_main_menu", code), "/mainmenu_back_del"));

        public InlineKeyboardMarkup ShowPhoneButton(LanguageCode? code = null) =>
            new(InlineKeyboardButton.WithCallbackData(
                _i18n.T("start_command_show_phone_button", code), "/1000000700"));

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

            List<CartItemDto>? cart = await _cache.GetCacheAsync<List<CartItemDto>>($"{CacheKeys.CustomerCartId}{customer.Id}");

            if (cart is null)
            {
                buttons.Add([
                    InlineKeyboardButton.WithCallbackData(_i18n.T("main_menu_command_make_order_button", code), "/64")]);
            }
            else
            {
                buttons.Add([
                    InlineKeyboardButton.WithCallbackData(_i18n.T("main_menu_command_cart_button", code), "/4096")]);
            }

            buttons.AddRange(
            [
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_product_catalog_button", code), "/64"),
                ],
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_product_search_by_name", code), "/16384"),
                ],
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_history_of_orders_button", code), "/1000000600"),
                ],
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_find_store_button", code), "/524288"),
                ],
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_customer_reviews_button", code), "/268435456"),
                ],
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_consultation_button", code), "/1000000500"),

                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_leave_review_button", code), "/134217728"),
                ],
                [
                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_profile_button", code), "/8388608"),

                    InlineKeyboardButton
                        .WithCallbackData(_i18n.T("main_menu_command_contacts_button", code), "/2097152"),
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

            if (_memoryCache.TryGetValue($"{CacheKeys.KeyboardMarkupMenuProductUnderTypes}{query}", out (InlineKeyboardMarkup Markup, string CategoryType)? result))
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
                        InlineKeyboardButton.WithCallbackData(item,  $"/512_{guid}_1")]);
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

            _memoryCache.Set($"{CacheKeys.KeyboardMarkupMenuProductUnderTypes}{query}", result, _memoryCacheOptions);

            return ((InlineKeyboardMarkup Markup, string CategoryType))result;
        }

        public InlineKeyboardMarkup ProductView(ProductWithDbInfoDto dto, int quantity, LanguageCode? code = null)
        {
            if (dto.PageNumber <= 0)
            {
                throw new TelegramException();
            }

            if (dto.Product == null)
            {
                throw new TelegramException();
            }

            List<InlineKeyboardButton[]> buttons = new();

            if (quantity <= 0)
            {
                if (dto.Product.Status)
                {
                    buttons.Add([
                        dto.PageNumber == 1
                            ? InlineKeyboardButton.WithCallbackData(
                                _i18n.T("product_view_button_disable", code),
                                _i18n.T("product_view_button_is_disable_button", code))
                            : InlineKeyboardButton.WithCallbackData(
                                _i18n.T("product_view_previous_button", code),
                                $"/512_{dto.Product.UnderTypeId}_{dto.PageNumber - 1}"),

                        InlineKeyboardButton.WithCallbackData(
                            _i18n.T("product_view_quantity_selection_button", code),
                            $"/1024_{dto.Product.UnderTypeId}_{dto.PageNumber}_1"),

                        dto.PageNumber == dto.MaxNumber
                            ? InlineKeyboardButton.WithCallbackData(
                                _i18n.T("product_view_button_disable", code),
                                _i18n.T("product_view_button_is_disable_button", code))
                            : InlineKeyboardButton.WithCallbackData(
                                _i18n.T("product_view_next_button", code),
                                $"/512_{dto.Product.UnderTypeId}_{dto.PageNumber + 1}")]);
                }
                else
                {
                    buttons.Add([
                        dto.PageNumber == 1
                            ? InlineKeyboardButton.WithCallbackData(
                                _i18n.T("product_view_button_disable", code),
                                _i18n.T("product_view_button_is_disable_button", code))
                            : InlineKeyboardButton.WithCallbackData(
                                _i18n.T("product_view_previous_button", code),
                                $"/512_{dto.Product.UnderTypeId}_{dto.PageNumber - 1}"),

                        InlineKeyboardButton.WithCallbackData(
                            _i18n.T("product_view_button_disable", code),
                            _i18n.T("product_view_button_is_disable_button", code)),

                        dto.PageNumber == dto.MaxNumber
                            ? InlineKeyboardButton.WithCallbackData(
                                _i18n.T("product_view_button_disable", code),
                                _i18n.T("product_view_button_is_disable_button", code))
                            : InlineKeyboardButton.WithCallbackData(
                                _i18n.T("product_view_next_button", code),
                                $"/512_{dto.Product.UnderTypeId}_{dto.PageNumber + 1}")]);
                }

                buttons.Add([
                    InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_back", code), $"/128_{dto.TypeId}_back_del"),
                    InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_to_main_menu", code), "/mainmenu_back_del")]);
            }
            else
            {
                string quantityString = $"{_i18n.T("product_view_quantity_button", code)} {quantity}";

                buttons.Add([
                    InlineKeyboardButton.WithCallbackData(
                        _i18n.T("product_view_minus_button", code),
                        $"/1024_{dto.Product.UnderTypeId}_{dto.PageNumber}_{quantity - 1}"),

                    InlineKeyboardButton.WithCallbackData(quantityString, quantityString),

                    InlineKeyboardButton.WithCallbackData(
                        _i18n.T("product_view_plus_button", code),
                        $"/1024_{dto.Product.UnderTypeId}_{dto.PageNumber}_{quantity + 1}")]);

                buttons.Add([
                    InlineKeyboardButton.WithCallbackData(
                        _i18n.T("product_view_accept_button", code),
                        $"/2048_{dto.Product.Id}_{quantity}_{dto.PageNumber}")]);

                string price = $"💵 {dto.Product.Price} * {quantity} шт. = {quantity * dto.Product.Price} грн.";

                buttons.Add([
                    InlineKeyboardButton.WithCallbackData(price, price)]);

                buttons.Add([InlineKeyboardButton.WithCallbackData(
                    _i18n.T("menu_item_back", code),
                    $"/1024_{dto.Product.UnderTypeId}_{dto.PageNumber}_0")]);
            }

            return new(buttons);
        }

        public InlineKeyboardMarkup Cart(LanguageCode? code = null)
        {
            List<InlineKeyboardButton[]> buttons = new();

            buttons.Add([InlineKeyboardButton.WithCallbackData(
                _i18n.T("cart_command_confirm_the_order"),
                "/536870912")]);

            buttons.Add([InlineKeyboardButton.WithCallbackData(
                _i18n.T("cart_command_edit_cart_button"),
                "/8192")]);

            buttons.Add([InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_back", code),
                $"/mainmenu_back")]);

            return new(buttons);
        }

        public InlineKeyboardMarkup EditCart(List<CartItemDto> cart, LanguageCode? code = null)
        {
            List<InlineKeyboardButton[]> buttons = new();

            int index = 0;
            foreach (var item in cart)
            {
                index++;

                buttons.Add([
                    InlineKeyboardButton.WithCallbackData($"№ {index}", $"№ {index}"),
                    InlineKeyboardButton.WithCallbackData(
                        _i18n.T("product_view_minus_button", code),
                        $"/8192_{item.Id}_{item.Quantity - 1}"),
                    InlineKeyboardButton.WithCallbackData($"{item.Quantity} шт.", $"{item.Quantity} шт."),
                    InlineKeyboardButton.WithCallbackData(
                        _i18n.T("product_view_plus_button",code),
                        $"/8192_{item.Id}_{item.Quantity + 1}"),]);
            }

            buttons.Add([
                    InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_back", code), "/4096")]);

            return new(buttons);
        }

        public InlineKeyboardMarkup CartIsEmpty(LanguageCode? code = null)
        {
            List<InlineKeyboardButton[]> buttons = new();

            buttons.Add([ InlineKeyboardButton
                    .WithCallbackData(_i18n.T("main_menu_command_product_catalog_button", code), "/menuproducttypes")]);

            buttons.Add([ InlineKeyboardButton
                .WithCallbackData(_i18n.T("menu_item_to_main_menu", code), "/mainmenu_back")]);

            return new(buttons);
        }

        public InlineKeyboardMarkup SelectProductTypeForCustomerSearchChoice(LanguageCode? code = null)
        {
            if (_memoryCache.TryGetValue(CacheKeys.KeyboardMarkupSelectProductTypeForCustomerSearchChoice, out InlineKeyboardMarkup? result))
            {
                return result ?? throw new TelegramException();
            }

            List<InlineKeyboardButton> items = new();

            foreach (var item in _staticCollections.GuidToStringProductsTypesDictionary)
            {
                items.Add(InlineKeyboardButton.WithCallbackData(item.Value, $"/32768_{item.Key}"));
            }

            List<InlineKeyboardButton[]> buttons = new(items
                .OrderBy(button => button.Text.Length)
                .Select(button => new[] { button }));

            buttons.Add([
                InlineKeyboardButton.WithCallbackData(_i18n.T("menu_item_back", code), "/mainmenu_back")]);

            result = new(buttons);

            _memoryCache.Set(CacheKeys.KeyboardMarkupSelectProductTypeForCustomerSearchChoice, result, _memoryCacheOptions);

            return result;
        }

        public InlineKeyboardMarkup EndSearchProducts(List<ProductDto> products, LanguageCode? code = null)
        {
            if (products.Count <= 0)
            {
                return Empty;
            }

            List<InlineKeyboardButton[]> buttons = new();

            int index = 0;
            foreach (var item in products)
            {
                if (!item.Status)
                {
                    continue;
                }

                index++;

                buttons.Add([ InlineKeyboardButton
                    .WithCallbackData($"№{index} {item.Name}, {item.Dimensions}", $"/131072_{item.Id}_1")]);
            }

            buttons.Add([ InlineKeyboardButton
                .WithCallbackData(_i18n.T("menu_item_to_main_menu", code), "/mainmenu_back")]);

            return new(buttons);
        }

        public InlineKeyboardMarkup ProductViewBySearch(ProductDto product, int quantity, LanguageCode? code = null)
        {
            if (product == null)
            {
                throw new TelegramException();
            }

            if (quantity <= 0)
            {
                throw new TelegramException();
            }

            List<InlineKeyboardButton[]> buttons = new();

            string quantityString = $"{_i18n.T("product_view_quantity_button", code)} {quantity}";

            buttons.Add([
                quantity > 1
                ? InlineKeyboardButton.WithCallbackData(
                    _i18n.T("product_view_minus_button", code),
                    $"/131072_{product.Id}_{quantity - 1}")
                : InlineKeyboardButton.WithCallbackData(
                    _i18n.T("product_view_button_disable", code),
                    _i18n.T("product_view_button_is_disable_button", code)),

                InlineKeyboardButton.WithCallbackData(quantityString, quantityString),

                InlineKeyboardButton.WithCallbackData(
                    _i18n.T("product_view_plus_button", code),
                    $"/131072_{product.Id}_{quantity + 1}")]);

            buttons.Add([
                InlineKeyboardButton.WithCallbackData(
                    _i18n.T("product_view_accept_button", code),
                    $"/262144_{product.Id}_{quantity}")]);

            string price = $"💵 {product.Price} * {quantity} шт. = {quantity * product.Price} грн.";

            buttons.Add([
                InlineKeyboardButton.WithCallbackData(price, price)]);

            buttons.Add([ InlineKeyboardButton
                .WithCallbackData(_i18n.T("menu_item_to_main_menu", code), "/mainmenu_back_del")]);

            return new(buttons);
        }

        public InlineKeyboardMarkup SelectStoreAddressWithLocation(LanguageCode? code = null)
        {
            if (_memoryCache.TryGetValue(CacheKeys.SelectStoreAddressWithLocationId, out InlineKeyboardMarkup? result))
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
                        callbackData: $"/1048576_{item.Value.Latitude}_{item.Value.Longitude}"),
                });
            }

            tempButtons.Add([InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_back", code),
                "/mainmenu_back")]);

            result = new(tempButtons);

            _memoryCache.Set(CacheKeys.SelectStoreAddressWithLocationId, result, _memoryCacheOptions);

            return result;
        }

        public InlineKeyboardMarkup SelectStoreAddressWithContacts(LanguageCode? code = null)
        {
            if (_memoryCache.TryGetValue(CacheKeys.SelectStoreAddressWithContactsId, out InlineKeyboardMarkup? result))
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
                        callbackData: $"/4194304_{item.Value.StoreId}"),
                });
            }

            tempButtons.Add([InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_back", code),
                "/mainmenu_back")]);

            result = new(tempButtons);

            _memoryCache.Set(CacheKeys.SelectStoreAddressWithContactsId, result, _memoryCacheOptions);

            return result;
        }

        public InlineKeyboardMarkup Profile(LanguageCode? code = null)
        {
            if (_memoryCache.TryGetValue(CacheKeys.KeyboardMarkupProfileId, out InlineKeyboardMarkup? result))
            {
                return result ?? throw new TelegramException();
            }

            List<InlineKeyboardButton[]> buttons = new();

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("profile_command_update_phone_number_button", code),
                "/33554432")]);

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("profile_command_update_selected_store_button", code),
                "/16777216")]);

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_to_main_menu", code),
                "/mainmenu_back")]);

            result = new(buttons);

            _memoryCache.Set(CacheKeys.KeyboardMarkupProfileId, result, _memoryCacheOptions);

            return result;
        }

        public InlineKeyboardMarkup SelectStoreAddressUpdate(LanguageCode? code = null)
        {
            if (_memoryCache.TryGetValue(CacheKeys.SelectStoreAddressUpdateId, out InlineKeyboardMarkup? result))
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
                        callbackData: $"/16777216_{item.Value.StoreId}"),
                });
            }

            tempButtons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_back", code),
                "/8388608")]);

            result = new(tempButtons);

            _memoryCache.Set(CacheKeys.SelectStoreAddressUpdateId, result, _memoryCacheOptions);

            return result;
        }

        public InlineKeyboardMarkup SelectStoreAddressForAddReview(LanguageCode? code = null)
        {
            if (_memoryCache.TryGetValue(CacheKeys.SelectStoreAddressForAddReviewId, out InlineKeyboardMarkup? result))
            {
                return result ?? throw new TelegramException();
            }

            List<InlineKeyboardButton[]> buttons = new();

            foreach (var item in _staticCollections.CitiesWithStoreAddressesDictionary)
            {
                buttons.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: $"{item.Value.City} {item.Value.Address}",
                        callbackData: $"/67108864_{item.Value.StoreId}"),
                });
            }

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_to_main_menu", code),
                "/mainmenu_back")]);

            result = new(buttons);

            _memoryCache.Set(CacheKeys.SelectStoreAddressForAddReviewId, result, _memoryCacheOptions);

            return result;
        }

        public InlineKeyboardMarkup SelectStoreAddressForViewReview(LanguageCode? code = null)
        {
            if (_memoryCache.TryGetValue(CacheKeys.SelectStoreAddressForViewReviewId, out InlineKeyboardMarkup? result))
            {
                return result ?? throw new TelegramException();
            }

            List<InlineKeyboardButton[]> buttons = new();

            foreach (var item in _staticCollections.CitiesWithStoreAddressesDictionary)
            {
                buttons.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: $"{item.Value.City} {item.Value.Address}",
                        callbackData: $"/268435456_{item.Value.StoreId}_1"),
                });
            }

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_to_main_menu", code),
                "/mainmenu_back")]);

            result = new(buttons);

            _memoryCache.Set(CacheKeys.SelectStoreAddressForViewReviewId, result, _memoryCacheOptions);

            return result;
        }

        public InlineKeyboardMarkup ReviewView(ReviewWithDbInfoDto dto, LanguageCode? code = null)
        {
            if (dto.PageNumber <= 0)
            {
                throw new TelegramException();
            }

            if (dto.Review == null)
            {
                throw new TelegramException();
            }

            List<InlineKeyboardButton[]> buttons = new();

            buttons.Add([
                dto.PageNumber == 1
                    ? InlineKeyboardButton.WithCallbackData(
                        _i18n.T("product_view_button_disable", code),
                        _i18n.T("product_view_button_is_disable_button", code))
                    : InlineKeyboardButton.WithCallbackData(
                        _i18n.T("product_view_previous_button", code),
                        $"/268435456_{dto.Review.StoreId}_{dto.PageNumber - 1}"),

                dto.PageNumber == dto.MaxNumber
                    ? InlineKeyboardButton.WithCallbackData(
                        _i18n.T("product_view_button_disable", code),
                        _i18n.T("product_view_button_is_disable_button", code))
                    : InlineKeyboardButton.WithCallbackData(
                        _i18n.T("product_view_next_button", code),
                        $"/268435456_{dto.Review.StoreId}_{dto.PageNumber + 1}")]);

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_to_main_menu", code),
                "/mainmenu_back")]);

            return new(buttons);
        }

        public InlineKeyboardMarkup NoReviewsHaveBeenView(LanguageCode? code = null)
        {
            if (_memoryCache.TryGetValue(CacheKeys.NoReviewsHaveBeenViewId, out InlineKeyboardMarkup? result))
            {
                return result ?? throw new TelegramException();
            }

            List<InlineKeyboardButton[]> buttons = new();

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("add_review_command_add_first_review_button", code),
                $"/134217728")]);

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_to_main_menu", code),
                "/mainmenu_back")]);

            result = new(buttons);

            _memoryCache.Set(CacheKeys.NoReviewsHaveBeenViewId, result, _memoryCacheOptions);

            return result;
        }

        public InlineKeyboardMarkup CreateOrder(LanguageCode? code = null)
        {
            List<InlineKeyboardButton[]> buttons = new();

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("order_command_delivery_delivery_button", code),
                $"/1000000000_delivery")]);

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("order_command_delivery_self_pickup_button", code),
                $"/1000000000_selfpickup")]);

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_back", code),
                "/4096")]);

            return new(buttons);
        }

        public InlineKeyboardMarkup MarkupOrderForStoreChat(string status, Guid orderId, LanguageCode? code = null)
        {
            List<InlineKeyboardButton[]> buttons = new();

            string statusString = $"{_i18n.T("admins_command_status_button", code)} {status}";

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                statusString, statusString)]);

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("admins_command_change_order_status_button", code),
                $"/1000000400_{orderId}")]);

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                new string('―', 12), new string('―', 12))]);

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("admins_command_delete_message_button", code),
                "/1000000300")]);

            return new(buttons);
        }

        public InlineKeyboardMarkup ChangeOrderStatus(string orderId, LanguageCode? code = null)
        {
            List<InlineKeyboardButton[]> buttons = new();

            foreach (var item in _staticCollections.OrderStatusesDictionary)
            {
                buttons.Add([ InlineKeyboardButton.WithCallbackData(
                    item.Value,
                    $"/1000000400_{orderId}_{item.Key}")]);
            }

            return new(buttons);
        }

        public InlineKeyboardMarkup SelectStoreAddressForConsultation(LanguageCode? code = null)
        {
            if (_memoryCache.TryGetValue(CacheKeys.SelectStoreAddressForConsultationId, out InlineKeyboardMarkup? result))
            {
                return result ?? throw new TelegramException();
            }

            List<InlineKeyboardButton[]> buttons = new();

            foreach (var item in _staticCollections.CitiesWithStoreAddressesDictionary)
            {
                buttons.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: $"{item.Value.City} {item.Value.Address}",
                        callbackData: $"/1000000500_{item.Value.StoreId}"),
                });
            }

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_back", code),
                "/mainmenu_back")]);

            result = new(buttons);

            _memoryCache.Set(CacheKeys.SelectStoreAddressForConsultationId, result, _memoryCacheOptions);

            return result;
        }

        public InlineKeyboardMarkup RemoveThisMessage(LanguageCode? code = null)
        {
            if (_memoryCache.TryGetValue(CacheKeys.RemoveThisMessageId, out InlineKeyboardMarkup? result))
            {
                return result ?? throw new TelegramException();
            }

            List<InlineKeyboardButton[]> buttons = new();

            buttons.Add([ InlineKeyboardButton.WithCallbackData(
                _i18n.T("admins_command_delete_message_button", code),
                "/1000000300")]);

            result = new(buttons);

            _memoryCache.Set(CacheKeys.RemoveThisMessageId, result, _memoryCacheOptions);

            return result;
        }

        public InlineKeyboardMarkup CustomerOrders(LanguageCode? code = null)
        {
            if (_memoryCache.TryGetValue(CacheKeys.CustomerOrdersId, out InlineKeyboardMarkup? result))
            {
                return result ?? throw new TelegramException();
            }

            List<InlineKeyboardButton[]> buttons = new();

            buttons.Add([InlineKeyboardButton.WithCallbackData(
                _i18n.T("order_command_update", code),
                "/1000000600")]);

            buttons.Add([InlineKeyboardButton.WithCallbackData(
                _i18n.T("menu_item_back", code),
                "/mainmenu_back")]);

            result = new(buttons);

            _memoryCache.Set(CacheKeys.CustomerOrdersId, result, _memoryCacheOptions);

            return result;
        }
    }
}