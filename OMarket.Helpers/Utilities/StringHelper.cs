using System.Text.RegularExpressions;

using OMarket.Domain.Exceptions.Telegram;
using OMarket.Helpers.Extensions;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OMarket.Helpers.Utilities
{
    public static class StringHelper
    {
        public static bool RegexIsMatch(string input, string pattern)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pattern, nameof(pattern));

            if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return Regex.IsMatch(input, pattern);
        }

        public static bool IsBackCommand(Update update)
        {
            string command = GetMessageFromUpdate(update);

            if (!command.StartsWith('/'))
            {
                throw new TelegramException();
            }

            string[] lines = command.Split('_', 2);

            if (lines.Length > 1)
            {
                return lines[1].ToUpper()
                    .RegexIsMatch(RegexPatterns.Back);
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

            string result = lines[1].ToUpper();

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

        public static string GetCityNameFromQuery(string query)
        {
            return query switch
            {
                "SMILA" => "м. Сміла",
                _ => throw new TelegramException()
            };
        }

        public static string GetQueryFromCityName(string city)
        {
            return city switch
            {
                "м. Сміла" => "SMILA",
                _ => throw new TelegramException()
            };
        }
    }
}