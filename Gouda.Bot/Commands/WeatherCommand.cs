using System.ComponentModel;
using Gouda.Bot.Extensions;
using Gouda.Bot.Services;
using Gouda.Bot.Weather;
using Gouda.Database;
using Gouda.Geocoding;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Discord.Interactivity;
using Remora.Results;
using SkiaSharp;

namespace Gouda.Bot.Commands;

public class WeatherCommand(GeocodingService geocodingService, IFeedbackService feedbackService, TranslationService translationService, WeatherService weatherService) : CommandGroup
{
    [Command("weather")]
    [Description("Retrieve the weather at a place")]
    public async Task<Result> Weather([Description("The user to get the weather for")] IUser? user = null, [Description("The city to get the weather for")] string? city = null)
    {
        var response = await geocodingService.Locate(user, city);
        if (response is null)
        {
            await feedbackService.SendContextualAsync(translationService["TIME_ERROR"]);
            return Result.Success;
        }

        var (geoname, userObject, location) = response;

        var hourlyForecastTask = weatherService.HourlyForecast(location.Latitude, location.Longitude, geoname.Timezone);
        var currentWeatherTask = weatherService.CurrentWeather(location.Latitude, location.Longitude, geoname.Timezone);

        var localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById(geoname.Timezone));

        var hourlyForecast = await hourlyForecastTask;
        var currentWeather = await currentWeatherTask;

        IWeatherPalette palette = currentWeather.WeatherCode switch
        {
            0 or 1 when currentWeather.IsDay == 1 => new DayWeatherPalette(),
            0 or 1 when currentWeather.IsDay == 0 => new NightWeatherPalette(),
            > 1 => new DayWeatherPalette(),
            _ => new DayWeatherPalette(),
        };

        var bitmap = new SKBitmap(700, 500);
        var canvas = new RtlEnabledCanvas(bitmap);
        canvas.Clear(palette.Background);
        canvas.DrawRect(0, 300, 700, 200, new()
        {
            Color = palette.BackgroundAccent,
        });
        canvas.DrawText(geoname.Name, 10, 55, new()
        {
            Color = palette.Foreground,
            Typeface = SKTypeface.FromFamilyName("Contemporary"),
            TextSize = 50,
            IsAntialias = true,
        });
        canvas.DrawText(string.Join(", ", ((string?[])[geoname.Admin1, geoname.Country]).Where(x => x is not null)), 10, 95, new()
        {
            Color = palette.Foreground,
            Typeface = SKTypeface.FromFamilyName("Contemporary"),
            TextSize = 30,
            IsAntialias = true,
        });
        using (var paint = new SKPaint())
        {
            paint.Color = palette.Foreground;
            paint.Typeface = SKTypeface.FromFamilyName("Contemporary");
            paint.TextSize = 80;
            paint.IsAntialias = true;

            var temperature = $"{currentWeather.Temperature2m}\u00b0";
            var temperatureLength = paint.MeasureText(temperature);
            canvas.DrawText(temperature, bitmap.Width - 10 - temperatureLength, 90, paint);
        }

        using (var paint = new SKPaint())
        {
            paint.Color = palette.Foreground;
            paint.Typeface = SKTypeface.FromFamilyName("Contemporary");
            paint.TextSize = 30;
            paint.IsAntialias = true;

            var temperature = translationService["WEATHER_FEELS_LIKE", new
            {
                temperature = $"{currentWeather.Temperature2m}\u00b0",
            }
            ];

            var temperatureLength = paint.MeasureText(temperature);
            canvas.DrawText(temperature, bitmap.Width - 10 - temperatureLength, 140, paint);
        }

        using (var paint = new SKPaint())
        {
            paint.Color = palette.Foreground;
            paint.Typeface = SKTypeface.FromFamilyName("Contemporary");
            paint.TextSize = 30;
            paint.IsAntialias = true;

            foreach (var (forecast, i) in hourlyForecast.SkipWhile(x => x.Time < localTime.Subtract(TimeSpan.FromHours(1))).Take(bitmap.Width / 100).Select((forecast, i) => (forecast, i)))
            {
                var time = $"{forecast.Time:HH:mm}";
                var timeLength = paint.MeasureText(time);
                canvas.DrawText(time, i * 100 + (50 - timeLength / 2), 340, paint);

                var temperature = $"{forecast.Temperature2m}\u00b0";
                var temperatureLength = paint.MeasureText(temperature);
                canvas.DrawText(temperature, i * 100 + (50 - temperatureLength / 2), 480, paint);
            }
        }

        var memoryStream = new MemoryStream();
        bitmap.Encode(new SKManagedWStream(memoryStream, false), SKEncodedImageFormat.Png, 100);
        memoryStream.Seek(0, SeekOrigin.Begin);

        await feedbackService.SendContextualEmbedAsync(
            new EmbedBuilder()
            .WithTitle(userObject is null ? translationService["WEATHER"] : translationService["WEATHER_USER", new
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
            MessageComponents = new([
                new ActionRowComponent([
                    new StringSelectComponent(
                        CustomIDHelpers.CreateSelectMenuID("weather"),
                        [
                            new SelectOption(translationService["WEATHER_CURRENT"], "current"),
                            new SelectOption(translationService["WEATHER_HOURLY"], "hourly", IsDefault: true),
                            new SelectOption(translationService["WEATHER_DAILY"], "daily"),
                        ]),
                ]),
            ]),
        });

        return Result.Success;
    }
}
