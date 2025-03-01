using System.ComponentModel;
using System.Globalization;
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
using Topten.RichTextKit;

namespace Gouda.Bot.Commands;

public class WeatherCommand(GeocodingService geocodingService, IFeedbackService feedbackService, TranslationService translationService, WeatherService weatherService, WeatherIconService weatherIconService) : CommandGroup
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
            > 1 => new CloudWeatherPalette(),
            _ => new DayWeatherPalette(),
        };

        var temperatureRenderer = new TemperatureRenderer(true);

        // SKFontManager.Default.

        // var font = SKTypeface.FromStream(typeof(WeatherCommand).Assembly.GetManifestResourceStream("Gouda.Bot.Weather.AsapCondensed-Regular.ttf"));
        var font = "Asap Condensed";

        var cityName = new RichString().FontFamily(font).FontSize(50).TextColor(palette.Foreground).Add(geoname.Name);
        var cityDetails = new RichString().FontFamily(font).FontSize(30).TextColor(palette.Foreground)
            .Add(string.Join(", ", ((string?[])[geoname.Admin1, geoname.Country]).Where(x => x is not null)));
        var currentTemp = new RichString().FontFamily(font).FontSize(80).TextColor(palette.Foreground)
            .Add(temperatureRenderer.Render(currentWeather.Temperature2m));
        var feelsLikeTemp = new RichString().FontFamily(font).FontSize(30).TextColor(palette.Foreground)
            .Add(translationService["WEATHER_FEELS_LIKE", new
                {
                    temperature = temperatureRenderer.Render(currentWeather.ApparentTemperature),
                }
            ]);

        using var bitmap = new SKBitmap(700, 500);
        using var canvas = new RtlEnabledCanvas(bitmap);
        canvas.Clear(palette.Background);
        canvas.DrawRect(0, 300, 700, 200, new()
        {
            Color = palette.BackgroundAccent,
        });
        canvas.DrawRichString(cityName, 10, 10);
        canvas.DrawRichString(cityDetails, 10, 10 + cityName.MeasuredHeight + 10);
        canvas.DrawRichString(currentTemp, bitmap.Width - 10 - currentTemp.MeasuredWidth, 10);
        canvas.DrawSvg(weatherIconService.WeatherIcon(currentWeather.WeatherCode!.Value, currentWeather.IsDay == 1), bitmap.Width - 20 - currentTemp.MeasuredWidth - 80, 20, 80, 80);
        canvas.DrawRichString(feelsLikeTemp, bitmap.Width - 10 - feelsLikeTemp.MeasuredWidth, 10 + currentTemp.MeasuredHeight + 10);

        foreach (var (forecast, i) in hourlyForecast.SkipWhile(x => new DateTimeOffset(x.Time!.Value, TimeZoneInfo.FindSystemTimeZoneById(geoname.Timezone).GetUtcOffset(x.Time.Value)) < localTime.Subtract(TimeSpan.FromHours(1)).ToUniversalTime()).Take(bitmap.Width / 100).Select((forecast, i) => (forecast, i)))
        {
            var time = new RichString().FontFamily(font).FontSize(25).TextColor(palette.Foreground)
                .Add($"{forecast.Time:HH:mm}");
            canvas.DrawRichString(time, i * 100 + (50 - time.MeasuredWidth / 2), 310);

            canvas.DrawSvg(weatherIconService.WeatherIcon(forecast.WeatherCode!.Value, forecast.IsDay == 1), i * 100 + (50 - 40), 360, 80, 80);

            var temperature = new RichString().FontFamily(font).FontSize(25).TextColor(palette.Foreground)
                .Add(temperatureRenderer.Render(forecast.Temperature2m));
            canvas.DrawRichString(temperature, i * 100 + (50 - temperature.MeasuredWidth / 2), 490 - temperature.MeasuredHeight);
        }

        if (Random.Shared.NextDouble() < 0.01)
        {
            canvas.DrawBitmap(weatherIconService.Barry, -10, bitmap.Height - 400);
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

    private class TemperatureRenderer(bool isCelsius)
    {
        public string Render(double? temperature)
        {
            return $"{temperature:N1}\u00b0{(isCelsius ? 'C' : 'F')}";
        }
    }
}
