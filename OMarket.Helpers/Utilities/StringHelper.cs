using OMarket.Domain.Exceptions.Telegram;
using OMarket.Helpers.Extensions;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OMarket.Helpers.Utilities
{
    public static class StringHelper
    {
        public static bool IsBackCommand(Update update, out string command)
        {
            string commandTemp = GetMessageFromUpdate(update);

            if (!commandTemp.StartsWith('/'))
            {
                throw new TelegramException();
            }

            string[] lines = commandTemp.Split('_');

            if (lines.Length == 1)
            {
                command = string.Empty;

                return false;
            }
            else if (lines.Length == 2)
            {
                command = lines[^1];

                return command.ToUpper()
                    .RegexIsMatch(RegexPatterns.Back);
            }
            else if (lines.Length > 2)
            {
                command = $"{lines[^2]}_{lines[^1]}";

                return command.ToUpper()
                    .RegexIsMatch(RegexPatterns.Back);
            }

            command = string.Empty;

            return false;
        }

        public static bool IsDelCommand(string command)
        {
            string[] lines = command.Split('_', 2);

            if (lines.Length > 1)
            {
                return lines[1].ToUpper()
                    .RegexIsMatch(RegexPatterns.Del);
            }

            return false;
        }

        public static string GetQueryFromCommand(Update update)
        {
            string command = GetMessageFromUpdate(update);

            if (!command.StartsWith('/'))
            {
                return string.Empty;
            }

            string[] lines = command.Split('_', 2);

            if (lines.Length <= 1)
            {
                return string.Empty;
            }

            string result = lines[1];

            return result;
        }

        public static string GetMessageFromUpdate(Update update)
        {
            if (update.Type == UpdateType.Message)
            {
                return update.Message?.Text ?? string.Empty;
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                return update.CallbackQuery?.Data ?? string.Empty;
            }
            else
            {
                throw new TelegramException();
            }
        }

        public static bool ValidateLogin(string login, out string errorMessage)
        {
            if (login.Length < 3)
            {
                errorMessage = "Логін має бути не менше 3 символів.";
                return false;
            }

            if (login.Length > 32)
            {
                errorMessage = "Логін має бути не більше 32 символів.";
                return false;
            }

            if (login.Contains(' '))
            {
                errorMessage = "Логін не повинен містити пробіли.";
                return false;
            }

            if (!login.RegexIsMatch(RegexPatterns.Login))
            {
                errorMessage = "Дані введені в невірному форматі.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public static bool ValidatePassword(string password, out string errorMessage)
        {
            if (password.Length < 8)
            {
                errorMessage = "Пароль має бути не менше 8 символів.";
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                errorMessage = "Пароль повинен містити хоча б одну велику літеру.";
                return false;
            }

            if (!password.Any(char.IsLower))
            {
                errorMessage = "Пароль повинен містити хоча б одну малу літеру.";
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                errorMessage = "Пароль повинен містити хоча б одну цифру.";
                return false;
            }

            if (password.Contains(' '))
            {
                errorMessage = "Пароль не повинен містити пробіли.";
                return false;
            }

            if (!password.RegexIsMatch(RegexPatterns.Password))
            {
                errorMessage = "Дані введені в невірному форматі.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}