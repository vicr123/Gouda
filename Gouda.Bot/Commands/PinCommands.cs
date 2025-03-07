using System.ComponentModel;
using Gouda.Bot.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Gouda.Bot.Commands;

public class PinCommands(PinService pinService, IFeedbackService feedbackService, IInteractionCommandContext interactionContext) : CommandGroup
{
    [Command("pins")]
    [Description("View pins on your portable pinboard")]
    public async Task<Result> ViewPins(int? pinNumber = null)
    {
        if (!interactionContext.TryGetUserID(out var interactionUserId))
        {
            return Result.Success;
        }

        if (pinNumber.HasValue)
        {
            await feedbackService.SendContextualAsync("TODO");
            return Result.Success;
        }

        var (embed, components) = await pinService.ListPins(interactionUserId.Value, 0);

        await feedbackService.SendContextualEmbedAsync(embed, new()
        {
            MessageComponents = new(components),
        });

        return Result.Success;
    }
}