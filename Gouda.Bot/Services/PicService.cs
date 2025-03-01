using System.Globalization;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Extensions.Embeds;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;

namespace Gouda.Bot.Services;

public class PicService(IDiscordRestGuildAPI guildApi, TranslationService translationService)
{
    public async Task<(Embed Embed, IReadOnlyList<IMessageComponent> Components)> GetPicEmbed(IUser user, Snowflake? guildId, bool preferServerPicture)
    {
        IGuildMember? guildMember = null;
        if (guildId.HasValue)
        {
            var guildMemberResult = await guildApi.GetGuildMemberAsync(guildId.Value, user.ID);
            if (guildMemberResult.IsSuccess)
            {
                guildMember = guildMemberResult.Entity;
            }
        }

        var baseAvatarUrl = $"https://cdn.discordapp.com/embed/avatars/{(user.ID.Value >> 22) % 6}.png";
        var isServerProfilePicture = false;
        if (user.Avatar is not null)
        {
            baseAvatarUrl = $"https://cdn.discordapp.com/avatars/{user.ID.Value}/{user.Avatar.Value}.{(user.Avatar.HasGif ? "gif" : "png")}";
        }

        if (guildMember?.Avatar is { HasValue: true, Value: not null } && preferServerPicture)
        {
            baseAvatarUrl = $"https://cdn.discordapp.com/guilds/{guildId!.Value}/users/{user.ID.Value}/avatars/{guildMember.Avatar.Value.Value}.{(guildMember.Avatar.Value!.HasGif ? "gif" : "png")}";
            isServerProfilePicture = true;
        }

        return (
            new EmbedBuilder()
                .WithTitle(isServerProfilePicture
                    ? translationService["PROFILE_PICTURE_SERVER"]
                    : translationService["PROFILE_PICTURE"])
                .WithAuthor(user.Username, iconUrl: $"{baseAvatarUrl}?size=128")
                .WithImageUrl($"{baseAvatarUrl}?size=2048")
                .Build().Entity,
            guildMember?.Avatar is { HasValue: true, Value: not null } ? [
                new ActionRowComponent([
                    new StringSelectComponent(
                        CustomIDHelpers.CreateSelectMenuID("pic"),
                        [
                            new SelectOption(
                                translationService["PROFILE_PICTURE_SERVER"],
                                string.Create(CultureInfo.InvariantCulture, $"server_{user.ID.Value}_{guildId!.Value}"),
                                IsDefault: isServerProfilePicture),
                            new SelectOption(
                                translationService["PROFILE_PICTURE"],
                                string.Create(
                                    CultureInfo.InvariantCulture,
                                    $"universal_{user.ID.Value}_{guildId.Value}"), IsDefault: !isServerProfilePicture),
                        ]),
                ]),
            ] : []);
    }
}
