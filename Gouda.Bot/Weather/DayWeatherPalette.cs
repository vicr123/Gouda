using SkiaSharp;

namespace Gouda.Bot.Weather;

public class DayWeatherPalette : IWeatherPalette
{
    public SKColor Background { get; } = new(0, 150, 255);

    public SKColor BackgroundAccent { get; } = new(0, 200, 255);

    public SKColor Foreground { get; } = new(0, 0, 0);
}
