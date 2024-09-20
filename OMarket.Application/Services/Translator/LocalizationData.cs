using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using System.Text;

using Microsoft.Extensions.Logging;

using OMarket.Domain.Interfaces.Application.Services.Translator;

namespace OMarket.Application.Services.Translator
{
    public class LocalizationData : ILocalizationData
    {
        public FrozenDictionary<string, string> Uk { get; private set; } = null!;

        private readonly ILogger<LocalizationData> _logger;

        private List<string> LocalizationCodes { get; init; } = new() { "UK" };

        public LocalizationData(ILogger<LocalizationData> logger)
        {
            _logger = logger;
        }

        public void MapLocalizationData(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Starting localization data mapping...");

            ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 5 };

            Parallel.ForEach(LocalizationCodes, parallelOptions, code =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                string path = GetFolderLocalizationPath(code, cancellationToken);

                ConcurrentDictionary<string, string> dictionary = MapLocalizationDataToDictionary(path, cancellationToken);

                switch (code.ToUpper())
                {
                    case "UK":
                        Uk = dictionary.ToFrozenDictionary();
                        break;
                    default:
                        throw new ArgumentException(nameof(LocalizationCodes));
                }

                cancellationToken.ThrowIfCancellationRequested();
            });

            _logger.LogInformation("Localization data mapping completed.");
        }

        private static ConcurrentDictionary<string, string> MapLocalizationDataToDictionary(string path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ConcurrentBag<string> lines = ReadAllLines(path, cancellationToken);

            ConcurrentDictionary<string, string> result = new();

            foreach (string line in lines)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                if (line.StartsWith('{'))
                {
                    continue;
                }
                if (line.StartsWith('}'))
                {
                    continue;
                }

                string trimLine = line.Trim();

                if (trimLine.StartsWith("//"))
                {
                    continue;
                }

                string[] splitLines = trimLine
                  .Split("//");

                if (splitLines.Length < 1)
                {
                    continue;
                }

                string tempLine = splitLines[0];

                tempLine = tempLine.Trim();

                string[] pair = tempLine
                  .Replace('"', ' ')
                  .Split(":", 2);

                if (pair.Length != 2)
                {
                    continue;
                }

                if (pair[1].EndsWith(','))
                {
                    pair[1] = pair[1]
                      .Remove(pair[1].Length - 1, 1);
                }

                string key = pair[0].Trim();

                string value = pair[1].Trim();

                result.TryAdd(key, value);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return result;
        }

        private static ConcurrentBag<string> ReadAllLines(string path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            ConcurrentBag<string> lines = new();

            using (StreamReader reader = new(path, Encoding.UTF8))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    lines.Add(line);
                }
            }

            return lines;
        }

        private static string GetFolderLocalizationPath(string code, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            string? executableFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (string.IsNullOrEmpty(executableFilePath))
            {
                throw new ArgumentNullException(nameof(executableFilePath));
            }

            string filePath = Path.Combine(executableFilePath, "LanguageFiles", $"{code.ToUpper()}.json");

            if (!File.Exists(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            return filePath;
        }
    }
}