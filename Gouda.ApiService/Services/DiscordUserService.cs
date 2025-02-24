using Remora.Discord.API.Abstractions.Rest;

namespace Gouda.ApiService.Services;

public class DiscordUserService(IDiscordRestUserAPI discordRestUserApi, IHttpContextAccessor httpContextAccessor)
{
    public async Task<ulong?> LoggedInUserIdAsync()
    {
        if (httpContextAccessor.HttpContext is null)
        {
            return null;
        }

        using var restRequestCustomization = await discordRestUserApi.Userify(httpContextAccessor.HttpContext);
        var currentUser = await discordRestUserApi.GetCurrentUserAsync();
        if (!currentUser.IsSuccess)
        {
            return null;
        }

        return currentUser.Entity.ID.Value;
    }
}
