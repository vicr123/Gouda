using System.Globalization;
using Gouda.Database;
using I18Next.Net;
using I18Next.Net.Plugins;
using Remora.Discord.Commands.Contexts;

namespace Gouda.Bot.Services;

public class TranslationService
{
    private readonly II18Next _i18Next;

    public TranslationService(IInteractionCommandContext interactionContext, GoudaDbContext dbContext)
    {
        var backend = new BotResourcesTranslationBackend();
        var languageDetector = new GoudaDbLanguageDetector(interactionContext, dbContext);
        _i18Next = new I18NextNet(
            backend,
            new DefaultTranslator(backend),
            languageDetector)
        {
            FallbackLanguages = ["en"],
        };
        _i18Next.UseDetectedLanguage();

        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(languageDetector.GetLanguage());
    }

    public string this[string key] => _i18Next.T(key);

    public string this[string key, object args] => _i18Next.T(key, args);

    public CultureInfo Culture => new(_i18Next.Language);
}
