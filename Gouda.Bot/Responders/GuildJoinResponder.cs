using Gouda.Bot.Services;
using JetBrains.Annotations;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Gouda.Bot.Responders;

[UsedImplicitly]
public class GuildJoinResponder(ThreadPresenceService threadPresenceService) : IResponder<IGuildCreate>
{
    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        if (!gatewayEvent.Guild.IsT1)
        {
            await threadPresenceService.ProcessGuild(gatewayEvent.Guild.AsT0, ct);
        }

        return Result.Success;
    }
}
