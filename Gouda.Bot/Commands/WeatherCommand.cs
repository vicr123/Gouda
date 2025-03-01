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
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Discord.Interactivity;
using Remora.Results;
using SkiaSharp;
using Topten.RichTextKit;

namespace Gouda.Bot.Commands;

public class WeatherCommand(GeocodingService geocodingService, IFeedbackService feedbackService, TranslationService translationService, WeatherService weatherService, WeatherIconService weatherIconService, IInteractionContext interactionContext) : CommandGroup
{
    [Command("weather")]
    [Description("Retrieve the weather at a place")]
    public async Task<Result> Weather([Description("The user to get the weather for")] IUser? user = null, [Description("The city to get the weather for")] string? city = null)
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
                var closestCity = await geocodingService.CityInformation(await geocodingService.ClosestCity(userLocation));
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

        var hourlyForecastTask = weatherService.HourlyForecast(location.Latitude, location.Longitude, geoname.Timezone);
        var dailyForecastTask = weatherService.DailyForecast(location.Latitude, location.Longitude, geoname.Timezone);
        var currentWeatherTask = weatherService.CurrentWeather(location.Latitude, location.Longitude, geoname.Timezone);

        var localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById(geoname.Timezone));

        var hourlyForecast = await hourlyForecastTask;
        var dailyForecast = await dailyForecastTask;
        var currentWeather = await currentWeatherTask;

        IWeatherPalette palette = currentWeather.WeatherCode switch
        {
            0 or 1 when currentWeather.IsDay == 1 => new DayWeatherPalette(),
            0 or 1 when currentWeather.IsDay == 0 => new NightWeatherPalette(),
            > 1 => new CloudWeatherPalette(),
            _ => new DayWeatherPalette(),
        };

        var unitConverter = new UnitConverter(conversionCountry);

        const string font = "Asap Condensed";

        var cityName = new RichString().FontFamily(font).FontSize(50).TextColor(palette.Foreground).Add(geoname.Name);
        var cityDetails = new RichString().FontFamily(font).FontSize(30).TextColor(palette.Foreground)
            .Add(string.Join(", ", ((string?[])[geoname.Admin1, geoname.Country]).Where(x => x is not null)));
        var currentTemp = new RichString().FontFamily(font).FontSize(80).TextColor(palette.Foreground)
            .Add(unitConverter.Temperature(currentWeather.Temperature2m));
        var feelsLikeTemp = new RichString().FontFamily(font).FontSize(30).TextColor(palette.Foreground)
            .Add($"{translationService[currentWeather.WeatherCode switch {
                0 => "WEATHER_CONDITION_CLEAR",
                1 => "WEATHER_CONDITION_CLEAR",
                2 => "WEATHER_CONDITION_PARTLY_CLOUDY",
                3 => "WEATHER_CONDITION_OVERCAST",
                45 => "WEATHER_CONDITION_FOGGY",
                48 => "WEATHER_CONDITION_FOGGY",
                51 => "WEATHER_CONDITION_DRIZZLE_LIGHT",
                53 => "WEATHER_CONDITION_DRIZZLE_MODERATE",
                55 => "WEATHER_CONDITION_DRIZZLE_HEAVY",
                56 => "WEATHER_CONDITION_DRIZZLE_FREEZING_LIGHT",
                57 => "WEATHER_CONDITION_DRIZZLE_FREEZING_DENSE",
                61 => "WEATHER_CONDITION_RAIN_LIGHT",
                63 => "WEATHER_CONDITION_RAIN_MODERATE",
                65 => "WEATHER_CONDITION_RAIN_HEAVY",
                66 => "WEATHER_CONDITION_RAIN_FREEZING_LIGHT",
                67 => "WEATHER_CONDITION_RAIN_FREEZING_HEAVY",
                71 => "WEATHER_CONDITION_SNOW_LIGHT",
                73 => "WEATHER_CONDITION_SNOW_MODERATE",
                75 => "WEATHER_CONDITION_SNOW_HEAVY",
                77 => "WEATHER_CONDITION_SNOW_GRAINS",
                80 => "WEATHER_CONDITION_RAIN_SHOWERS_LIGHT",
                81 => "WEATHER_CONDITION_RAIN_SHOWERS_MODERATE",
                82 => "WEATHER_CONDITION_RAIN_SHOWERS_HEAVY",
                85 => "WEATHER_CONDITION_SNOW_SHOWERS_LIGHT",
                86 => "WEATHER_CONDITION_SNOW_SHOWERS_HEAVY",
                95 => "WEATHER_CONDITION_THUNDERSTORM",
                96 => "WEATHER_CONDITION_HAIL_LIGHT",
                97 => "WEATHER_CONDITION_HAIL_HEAVY",
                _ => "WEATHER_CONDITION_UNKNOWN",
            }]} • {translationService["WEATHER_FEELS_LIKE", new
                {
                    temperature = unitConverter.Temperature(currentWeather.ApparentTemperature),
                }
            ]}");

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

        var windSpeed = new RichString().FontFamily(font).FontSize(30).TextColor(palette.Foreground).Add(unitConverter.Speed(currentWeather.WindSpeed10m));
        canvas.DrawSvg(weatherIconService.Wind, 10, 200, 32, 32, true);
        canvas.DrawRichString(windSpeed, 52, 200);

        var windDirection = new RichString().FontFamily(font).FontSize(30).TextColor(palette.Foreground).Add($"{currentWeather.WindDirection10m}\u00b0");
        canvas.DrawSvg(weatherIconService.Compass, 200, 200, 32, 32);
        canvas.DrawRichString(windDirection, 242, 200);

        var humidity = new RichString().FontFamily(font).FontSize(30).TextColor(palette.Foreground).Add($"{currentWeather.RelativeHumidity2m}%");
        canvas.DrawSvg(weatherIconService.Humidity, 300, 200, 32, 32);
        canvas.DrawRichString(humidity, 342, 200);

        var pressure = new RichString().FontFamily(font).FontSize(30).TextColor(palette.Foreground).Add(unitConverter.Pressure(currentWeather.PressureMsl));
        canvas.DrawSvg(weatherIconService.Pressure, 400, 200, 32, 32, true);
        canvas.DrawRichString(pressure, 442, 200);

        foreach (var (forecast, i) in hourlyForecast.SkipWhile(x => new DateTimeOffset(x.Time!.Value, TimeZoneInfo.FindSystemTimeZoneById(geoname.Timezone).GetUtcOffset(x.Time.Value)) < localTime.Subtract(TimeSpan.FromHours(1)).ToUniversalTime()).Take(bitmap.Width / 100).Select((forecast, i) => (forecast, i)))
        {
            var time = new RichString().FontFamily(font).FontSize(25).TextColor(palette.Foreground)
                .Add($"{forecast.Time:HH:mm}");
            canvas.DrawRichString(time, i * 100 + (50 - time.MeasuredWidth / 2), 310);

            var temperature = new RichString().FontFamily(font).FontSize(25).TextColor(palette.Foreground)
                .Add(unitConverter.Temperature(forecast.Temperature2m));
            canvas.DrawRichString(temperature, i * 100 + (50 - temperature.MeasuredWidth / 2), 490 - temperature.MeasuredHeight);

            if (forecast.PrecipitationProbability!.Value > 0)
            {
                var precipitation = new RichString().FontFamily(font).FontSize(25).TextColor(palette.Foreground).Add($"{forecast.PrecipitationProbability}%");
                var fullWidth = precipitation.MeasuredWidth + 32;
                canvas.DrawSvg(weatherIconService.Precipitation, i * 100 + (50 - fullWidth / 2), 490 - temperature.MeasuredHeight - 10 - precipitation.MeasuredHeight, 32, 32);
                canvas.DrawRichString(precipitation, i * 100 + (50 - fullWidth / 2) + 32, 490 - temperature.MeasuredHeight - 10 - precipitation.MeasuredHeight);

                canvas.DrawSvg(weatherIconService.WeatherIcon(forecast.WeatherCode!.Value, forecast.IsDay == 1), i * 100 + (50 - 30), 340, 60, 60);
            }
            else
            {
                canvas.DrawSvg(weatherIconService.WeatherIcon(forecast.WeatherCode!.Value, forecast.IsDay == 1), i * 100 + (50 - 40), 360, 80, 80);
            }
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

    private class UnitConverter(string country)
    {
        public string Temperature(double? temperature)
        {
            if (country == "us")
            {
                return $"{temperature * 9 / 5 + 32:N0}\u00b0F";
            }

            return $"{temperature:N0}\u00b0C";
        }

        public string Speed(double? speed)
        {
            if (country is "us" or "gb")
            {
                return $"{speed * 0.6214:N1} mph";
            }

            return $"{speed:N1} km/h";
        }

        public string Pressure(double? pressure)
        {
            if (country == "us")
            {
                return $"{pressure * 0.029529983071445:N2} inHg";
            }

            return $"{pressure:N0} hPa";
        }
    }
}
