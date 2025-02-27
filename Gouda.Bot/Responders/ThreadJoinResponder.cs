using JetBrains.Annotations;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Gouda.Bot.Responders;

[UsedImplicitly]
public class ThreadJoinResponder(IDiscordRestChannelAPI channelApi) : IResponder<IThreadCreate>
{
    public async Task<Result> RespondAsync(IThreadCreate gatewayEvent, CancellationToken ct)
    {
        await channelApi.JoinThreadAsync(gatewayEvent.ID, ct);
        return Result.FromSuccess();
    }
}
