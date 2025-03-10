using Remora.Discord.API.Abstractions.Objects;

namespace Gouda.Bot.Helpers;

public static class UserHelpers
{
    public static string GetAvatarUrl(IUser user)
    {
        var baseAvatarUrl = $"https://cdn.discordapp.com/embed/avatars/{(user.ID.Value >> 22) % 6}.png";
        if (user.Avatar is not null)
        {
            baseAvatarUrl = $"https://cdn.discordapp.com/avatars/{user.ID.Value}/{user.Avatar.Value}.{(user.Avatar.HasGif ? "gif" : "png")}";
        }

        return baseAvatarUrl;
    }
}