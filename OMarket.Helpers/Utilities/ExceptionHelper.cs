using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http;

using OMarket.Domain.DTOs;
using OMarket.Helpers.Extensions;

namespace OMarket.Helpers.Utilities
{
    public static class ExceptionHelper
    {
        private static readonly string[] _permittedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        private static readonly string[] _permittedContentTypes = { "image/jpeg", "image/png", "image/webp" };

        public static TokenClaims GetTokenClaims(this IEnumerable<Claim> claims)
        {
            string permission = claims.SingleOrDefault(claim => claim.Type == ClaimTypes.Role)
                ?.Value ?? throw new ArgumentException("Токен не знайдено.");

            string login = claims.SingleOrDefault(claim => claim.Type == ClaimTypes.Name)
                ?.Value ?? throw new ArgumentException("Токен не знайдено.");

            return new(permission, login);
        }

        public static RegisterRequestDto VerificationData(this RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Login))
            {
                throw new ArgumentNullException(nameof(request.Login), "Поле логін пустe.");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentNullException(nameof(request.Password), "Поле пароль пустe.");
            }

            if (string.IsNullOrEmpty(request.StoreId))
            {
                throw new ArgumentNullException(nameof(request.StoreId), "Поле ідентифікатор магазину пустe.");
            }

            if (string.IsNullOrEmpty(request.Permission))
            {
                throw new ArgumentNullException(nameof(request.Permission), "Поле дозвіл пустe.");
            }

            if (!Guid.TryParse(request.StoreId, out Guid storeId))
            {
                throw new ArgumentException("Унікальний ідентифікатор магазину передано в невірному форматі.");
            }

            if (storeId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор магазину передано в невірному форматі.");
            }

            if (!StringHelper.ValidateLogin(request.Login, out string exceptionMessageLogin))
            {
                throw new ArgumentException(exceptionMessageLogin);
            }

            if (!StringHelper.ValidatePassword(request.Password, out string exceptionMessage))
            {
                throw new ArgumentException(exceptionMessage);
            }

