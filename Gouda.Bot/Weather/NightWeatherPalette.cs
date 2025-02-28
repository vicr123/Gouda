using SkiaSharp;

namespace Gouda.Bot.Weather;

public class NightWeatherPalette : IWeatherPalette
{
    public SKColor Background { get; } = new(0, 25, 80);

    public SKColor BackgroundAccent { get; } = new(0, 10, 35);

    public SKColor Foreground { get; } = new(255, 255, 255);
}
