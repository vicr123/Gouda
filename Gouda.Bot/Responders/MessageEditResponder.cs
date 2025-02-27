using Gouda.Bot.Extensions;
using Gouda.Database;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;
using Remora.Discord.Extensions.Formatting;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Gouda.Bot.Responders;

[UsedImplicitly]
public class MessageEditResponder(GoudaDbContext dbContext, CacheService cache, IDiscordRestChannelAPI channelApi) : IResponder<IMessageUpdate>
{
    public async Task<Result> RespondAsync(IMessageUpdate gatewayEvent, CancellationToken ct = default)
    {
        var oldMessageContent = string.Empty;

        var prevEdit =
            await cache.TryGetValueAsync<IMessage>(
                new KeyHelpers.MessageCacheKey(gatewayEvent.ChannelID, gatewayEvent.ID), ct);
        if (prevEdit.IsSuccess)
        {
            oldMessageContent = prevEdit.Entity.Content;
        }
        else
        {
            var prevMsg = await cache.TryGetValueAsync<IMessageCreate>(new KeyHelpers.MessageCacheKey(gatewayEvent.ChannelID, gatewayEvent.ID), ct);
            if (prevMsg.IsSuccess)
            {
                oldMessageContent = prevMsg.Entity.Content;
            }
        }

        if (oldMessageContent == gatewayEvent.Content)
        {
            return Result.Success;
        }

        // Determine if we should be sending chat logs
        var logsChannel = await dbContext.GuildChannels.FirstOrDefaultAsync(x => x.Id == gatewayEvent.GuildID.Value.Value, cancellationToken: ct);
        if (logsChannel?.ChatLogChannel is null)
        {
            return Result.Success;
        }

        await channelApi.CreateMessageAsync(
            new(logsChannel.ChatLogChannel.Value),
            $"""
            :pencil: **{gatewayEvent.Author.DisplayName()}** ({gatewayEvent.Author.ID.Value.ToString()}) {Mention.Channel(gatewayEvent.ChannelID)} `{DateTime.UtcNow:F}`
            ```
            {oldMessageContent}
            ``````
            {gatewayEvent.Content}
            ```https://discordapp.com/channels/{gatewayEvent.GuildID.Value.ToString()}/{gatewayEvent.ChannelID.Value.ToString()}/{gatewayEvent.ID.Value.ToString()}
            """,
            ct: ct);

        return Result.Success;
    }
}
