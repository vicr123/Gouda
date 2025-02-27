using Gouda.Bot.Services;
using JetBrains.Annotations;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Gouda.Bot.Responders;

[UsedImplicitly]
public class ReadyResponder(
    IDiscordRestUserAPI userApi,
    ThreadPresenceService threadPresenceService) : IResponder<IReady>
{
    public async Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct)
    {
        var joinedGuilds = await userApi.GetCurrentUserGuildsAsync(ct: ct);

        if (!joinedGuilds.IsSuccess)
        {
            return Result.FromSuccess();
        }

        foreach (var joinedGuild in joinedGuilds.Entity)
        {
            await threadPresenceService.ProcessGuild(joinedGuild, ct);
        }

        return Result.FromSuccess();
    }
}
