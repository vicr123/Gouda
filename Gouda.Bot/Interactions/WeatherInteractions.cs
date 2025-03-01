using Gouda.Bot.Enum;
using Gouda.Bot.Services;
using Gouda.Geocoding;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Discord.Interactivity;
using Remora.Results;

namespace Gouda.Bot.Interactions;

public class WeatherInteractions(IInteractionCommandContext interactionCommandContext, WeatherService weatherService, IDiscordRestChannelAPI channelApi, IDiscordRestUserAPI userApi, IFeedbackService feedbackService, GeocodingService geocodingService, TranslationService translationService) : InteractionGroup
{
    [SelectMenu("weather")]
    [Ephemeral]
    public async Task<Result> ChangeWeatherDisplay(IReadOnlyList<string> values)
    {
        if (values.Count != 1)
        {
            return Result.FromError<string>("Error");
        }

        if (!interactionCommandContext.TryGetUserID(out var userId))
        {
            throw new InvalidOperationException("Failed to get user ID");
        }

        var originalMessage = interactionCommandContext.Interaction.Message.Value;

        var parts = values[0].Split('_');
        var weatherType = parts[0];
        var geonameId = ulong.Parse(parts[1]);
        var latitude = double.Parse(parts[2]);
        var longitude = double.Parse(parts[3]);
        var caller = ulong.Parse(parts[4]);
        var units = parts[5];
        var userString = parts[6];
        if (caller != userId.Value)
        {
            await feedbackService.SendContextualAsync("Sorry, you didn't call this command.", options: new()
            {
                MessageFlags = new(MessageFlags.Ephemeral),
            });
            return Result.Success;
        }

        var geoname = await geocodingService.CityInformation(geonameId);
        var user = userString == "na" ? null : (await userApi.GetUserAsync(new(ulong.Parse(userString)))).Entity;

        var (memoryStream, components) = await weatherService.GenerateWeather(geoname, new()
        {
            Latitude = latitude,
            Longitude = longitude,
        }, user, caller, units, weatherType switch
        {
            "hourly" => WeatherType.Hourly,
            "daily" => WeatherType.Daily,
            _ => WeatherType.Hourly,
        });

        await channelApi.EditMessageAsync(originalMessage.ChannelID, originalMessage.ID, embeds: new([
            new EmbedBuilder()
                .WithTitle(user is null
                    ? translationService["WEATHER"]
                    : translationService["WEATHER_USER", new
                        {
                            user = user.Username,
                        }
                    ])
                .WithImageUrl("attachment://weather.png")
                .WithFooter("Weather data by Open-Meteo.com. I wonder if Victor etc. etc.")
                .Build().Entity,
        ]), components: new(components.ToList()), attachments: new([
            new FileData("weather.png", memoryStream),
        ]));

        return Result.Success;
    }
}
