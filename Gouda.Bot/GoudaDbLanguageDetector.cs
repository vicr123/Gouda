using Gouda.Database;
using I18Next.Net.Plugins;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;

namespace Gouda.Bot;

public class GoudaDbLanguageDetector(IInteractionCommandContext interactionContext, GoudaDbContext dbContext) : ILanguageDetector
{
    public string GetLanguage()
    {
        var haveUserId = interactionContext.TryGetUserID(out var userId);
        if (haveUserId)
        {
            var localeName = dbContext.Set<Locale>().FirstOrDefault(x => x.UserId == userId.Value)?.LocaleName;
            if (localeName is not null)
            {
                return localeName;
            }
        }

        var discordLocale = interactionContext.Interaction.Locale;
        if (discordLocale.HasValue)
        {
            return discordLocale.Value;
        }

        return "en";
    }
}
