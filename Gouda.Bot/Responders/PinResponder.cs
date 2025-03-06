using Gouda.Bot.Services;
using Gouda.Database;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Gouda.Bot.Responders;

public class PinResponder(GoudaDbContext dbContext, PinService pinService) : IResponder<IMessageReactionAdd>, IResponder<IMessageReactionRemove>
{
    public async Task<Result> RespondAsync(IMessageReactionAdd gatewayEvent, CancellationToken ct = default)
    {
        var pinEmoji = (await dbContext.GuildPins.FirstOrDefaultAsync(x => x.Id == gatewayEvent.GuildID.Value.Value, cancellationToken: ct))
            ?.PinEmoji ?? "\ud83d\udc7a"; // Japanese Goblin
        if (gatewayEvent.Emoji.Name.Value != pinEmoji)
        {
            return Result.Success;
        }

        await pinService.PinMessage(gatewayEvent.ChannelID.Value, gatewayEvent.MessageID.Value, gatewayEvent.UserID.Value);

        return Result.Success;
    }

    public async Task<Result> RespondAsync(IMessageReactionRemove gatewayEvent, CancellationToken ct = default)
    {
        var pinEmoji = (await dbContext.GuildPins.FirstOrDefaultAsync(x => x.Id == gatewayEvent.GuildID.Value.Value, cancellationToken: ct))
            ?.PinEmoji ?? "\ud83d\udc7a"; // Japanese Goblin
        if (gatewayEvent.Emoji.Name.Value != pinEmoji)
        {
            return Result.Success;
        }

        await pinService.UnpinMessage(gatewayEvent.ChannelID.Value, gatewayEvent.MessageID.Value, gatewayEvent.UserID.Value);

        return Result.Success;
    }
}