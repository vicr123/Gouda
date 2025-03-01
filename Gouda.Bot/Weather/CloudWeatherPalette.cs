using SkiaSharp;

namespace Gouda.Bot.Weather;

public class CloudWeatherPalette : IWeatherPalette
{
    public SKColor Background { get; } = new(200, 200, 200);

    public SKColor BackgroundAccent { get; } = new(170, 170, 170);

    public SKColor Foreground { get; } = new(0, 0, 0);
}
