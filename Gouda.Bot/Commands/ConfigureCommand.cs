using System.ComponentModel;
using Gouda.Bot.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Results;

namespace Gouda.Bot.Commands;

public class ConfigureCommand(IFeedbackService feedbackService, TranslationService translationService) : CommandGroup
{
    [Command("Configure")]
    [Description("Make changes to settings")]
    public async Task<Result> Configure()
    {
#pragma warning disable SA1500
        await feedbackService.SendContextualEmbedAsync(
            new EmbedBuilder()
                .WithTitle(translationService["CONFIGURE_TITLE"])
                .WithDescription(translationService["CONFIG_RESPONSE", new
                {
                    botName = "Gouda",
                    configurationLink = "https://localhost:1234",
                }])
                .Build().Entity,
            new()
            {
                MessageComponents = new(
                [
                    new ActionRowComponent(
                    [
                        new ButtonComponent(ButtonComponentStyle.Link, translationService["ABOUT_CONFIG_BUTTON"], URL: "https://example.com"),
                    ]),
                ]),
            });
#pragma warning restore SA1500
        return Result.Success;
    }
}
