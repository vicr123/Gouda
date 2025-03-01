using System.Globalization;
using Gouda.Bot.Services;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Results;

namespace Gouda.Bot.Interactions;

public class PicInteractions(IDiscordRestUserAPI userApi, IDiscordRestChannelAPI channelApi, PicService picService, IInteractionCommandContext interactionCommandContext) : InteractionGroup
{
    [SelectMenu("pic")]
    [Ephemeral]
    public async Task<Result> ChangeWeatherDisplay(IReadOnlyList<string> values)
    {
        if (values.Count != 1)
        {
            return Result.FromError<string>("Error");
        }

        var originalMessage = interactionCommandContext.Interaction.Message.Value;

        var parts = values[0].Split('_');
        var type = parts[0];
        var userId = ulong.Parse(parts[1], CultureInfo.InvariantCulture);
        var guildId = ulong.Parse(parts[2], CultureInfo.InvariantCulture);

        var userResult = await userApi.GetUserAsync(new(userId));

        var (embed, components) = await picService.GetPicEmbed(userResult.Entity, new(guildId), type == "server");

        await channelApi.EditMessageAsync(originalMessage.ChannelID, originalMessage.ID, embeds: new([embed]),
            components: components.ToList());

        return Result.Success;
    }
}
