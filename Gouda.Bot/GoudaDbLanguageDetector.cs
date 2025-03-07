using Gouda.Database;
using I18Next.Net.Plugins;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Rest.Core;

namespace Gouda.Bot;

public class GoudaDbLanguageDetector(IServiceProvider serviceProvider, GoudaDbContext dbContext) : ILanguageDetector
{
    public string GetLanguage()
    {
        try
        {
            var interactionContext = serviceProvider.GetService<IInteractionCommandContext>();
            if (interactionContext is not null)
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
            }
        }
        catch (InvalidOperationException)
        {
        }

        var gatewayEvent = serviceProvider.GetService<IGatewayEvent>();
        if (gatewayEvent is not null)
        {
            var userIdProperty = gatewayEvent.GetType().GetProperty("UserId");
            if (userIdProperty?.GetValue(gatewayEvent) is Optional<Snowflake> userId)
            {
                var localeName = dbContext.Set<Locale>().FirstOrDefault(x => x.UserId == userId.Value)?.LocaleName;
                if (localeName is not null)
                {
                    return localeName;
                }
            }
        }

        return "en";
    }
}
