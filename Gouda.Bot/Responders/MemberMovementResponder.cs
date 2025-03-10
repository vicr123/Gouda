using Gouda.Bot.Extensions;
using Gouda.Database;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Extensions.Formatting;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Gouda.Bot.Responders;

[UsedImplicitly]
public class MemberMovementResponder(GoudaDbContext dbContext, IDiscordRestChannelAPI channelApi) : IResponder<IGuildMemberAdd>, IResponder<IGuildMemberRemove>
{
    public async Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
    {
        // Determine if we should be sending alert logs
        var logsChannel = await dbContext.GuildChannels.FirstOrDefaultAsync(x => x.Id == gatewayEvent.GuildID.Value, cancellationToken: ct);
        if (logsChannel?.AlertChannel is null)
        {
            return Result.Success;
        }

        await channelApi.CreateMessageAsync(
            new(logsChannel.AlertChannel.Value),
            $":arrow_right: {Mention.User(gatewayEvent.User.Value)}",
            ct: ct);

        return Result.Success;
    }

    public async Task<Result> RespondAsync(IGuildMemberRemove gatewayEvent, CancellationToken ct = default)
    {
        // Determine if we should be sending alert logs
        var logsChannel = await dbContext.GuildChannels.FirstOrDefaultAsync(x => x.Id == gatewayEvent.GuildID.Value, cancellationToken: ct);
        if (logsChannel?.AlertChannel is null)
        {
            return Result.Success;
        }

        await channelApi.CreateMessageAsync(
            new(logsChannel.AlertChannel.Value),
            $":arrow_left: {Mention.User(gatewayEvent.User)} ({gatewayEvent.User.DisplayName()})",
            ct: ct);

        return Result.Success;
    }
}
