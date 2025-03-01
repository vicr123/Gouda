using System.ComponentModel;
using Gouda.Bot.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Gouda.Bot.Commands;

public class PicCommand(IInteractionContext interactionContext, IDiscordRestUserAPI userApi, IFeedbackService feedbackService, PicService picService) : CommandGroup
{
    [Command("pic")]
    [Description("Retrieves the profile picture of a user")]
    public async Task<Result> ProfilePicture([Description("The user to get the profile picture of")] IUser? user = null)
    {
        if (user is null)
        {
            if (!interactionContext.TryGetUserID(out var userId))
            {
                return Result.Success;
            }

            user = (await userApi.GetUserAsync(userId)).Entity;
        }

        var haveGuildId = interactionContext.TryGetGuildID(out var guildId);
        var (embed, components) = await picService.GetPicEmbed(user, haveGuildId ? guildId : null, true);

        await feedbackService.SendContextualEmbedAsync(
            embed,
            new()
            {
                MessageComponents = new(components.ToList()),
            });

        return Result.Success;
    }
}
