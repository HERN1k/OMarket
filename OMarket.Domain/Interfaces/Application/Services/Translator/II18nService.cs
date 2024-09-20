using OMarket.Domain.Enums;

namespace OMarket.Domain.Interfaces.Application.Services.Translator
{
    public interface II18nService
    {
        string T(string key, LanguageCode? code = null);
    }
}