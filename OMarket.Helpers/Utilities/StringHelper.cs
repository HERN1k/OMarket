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
    }
}