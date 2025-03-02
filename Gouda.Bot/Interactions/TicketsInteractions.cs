using System.Globalization;
using Gouda.Bot.Extensions;
using Gouda.Bot.Services;
using Gouda.Database;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Discord.Extensions.Formatting;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;
using Remora.Results;

namespace Gouda.Bot.Interactions;

public class TicketsInteractions(IInteractionCommandContext interactionCommandContext, IFeedbackService feedbackService, IDiscordRestUserAPI userApi, IDiscordRestChannelAPI channelApi, GoudaDbContext dbContext, IDiscordRestInteractionAPI interactionApi, TranslationService translationService) : InteractionGroup
{
    [Button("ticket")]
    [Ephemeral]
    [SuppressInteractionResponse(true)]
    public async Task<Result> OpenTicket()
    {
        if (!interactionCommandContext.TryGetUserID(out var userId))
        {
            throw new InvalidOperationException("Failed to get user ID");
        }

        if (!interactionCommandContext.TryGetChannelID(out var channelId))
        {
            throw new InvalidOperationException("Failed to get channel ID");
        }

        if (!interactionCommandContext.TryGetGuildID(out var guildId))
        {
            throw new InvalidOperationException("Failed to get guild ID");
        }

        return await interactionApi.CreateInteractionResponseAsync(
            interactionCommandContext.Interaction.ID,
            interactionCommandContext.Interaction.Token,
            new InteractionResponse(InteractionCallbackType.Modal, new(new InteractionModalCallbackData(
                CustomIDHelpers.CreateModalIDWithState("create-ticket", string.Create(CultureInfo.InvariantCulture, $"{guildId.Value}_{channelId.Value}")),
                translationService["TICKETS_CREATE_TITLE"],
                [
                    new ActionRowComponent([
                        new TextInputComponent("title", TextInputStyle.Short, translationService["TICKETS_CREATE_FIELD_TITLE"], 1, default, true, $"Ticket created {DateTime.UtcNow:s}", default),
                    ]),
                    new ActionRowComponent([
                        new TextInputComponent("reason", TextInputStyle.Paragraph, translationService["TICKETS_CREATE_FIELD_REASON"], 1, default, true, default, default),
                    ]),
                ]))));
    }

    [Modal("create-ticket")]
    [Ephemeral]
    public async Task<Result> CreateTicket(string state, string title, string reason)
    {
        var stateParts = state.Split("_");
        var guildId = ulong.Parse(stateParts[0], CultureInfo.InvariantCulture);
        var channelId = ulong.Parse(stateParts[1], CultureInfo.InvariantCulture);

        if (!interactionCommandContext.TryGetUserID(out var userId))
        {
            throw new InvalidOperationException("Failed to get user ID");
        }

        var user = await userApi.GetUserAsync(userId);

        var thread = await channelApi.StartThreadWithoutMessageAsync(new(channelId), title, ChannelType.PrivateThread,
            new(AutoArchiveDuration.Week), true, reason: $"Ticket created by {user.Entity.DisplayName()}");
        if (!thread.IsSuccess)
        {
            await feedbackService.SendContextualAsync(translationService["TICKETS_CREATE_ERROR"]);
            return Result.Success;
        }

        await channelApi.CreateMessageAsync(thread.Entity.ID, embeds: new([
            new EmbedBuilder()
                .WithAuthor(user.Entity)
                .WithTitle(title)
                .WithDescription(reason)
                .AddField("Date Created", string.Create(new CultureInfo("en"), $"{DateTime.UtcNow:R}")).Entity
                .Build().Entity,
        ]), components: new([
            new ActionRowComponent([
                new ButtonComponent(ButtonComponentStyle.Danger, "Close Ticket", CustomID: CustomIDHelpers.CreateButtonID("ticket-close")),
            ]),
        ]));
        await channelApi.CreateMessageAsync(
            thread.Entity.ID,
            translationService["TICKETS_CREATE_SUCCESS"]);
        await channelApi.AddThreadMemberAsync(thread.Entity.ID, userId);

        await feedbackService.SendContextualAsync(
            translationService["TICKETS_CREATE_SUCCESS_RESPONSE", new
            {
                channel = Mention.Channel(thread.Entity),
            }
        ], options: new()
        {
            MessageFlags = new(MessageFlags.Ephemeral),
        });

        var logsChannel = await dbContext.GuildChannels.FirstOrDefaultAsync(x => x.Id == guildId);
        if (logsChannel?.AlertChannel is null)
        {
            // Too bad, no alerts for you!
            return Result.Success;
        }

        await channelApi.CreateMessageAsync(new(logsChannel.AlertChannel.Value), embeds: new([
            new EmbedBuilder()
                .WithAuthor(user.Entity)
                .WithTitle("Ticket Opened")
                .WithDescription($"A ticket has been opened in {Mention.Channel(thread.Entity)}")
                .Build().Entity
        ]), components: new([
            new ActionRowComponent([
                new ButtonComponent(ButtonComponentStyle.Success, "Join Thread", CustomID: CustomIDHelpers.CreateButtonIDWithState("ticket-join-thread", $"{thread.Entity.ID}")),
            ]),
        ]));

        return Result.Success;
    }

    [Button("ticket-join-thread")]
    [Ephemeral]
    public async Task<Result> JoinThread(string state)
    {
        var threadId = ulong.Parse(state, CultureInfo.InvariantCulture);
        if (!interactionCommandContext.TryGetUserID(out var userId))
        {
            throw new InvalidOperationException("Failed to get user ID");
        }

        await channelApi.AddThreadMemberAsync(new(threadId), userId);

        await feedbackService.SendContextualAsync($"You joined the thread. You can visit the thread here: {Mention.Channel(new Snowflake(threadId))}", options: new()
        {
            MessageFlags = new(MessageFlags.Ephemeral),
        });

        return Result.Success;
    }

    [Button("ticket-close")]
    public async Task<Result> CloseTicket()
    {
        if (!interactionCommandContext.TryGetUserID(out var userId))
        {
            throw new InvalidOperationException("Failed to get user ID");
        }

        if (!interactionCommandContext.TryGetChannelID(out var channelId))
        {
            throw new InvalidOperationException("Failed to get channel ID");
        }

        var user = await userApi.GetUserAsync(userId);

        await feedbackService.SendContextualAsync($":octagonal_sign: The ticket was closed by {user.Entity.DisplayName()}.");

        await channelApi.ModifyThreadChannelAsync(channelId, isLocked: true, isArchived: true, isInvitable: false, reason: $"Ticket closed by {user.Entity.DisplayName()}");
        return Result.Success;
    }
}
