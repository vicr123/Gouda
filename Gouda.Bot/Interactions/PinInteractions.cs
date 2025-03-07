using System.Globalization;
using Gouda.Bot.Services;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Interactivity;
using Remora.Results;

namespace Gouda.Bot.Interactions;

public class PinInteractions(IInteractionCommandContext interactionCommandContext, IFeedbackService feedbackService, TranslationService translationService, PinService pinService, IDiscordRestChannelAPI channelApi) : InteractionGroup
{
    [Button("change-pin-page")]
    public async Task<Result> ChangePage(string state)
    {
        if (!interactionCommandContext.TryGetUserID(out var userId))
        {
            throw new InvalidOperationException("Failed to get user ID");
        }

        var originalMessage = interactionCommandContext.Interaction.Message.Value;

        var parts = state.Split('_');
        var page = int.Parse(parts[0], CultureInfo.InvariantCulture);
        var caller = ulong.Parse(parts[1], CultureInfo.InvariantCulture);
        if (caller != userId.Value)
        {
            await feedbackService.SendContextualAsync(translationService["COMMAND_NOT_CALLED"], options: new()
            {
                MessageFlags = new(MessageFlags.Ephemeral),
            });
            return Result.Success;
        }

        var (embed, components) = await pinService.ListPins(userId.Value, page);

        await channelApi.EditMessageAsync(originalMessage.ChannelID, originalMessage.ID, embeds: new([
            embed,
        ]), components: components.ToList());

        return Result.Success;
    }
}