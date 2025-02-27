using Gouda.Database;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;

namespace Gouda.Bot.Services;

public class GuildAlertService(GoudaDbContext dbContext, IDiscordRestChannelAPI channelApi)
{
    public async Task SendAlert(IPartialGuild guild, string alert, CancellationToken ct = default)
    {
        // Determine if we should be sending chat logs
        var logsChannel = await dbContext.GuildChannels.FirstOrDefaultAsync(x => x.Id == guild.ID.Value.Value, cancellationToken: ct);
        if (logsChannel?.AlertChannel is null)
        {
            // Too bad, no alerts for you!
            return;
        }

        await channelApi.CreateMessageAsync(new(logsChannel.AlertChannel.Value), content: alert, ct: ct);
    }
}
