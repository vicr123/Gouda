using Gouda.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Extensions.Embeds;
using Remora.Discord.Extensions.Formatting;
using Remora.Rest.Core;

namespace Gouda.Bot.Services;

public class PinService(GoudaDbContext dbContext, IDiscordRestChannelAPI channelApi, IMemoryCache memoryCache)
{
    public async Task PinMessage(ulong channelId, ulong messageId, ulong userId)
    {
        var newPinId = dbContext.Pins.Where(x => x.UserId == userId).Select(x => x.PinNumber).DefaultIfEmpty().Max() + 1;
        dbContext.Pins.Add(new()
        {
            PinNumber = newPinId,
            UserId = userId,
            Channel = channelId,
            Message = messageId,
        });
        await dbContext.SaveChangesAsync();

        var channelTask = channelApi.GetChannelAsync(new(channelId));
        var messageTask = channelApi.GetChannelMessageAsync(new(channelId), new(messageId));

        var channel = await channelTask;
        var pinnedMessage = await messageTask;

        var message = await channelApi.CreateMessageAsync(
            new(channelId),
            embeds: new([
                new EmbedBuilder()
                    .WithTitle("Pin")
                    .WithDescription("The above messge was pinned")
                    .AddField(pinnedMessage.Entity.Author.Username, $"""
                        {(pinnedMessage.Entity.Attachments.Any() ? $"({pinnedMessage.Entity.Attachments.Count} attachments)" : string.Empty)}
                        {pinnedMessage.Entity.Content}
                        — {Mention.User(new Snowflake(userId))} ({Markdown.Hyperlink("Jump", $"https://discord.com/channels/{channel.Entity.GuildID}/{channelId}/{messageId}")})
                        """).Entity
                    .Build().Entity,
            ]),
            messageReference: new(new MessageReference(
                new(MessageReferenceType.Default),
                MessageID: new(new(messageId)),
                ChannelID: new(new(channelId)),
                GuildID: channel.Entity.GuildID)));

        memoryCache.Set($"pin-{channelId}-{messageId}", message.Entity.ID);
    }

    public async Task UnpinMessage(ulong channelId, ulong messageId, ulong userId)
    {
        var pin = await dbContext.Pins.FirstOrDefaultAsync(x => x.Channel == channelId && x.Message == messageId && x.UserId == userId);
        if (pin is not null)
        {
            dbContext.Pins.Remove(pin);
            await dbContext.SaveChangesAsync();

            var havePinMessage = memoryCache.TryGetValue($"pin-{channelId}-{messageId}", out Snowflake pinMessageId);
            if (havePinMessage)
            {
                await channelApi.DeleteMessageAsync(new(channelId), pinMessageId);
                memoryCache.Remove($"pin-{channelId}-{messageId}");
            }
        }
    }
}