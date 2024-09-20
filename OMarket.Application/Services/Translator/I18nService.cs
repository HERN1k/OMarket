using System.Collections.Frozen;
using OMarket.Domain.Enums;
using OMarket.Domain.Interfaces.Application.Services.Translator;
using OMarket.Helpers.Extensions;

namespace OMarket.Application.Services.Translator
{
    public class I18nService : II18nService
    {
        private readonly FrozenDictionary<string, string> _uk;

        public I18nService(ILocalizationData localization)
        {
            _uk = localization.Uk;
        }

        public string T(string key, LanguageCode? code = null)
        {
            if (code is not null)
            {
                return code switch
                {
                    LanguageCode.UK => _uk.TryGetTranslation(key),
                    _ => string.Empty,
                };
            }
            else
            {
                return _uk.TryGetTranslation(key);
            }
        }
    }
}