            return new(request.Login, request.Password, storeId, request.Permission);
        }

        public static LoginRequest VerificationData(this LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Login))
            {
                throw new ArgumentNullException(nameof(request.Login), "Поле логін пустe.");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentNullException(nameof(request.Password), "Поле пароль пустe.");
            }

            if (!StringHelper.ValidateLogin(request.Login, out string exceptionMessageLogin))
            {
                throw new ArgumentException(exceptionMessageLogin);
            }

            if (!StringHelper.ValidatePassword(request.Password, out string exceptionMessage))
            {
                throw new ArgumentException(exceptionMessage);
            }

            return request;
        }

        public static void VerificationData(this TokenClaims claims, string login, string permission)
        {
            if (string.IsNullOrEmpty(login))
            {
                throw new UnauthorizedAccessException("Поле логін пустe.");
            }

            if (string.IsNullOrEmpty(permission))
            {
                throw new UnauthorizedAccessException("Поле дозвіл пустe.");
            }

            if (claims.Login != login || claims.Permission != permission)
            {
                throw new UnauthorizedAccessException("Данні не збігаються.");
            }
        }

        public static ChangePasswordRequest VerificationData(this ChangePasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentNullException(nameof(request.Password), "Поле пароль пустe.");
            }

            if (string.IsNullOrEmpty(request.NewPassword))
            {
                throw new ArgumentNullException(nameof(request.Password), "Поле новий пароль пустe.");
            }

            if (!StringHelper.ValidatePassword(request.Password, out string exceptionMessagePass))
            {
                throw new ArgumentException(exceptionMessagePass);
            }

            if (!StringHelper.ValidatePassword(request.NewPassword, out string exceptionMessageNewPass))
            {
                throw new ArgumentException(exceptionMessageNewPass);
            }

            return request;
        }

        public static AddNewStoreRequestDto VerificationData(this AddNewStoreRequest request)
        {
            if (string.IsNullOrEmpty(request.CityId))
            {
                throw new ArgumentNullException(nameof(request.CityId), "Поле унікальний ідентифікатор міста пустe.");
            }

            if (string.IsNullOrEmpty(request.Address))
            {
                throw new ArgumentNullException(nameof(request.Address), "Поле адреса пустe.");
            }

            if (string.IsNullOrEmpty(request.Longitude))
            {
                throw new ArgumentNullException(nameof(request.Longitude), "Поле довгота пустe.");
            }

            if (string.IsNullOrEmpty(request.Latitude))
            {
                throw new ArgumentNullException(nameof(request.Latitude), "Поле широта пустe.");
            }

            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                throw new ArgumentNullException(nameof(request.PhoneNumber), "Поле номер телефону пустe.");
            }

            if (!Guid.TryParse(request.CityId, out Guid cityId))
            {
                throw new ArgumentException("Унікальний ідентифікатор міста передано в невірному форматі.");
            }

            if (cityId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор міста передано в невірному форматі.");
            }

            if (!decimal.TryParse(request.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal longitude))
            {
                throw new ArgumentException("Поле довгота передано в невірному форматі.");
            }

            if (!decimal.TryParse(request.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal latitude))
            {
                throw new ArgumentException("Поле широта передано в невірному форматі.");
            }

            string formattedPhoneNumber = Regex.Replace(
                    input: request.PhoneNumber.Trim(),
                    pattern: RegexPatterns.PhoneNumberFormattingPattern,
                    replacement: string.Empty);

            formattedPhoneNumber = '+' + formattedPhoneNumber;

            if (formattedPhoneNumber.Length <= 32 &&
                !formattedPhoneNumber.RegexIsMatch(RegexPatterns.PhoneNumber))
            {
                throw new ArgumentException("Поле номер телефону в невірному форматі.");
            }

            string formattedAddress = request.Address.Trim();
            if (formattedAddress.Length > 255)
            {
                formattedAddress = formattedAddress[..255];
            }

            decimal formattedLatitude = Math.Round(latitude, 6);
            decimal formattedLongitude = Math.Round(longitude, 6);

            return new(
                CityId: cityId,
                Address: formattedAddress,
                Latitude: formattedLatitude,
                Longitude: formattedLongitude,
                PhoneNumber: formattedPhoneNumber);
        }

        public static RemoveCityRequestDto VerificationData(this RemoveCityRequest request)
        {
            if (string.IsNullOrEmpty(request.CityId))
            {
                throw new ArgumentNullException(nameof(request.CityId), "Поле унікальний ідентифікатор міста пустe.");
            }

            if (!Guid.TryParse(request.CityId, out Guid cityId))
            {
                throw new ArgumentException("Унікальний ідентифікатор міста передано в невірному форматі.");
            }

            if (cityId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор міста передано в невірному форматі.");
            }

            return new(cityId);
        }

        public static RemoveStoreRequestDto VerificationData(this RemoveStoreRequest request)
        {
            if (string.IsNullOrEmpty(request.StoreId))
            {
                throw new ArgumentNullException(nameof(request.StoreId), "Поле унікальний ідентифікатор магазину пустe.");
            }

            if (!Guid.TryParse(request.StoreId, out Guid storeId))
            {
                throw new ArgumentException("Унікальний ідентифікатор магазину передано в невірному форматі.");
            }

            if (storeId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор магазину передано в невірному форматі.");
            }

            return new(storeId);
        }

        public static AddNewAdminRequestDto VerificationData(this AddNewAdminRequest request)
        {
            if (string.IsNullOrEmpty(request.Login))
            {
                throw new ArgumentNullException(nameof(request.Login), "Поле логін пустe.");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentNullException(nameof(request.Password), "Поле пароль пустe.");
            }

            if (string.IsNullOrEmpty(request.StoreId))
            {
                throw new ArgumentNullException(nameof(request.StoreId), "Поле унікальний ідентифікатор магазину пустe.");
            }

            if (!StringHelper.ValidateLogin(request.Login, out string exceptionMessageLogin))
            {
                throw new ArgumentException(exceptionMessageLogin);
            }

            if (!StringHelper.ValidatePassword(request.Password, out string exceptionMessage))
            {
                throw new ArgumentException(exceptionMessage);
            }

            if (!Guid.TryParse(request.StoreId, out Guid storeId))
            {
                throw new ArgumentException("Унікальний ідентифікатор магазину передано в невірному форматі.");
            }

            if (storeId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор магазину передано в невірному форматі.");
            }

            string validLogin = request.Login.Trim();
            if (validLogin.Length > 32)
            {
                validLogin = validLogin[..31];
            }
            string validPassword = request.Password.Trim();

            return new AddNewAdminRequestDto(
                validLogin,
                validPassword,
                storeId);
        }

        public static RemoveAdminRequestDto VerificationData(this RemoveAdminRequest request)
        {
            if (string.IsNullOrEmpty(request.AdminId))
            {
                throw new ArgumentNullException(nameof(request.AdminId), "Поле унікальний ідентифікатор адміністратора пустe.");
            }

            if (!Guid.TryParse(request.AdminId, out Guid adminId))
            {
                throw new ArgumentException("Унікальний ідентифікатор адміністратора передано в невірному форматі.");
            }

            if (adminId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор адміністратора передано в невірному форматі.");
            }

            return new(adminId);
        }

        public static ChangeAdminPasswordRequestDto VerificationData(this ChangeAdminPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.AdminId))
            {
                throw new ArgumentNullException(nameof(request.AdminId), "Поле унікальний ідентифікатор адміністратора пустe.");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentNullException(nameof(request.Password), "Поле пароль пустe.");
            }

            if (!Guid.TryParse(request.AdminId, out Guid adminId))
            {
                throw new ArgumentException("Унікальний ідентифікатор адміністратора передано в невірному форматі.");
            }

            if (adminId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор адміністратора передано в невірному форматі.");
            }

            string validPassword = request.Password.Trim();

            if (!StringHelper.ValidatePassword(validPassword, out string exceptionMessage))
            {
                throw new ArgumentException(exceptionMessage);
            }

            return new(adminId, validPassword);
        }

        public static ChangeCityNameRequestDto VerificationData(this ChangeCityNameRequest request)
        {
            if (string.IsNullOrEmpty(request.CityId))
            {
                throw new ArgumentNullException(nameof(request.CityId), "Поле унікальний ідентифікатор міста пустe.");
            }

            if (string.IsNullOrEmpty(request.CityName))
            {
                throw new ArgumentNullException(nameof(request.CityName), "Поле назва міста пустe.");
            }

            if (!Guid.TryParse(request.CityId, out Guid cityId))
            {
                throw new ArgumentException("Унікальний ідентифікатор міста передано в невірному форматі.");
            }

            if (cityId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор міста передано в невірному форматі.");
            }

            string validCityName = request.CityName.Trim();

            return new(cityId, validCityName);
        }

        public static ChangeStoreInfoRequestDto VerificationData(this ChangeStoreInfoRequest request)
        {
            if (string.IsNullOrEmpty(request.StoreId))
            {
                throw new ArgumentNullException(nameof(request.StoreId), "Поле унікальний ідентифікатор магазину пустe.");
            }

            string? address = null;
            string? phoneNumber = null;
            decimal? longitude = null;
            decimal? latitude = null;
            long? tgChatId = null;

            if (!string.IsNullOrEmpty(request.Address))
            {
                address = request.Address.Trim();
                if (address.Length > 255)
                {
                    address = address[..255];
                }
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                phoneNumber = Regex.Replace(
                    input: request.PhoneNumber.Trim(),
                    pattern: RegexPatterns.PhoneNumberFormattingPattern,
                    replacement: string.Empty);

                phoneNumber = '+' + phoneNumber;

                if (phoneNumber.Length <= 32 &&
                    !phoneNumber.RegexIsMatch(RegexPatterns.PhoneNumber))
                {
                    throw new ArgumentException("Поле номер телефону в невірному форматі.");
                }
            }

            if (!string.IsNullOrEmpty(request.Longitude))
            {
                if (!decimal.TryParse(request.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal longitudeTemp))
                {
                    throw new ArgumentException("Поле довгота передано в невірному форматі.");
                }

                longitude = Math.Round(longitudeTemp, 6);
            }

            if (!string.IsNullOrEmpty(request.Latitude))
            {
                if (!decimal.TryParse(request.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal latitudeTemp))
                {
                    throw new ArgumentException("Поле широта передано в невірному форматі.");
                }

                latitude = Math.Round(latitudeTemp, 6);
            }

            if (!string.IsNullOrEmpty(request.TgChatId))
            {
                if (!long.TryParse(request.TgChatId, out long chatId))
                {
                    throw new ArgumentException("Поле TG чат ID передано в невірному форматі.");
                }

                tgChatId = chatId;
            }

            if (!Guid.TryParse(request.StoreId, out Guid storeId))
            {
                throw new ArgumentException("Унікальний ідентифікатор магазину передано в невірному форматі.");
            }

            if (storeId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор магазину передано в невірному форматі.");
            }

            return new(storeId, address, phoneNumber, longitude, latitude, tgChatId);
        }

        public static ChangeStoreInfoRequestDto VerificationData(this ChangeStoreInfoBaseRequest request, HttpContext httpContext)
        {
            Guid storeId = GetStoreId(httpContext.User.Claims);

            if (storeId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор магазину передано в невірному форматі.");
            }

            string? address = null;
            string? phoneNumber = null;
            decimal? longitude = null;
            decimal? latitude = null;
            long? tgChatId = null;

            if (!string.IsNullOrEmpty(request.Address))
            {
                address = request.Address.Trim();
                if (address.Length > 255)
                {
                    address = address[..255];
                }
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                phoneNumber = Regex.Replace(
                    input: request.PhoneNumber.Trim(),
                    pattern: RegexPatterns.PhoneNumberFormattingPattern,
                    replacement: string.Empty);

                phoneNumber = '+' + phoneNumber;

                if (phoneNumber.Length <= 32 &&
                    !phoneNumber.RegexIsMatch(RegexPatterns.PhoneNumber))
                {
                    throw new ArgumentException("Поле номер телефону в невірному форматі.");
                }
            }

            if (!string.IsNullOrEmpty(request.Longitude))
            {
                if (!decimal.TryParse(request.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal longitudeTemp))
                {
                    throw new ArgumentException("Поле довгота передано в невірному форматі.");
                }

                longitude = Math.Round(longitudeTemp, 6);
            }

            if (!string.IsNullOrEmpty(request.Latitude))
            {
                if (!decimal.TryParse(request.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal latitudeTemp))
                {
                    throw new ArgumentException("Поле широта передано в невірному форматі.");
                }

                latitude = Math.Round(latitudeTemp, 6);
            }

            if (!string.IsNullOrEmpty(request.TgChatId))
            {
                if (!long.TryParse(request.TgChatId, out long chatId))
                {
                    throw new ArgumentException("Поле TG чат ID передано в невірному форматі.");
                }

                tgChatId = chatId;
            }

            return new(storeId, address, phoneNumber, longitude, latitude, tgChatId);
        }

        public static AddNewProductDto VerificationData(this AddNewProductMetadata request, string extension, string contentType)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException("З фото щось не так.");
            }

            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentException("З фото щось не так.");
            }

            if (string.IsNullOrEmpty(extension) || !_permittedExtensions.Contains(extension) || !_permittedContentTypes.Contains(contentType))
            {
                throw new ArgumentException("Недійсний формат файлу. Дозволені лише .webp .jpg, .jpeg і .png.");
            }

            if (string.IsNullOrEmpty(request.TypeId))
            {
                throw new ArgumentNullException(nameof(request.TypeId), "Поле унікальний ідентифікатор типу пустe.");
            }

            if (string.IsNullOrEmpty(request.UnderTypeId))
            {
                throw new ArgumentNullException(nameof(request.UnderTypeId), "Поле унікальний ідентифікатор під-типу пустe.");
            }

            if (string.IsNullOrEmpty(request.Name))
            {
                throw new ArgumentNullException(nameof(request.Name), "Поле назва пустe.");
            }

            if (string.IsNullOrEmpty(request.Price))
            {
                throw new ArgumentNullException(nameof(request.Price), "Поле ціна пустe.");
            }

            string name;
            decimal price;
            string? dimensions = null;
            string? description = null;

            #region typeId
            if (!Guid.TryParse(request.TypeId, out Guid typeId))
            {
                throw new ArgumentException("Унікальний ідентифікатор типу передано в невірному форматі.");
            }

            if (typeId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор типу передано в невірному форматі.");
            }
            #endregion

            #region underTypeId
            if (!Guid.TryParse(request.UnderTypeId, out Guid underTypeId))
            {
                throw new ArgumentException("Унікальний ідентифікатор під-типу передано в невірному форматі.");
            }

            if (underTypeId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор під-типу передано в невірному форматі.");
            }
            #endregion

            #region name
            name = request.Name.Trim();
            if (name.Length > 31)
            {
                name = name[..31];
            }
            #endregion

            #region price
            if (!decimal.TryParse(request.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal priceTemp))
            {
                throw new ArgumentException("Поле ціна передано в невірному форматі.");
            }

            price = Math.Round(priceTemp, 2);
            #endregion

            #region dimensions
            if (!string.IsNullOrEmpty(request.Dimensions))
            {
                dimensions = request.Dimensions.Trim();
                if (dimensions.Length > 31)
                {
                    dimensions = dimensions[..31];
                }
            }
            #endregion

            #region description
            if (!string.IsNullOrEmpty(request.Description))
            {
                description = request.Description.Trim();
                if (description.Length > 63)
                {
                    description = description[..63];
                }
            }
            #endregion

            return new()
            {
                TypeId = typeId,
                UnderTypeId = underTypeId,
                Name = name,
                Price = price,
                Dimensions = dimensions,
                Description = description,
                PhotoExtension = extension
            };
        }

        public static ChangeProductDto VerificationData(this ChangeProductMetadata request, string extension, string contentType)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException("З фото щось не так.");
            }

            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentException("З фото щось не так.");
            }

            if (string.IsNullOrEmpty(extension) || !_permittedExtensions.Contains(extension) || !_permittedContentTypes.Contains(contentType))
            {
                throw new ArgumentException("Недійсний формат файлу. Дозволені лише .webp .jpg, .jpeg і .png.");
            }

            if (string.IsNullOrEmpty(request.ProductId))
            {
                throw new ArgumentNullException(nameof(request.ProductId), "Поле унікальний ідентифікатор товару пустe.");
            }

            string? name = null;
            decimal? price = null;
            string? dimensions = null;
            string? description = null;

            #region productId
            if (!Guid.TryParse(request.ProductId, out Guid productId))
            {
                throw new ArgumentException("Унікальний ідентифікатор товару передано в невірному форматі.");
            }

            if (productId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор товару передано в невірному форматі.");
            }
            #endregion

            #region name
            if (!string.IsNullOrEmpty(request.Name))
            {
                name = request.Name.Trim();
                if (name.Length > 31)
                {
                    name = name[..31];
                }
            }
            #endregion

            #region price
            if (!string.IsNullOrEmpty(request.Price))
            {
                if (!decimal.TryParse(request.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal priceTemp))
                {
                    throw new ArgumentException("Поле ціна передано в невірному форматі.");
                }

                price = Math.Round(priceTemp, 2);
            }
            #endregion

            #region dimensions
            if (!string.IsNullOrEmpty(request.Dimensions))
            {
                dimensions = request.Dimensions.Trim();
                if (dimensions.Length > 31)
                {
                    dimensions = dimensions[..31];
                }
            }
            #endregion

            #region description
            if (!string.IsNullOrEmpty(request.Description))
            {
                description = request.Description.Trim();
                if (description.Length > 63)
                {
                    description = description[..63];
                }
            }
            #endregion

            return new()
            {
                ProductId = productId,
                Name = name,
                Price = price,
                Dimensions = dimensions,
                Description = description,
                PhotoExtension = extension
            };
        }

        public static ChangeProductDto VerificationData(this ChangeProductMetadata request)
        {
            if (string.IsNullOrEmpty(request.ProductId))
            {
                throw new ArgumentNullException(nameof(request.ProductId), "Поле унікальний ідентифікатор товару пустe.");
            }

            string? name = null;
            decimal? price = null;
            string? dimensions = null;
            string? description = null;

            #region productId
            if (!Guid.TryParse(request.ProductId, out Guid productId))
            {
                throw new ArgumentException("Унікальний ідентифікатор товару передано в невірному форматі.");
            }

            if (productId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор товару передано в невірному форматі.");
            }
            #endregion

            #region name
            if (!string.IsNullOrEmpty(request.Name))
            {
                name = request.Name.Trim();
                if (name.Length > 31)
                {
                    name = name[..31];
                }
            }
            #endregion

            #region price
            if (!string.IsNullOrEmpty(request.Price))
            {
                if (!decimal.TryParse(request.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal priceTemp))
                {
                    throw new ArgumentException("Поле ціна передано в невірному форматі.");
                }

                price = Math.Round(priceTemp, 2);
            }
            #endregion

            #region dimensions
            if (!string.IsNullOrEmpty(request.Dimensions))
            {
                dimensions = request.Dimensions.Trim();
                if (dimensions.Length > 31)
                {
                    dimensions = dimensions[..31];
                }
            }
            #endregion

            #region description
            if (!string.IsNullOrEmpty(request.Description))
            {
                description = request.Description.Trim();
                if (description.Length > 63)
                {
                    description = description[..63];
                }
            }
            #endregion

            return new()
            {
                ProductId = productId,
                Name = name,
                Price = price,
                Dimensions = dimensions,
                Description = description
            };
        }

        public static Guid GetStoreId(this IEnumerable<Claim> claims)
        {
            string locality = claims
                .SingleOrDefault(claim => claim.Type == ClaimTypes.Locality)?.Value
                    ?? throw new ArgumentException("Токен не знайдено.");

            if (!Guid.TryParse(locality, out Guid storeId))
            {
                throw new ArgumentException("Унікальний ідентифікатор магазину передано в неправильному форматі.");
            }

            if (storeId == Guid.Empty)
            {
                throw new ArgumentException("Унікальний ідентифікатор магазину передано в неправильному форматі.");
            }

            return storeId;
        }
    }
}