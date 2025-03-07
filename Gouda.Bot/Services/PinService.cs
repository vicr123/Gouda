using Gouda.Bot.Extensions;
using Gouda.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Extensions.Embeds;
using Remora.Discord.Extensions.Formatting;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;

namespace Gouda.Bot.Services;

public class PinService(GoudaDbContext dbContext, IDiscordRestChannelAPI channelApi, IMemoryCache memoryCache, TranslationService translationService, IDiscordRestUserAPI userApi, IOptions<GoudaConfiguration> options)
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
                    .WithTitle(translationService["PIN_DONE"])
                    .WithDescription(translationService["PIN_MESSAGE"])
                    .AddField(pinnedMessage.Entity.Author.Username, BuildMessageString(pinnedMessage.Entity, channel.Entity.GuildID.Value.Value)).Entity
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

    public async Task<(Embed Embed, IReadOnlyList<IMessageComponent> Components)> ListPins(ulong userId, int page)
    {
        var user = await userApi.GetUserAsync(new(userId));
        var pins = dbContext.Pins.Where(x => x.UserId == userId).OrderByDescending(x => x.PinNumber).Skip(page * 5).Take(5);
        var totalPins = await dbContext.Pins.CountAsync(x => x.UserId == userId);

        var fields = pins.ToAsyncEnumerable().SelectAwait(async pin =>
        {
            var channel = channelApi.GetChannelAsync(new(pin.Channel));
            var message = channelApi.GetChannelMessageAsync(new(pin.Channel), new(pin.Message));

            return new
            {
                pin.PinNumber,
                Message = (await message).Entity,
                GuildId = (await channel).Entity.GuildID,
            };
        });

        var embed = new EmbedBuilder()
            .WithTitle($"Pins - {user.Entity.DisplayName()}");

        await foreach (var pin in fields)
        {
            embed = embed.AddField($"#{pin.PinNumber}", BuildMessageString(pin.Message, pin.GuildId.Value.Value))
                .Entity;
        }

        return (embed.Build().Entity, [
            new ActionRowComponent([
                new ButtonComponent(ButtonComponentStyle.Primary, "\u2b05\ufe0f Newer", CustomID: CustomIDHelpers.CreateButtonIDWithState("change-pin-page", $"{page - 1}_{userId}"), IsDisabled: page == 0),
                new ButtonComponent(ButtonComponentStyle.Primary, "Older \u27a1\ufe0f", CustomID: CustomIDHelpers.CreateButtonIDWithState("change-pin-page", $"{page + 1}_{userId}"), IsDisabled: (page + 1) * 5 >= totalPins),
                new ButtonComponent(ButtonComponentStyle.Link, "View Online", URL: $"{options.Value.ConfigurationUrl}/pins")
            ]),
        ]);
    }

    private string BuildMessageString(IMessage pinnedMessage, ulong guildId)
    {
        return $"""
                {(pinnedMessage.Attachments.Any() ? $"({translationService["PIN_ATTACHMENTS", new
                {
                    count = pinnedMessage.Attachments.Count,
                }
            ]})" : string.Empty)}
                {pinnedMessage.Content}
                — {Mention.User(pinnedMessage.Author)} ({Markdown.Hyperlink(translationService["PIN_JUMP"], $"https://discord.com/channels/{guildId}/{pinnedMessage.ChannelID}/{pinnedMessage.ID}")})
                """;
    }
}