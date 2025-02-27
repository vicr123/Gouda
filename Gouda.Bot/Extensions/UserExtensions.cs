using Remora.Discord.API.Abstractions.Objects;

namespace Gouda.Bot.Extensions;

public static class UserExtensions
{
    public static string DisplayName(this IPartialUser user)
    {
        if (user.IsBot is { HasValue: true, Value: true })
        {
            return $"**{user.Username}#{user.Discriminator}** `[BOT]`";
        }

        return $"**{user.Username}**";
    }
}
