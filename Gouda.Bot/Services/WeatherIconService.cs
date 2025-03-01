using SkiaSharp;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace Gouda.Bot.Services;

public class WeatherIconService
{
    public WeatherIconService()
    {
        var thisAssembly = typeof(WeatherIconService).Assembly;
        using var barryStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.barry.png");
        using var sunnyStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.sunny.svg");
        using var moonyStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.moony.svg");
        using var cloudyStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.cloudy.svg");
        using var foggyStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.fog.svg");
        using var rainyStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.rain.svg");
        using var snowyRainyStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.rainsnow.svg");
        using var snowyStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.snow.svg");
        using var thunderStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.thunder.svg");
        using var windStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.wind.svg");
        using var pressureStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.pressure.svg");
        using var sunriseStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.sunrise.svg");
        using var sunsetStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.sunset.svg");
        using var compassStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.compass.svg");
        using var humidityStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.humidity.svg");
        using var precipitationStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.precipitation.svg");
        using var lowTempStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.low-temp.svg");
        using var highTempStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.high-temp.svg");
        using var unavailStream = thisAssembly.GetManifestResourceStream("Gouda.Bot.Weather.Images.unavail.svg");

        Barry = SKBitmap.Decode(barryStream);
        Sunny.Load(sunnyStream);
        Moony.Load(moonyStream);
        Cloudy.Load(cloudyStream);
        Foggy.Load(foggyStream);
        Rainy.Load(snowyRainyStream);
        Snowy.Load(snowyStream);
        Thunder.Load(thunderStream);
        Wind.Load(windStream);
        Pressure.Load(pressureStream);
        Sunrise.Load(sunriseStream);
        Sunset.Load(sunsetStream);
        Compass.Load(compassStream);
        Humidity.Load(humidityStream);
        Precipitation.Load(precipitationStream);
        LowTemp.Load(lowTempStream);
        HighTemp.Load(highTempStream);
        Unavailable.Load(unavailStream);
    }

    public SKBitmap Barry { get; }

    public SKSvg Sunny { get; } = new();

    public SKSvg Moony { get; } = new();

    public SKSvg Cloudy { get; } = new();

    public SKSvg Foggy { get; } = new();

    public SKSvg Rainy { get; } = new();

    public SKSvg SnowyRainy { get; } = new();

    public SKSvg Snowy { get; } = new();

    public SKSvg Thunder { get; } = new();

    public SKSvg Wind { get; } = new();

    public SKSvg Pressure { get; } = new();

    public SKSvg Sunrise { get; } = new();

    public SKSvg Sunset { get; } = new();

    public SKSvg Humidity { get; } = new();

    public SKSvg Compass { get; } = new();

    public SKSvg Precipitation { get; } = new();

    public SKSvg LowTemp { get; } = new();

    public SKSvg HighTemp { get; } = new();

    public SKSvg Unavailable { get; } = new();

    public SKSvg WeatherIcon(int weatherCode, bool isDay)
    {
        return weatherCode switch
        {
            0 or 1 when isDay => Sunny,
            0 or 1 when !isDay => Moony,
            2 or 3 => Cloudy,
            45 or 48 => Foggy,
            51 or 53 or 55 or 61 or 63 or 65 or 80 or 81 or 82 => Rainy,
            56 or 57 or 66 or 67 or 85 or 86 => SnowyRainy,
            71 or 73 or 75 or 77 => Snowy,
            95 or 96 or 99 => Thunder,
            _ => Sunny,
        };
    }
}
