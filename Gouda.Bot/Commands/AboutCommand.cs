using System.ComponentModel;
using Gouda.Bot.Services;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Results;

namespace Gouda.Bot.Commands;

public class AboutCommand(IFeedbackService feedbackService, TranslationService translationService, IOptions<GoudaConfiguration> configuration) : CommandGroup
{
    [Command("about")]
    [Description("Find information about Gouda")]
    public async Task<Result> About()
    {
        await feedbackService.SendContextualEmbedAsync(
            new EmbedBuilder()
                .WithTitle("Gouda (Melk)")
                .WithDescription(translationService["ABOUT_DESCRIPTION"])
                .AddField(
                    translationService["ABOUT_CONFIG_TITLE"],
                    translationService["ABOUT_CONFIG_DESCRIPTION", new
                    {
                        configurationLink = configuration.Value.ConfigurationUrl,
                    }
                    ]).Entity
                .AddField(
                    translationService["ABOUT_FILE_BUG_TITLE"],
                    translationService["ABOUT_FILE_BUG_DESCRIPTION", new
                    {
                        repositoryLink = "https://github.com/vicr123/Gouda/issues",
                    }
                    ]).Entity
                .Build().Entity, new()
                {
                    MessageComponents = new(
                    [
                        new ActionRowComponent(
                            [
                                new ButtonComponent(ButtonComponentStyle.Link, translationService["ABOUT_CONFIG_BUTTON"], URL: configuration.Value.ConfigurationUrl),
                                new ButtonComponent(ButtonComponentStyle.Link, translationService["ABOUT_FILE_BUG_BUTTON"], URL: "https://github.com/vicr123/Gouda/issues"),
                                new ButtonComponent(ButtonComponentStyle.Link, translationService["ABOUT_VIEW_SOURCE_BUTTON"], URL: "https://github.com/vicr123/Gouda"),
                            ]),
                    ]),
                });
        return Result.Success;
    }
}
