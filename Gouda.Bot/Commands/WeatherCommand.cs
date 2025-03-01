using System.ComponentModel;
using Gouda.Bot.Enum;
using Gouda.Bot.Services;
using Gouda.Geocoding;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Results;

namespace Gouda.Bot.Commands;

public class WeatherCommand(GeocodingService geocodingService, IFeedbackService feedbackService, TranslationService translationService, WeatherService weatherService, IInteractionContext interactionContext) : CommandGroup
{
    [Command("weather")]
    [Description("Retrieve the weather at a place")]
    public async Task<Result> Weather([Description("The user to get the weather for")] IUser? user = null, [Description("The city to get the weather for")] string? city = null)
    {
        try
        {
            if (!interactionContext.TryGetUserID(out var interactionUserId))
            {
                return Result.Success;
            }

            var response = await geocodingService.Locate(user, city);
            if (response is null)
            {
                await feedbackService.SendContextualAsync(translationService["TIME_ERROR"]);
                return Result.Success;
            }

            var (geoname, userObject, location) = response;

            string conversionCountry;
            try
            {
                if (userObject is null)
                {
                    var userLocation = await geocodingService.UserLocation(interactionUserId.Value);
                    var closestCity =
                        await geocodingService.CityInformation(await geocodingService.ClosestCity(userLocation));
                    conversionCountry = closestCity.Country.ToLower();
                }
                else
                {
                    conversionCountry = geoname.Country.ToLower();
                }
            }
            catch (InvalidLocationException)
            {
                conversionCountry = geoname.Country.ToLower();
            }

            var (memoryStream, components) = await weatherService.GenerateWeather(geoname, location, userObject,
                interactionUserId.Value, conversionCountry, WeatherType.Hourly);

            await feedbackService.SendContextualEmbedAsync(
                new EmbedBuilder()
                    .WithTitle(userObject is null
                        ? translationService["WEATHER"]
                        : translationService["WEATHER_USER", new
                            {
                                user = userObject.Username,
                            }
                        ])
                    .WithImageUrl("attachment://weather.png")
                    .WithFooter("Weather data by Open-Meteo.com. I wonder if Victor etc. etc.")
                    .Build().Entity, new()
                {
                    Attachments = new([
                        new FileData("weather.png", memoryStream),
                    ]),
                    MessageComponents = new(components.ToList()),
                });

            return Result.Success;
        }
        catch (InvalidLocationException)
        {
            await feedbackService.SendContextualAsync(translationService["WEATHER_ERROR"]);
            return Result.Success;
        }
    }
}
