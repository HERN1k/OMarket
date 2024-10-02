using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;

using OMarket.Domain.DTOs;
using OMarket.Helpers.Extensions;

namespace OMarket.Helpers.Utilities
{
    public static class ExceptionHelper
    {
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

        public static RemoveAdminRequest VerificationData(this RemoveAdminRequest request)
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
    }
}