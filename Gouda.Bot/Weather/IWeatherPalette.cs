using SkiaSharp;

namespace Gouda.Bot.Weather;

public interface IWeatherPalette
{
    public SKColor Background { get; }

    public SKColor BackgroundAccent { get; }

    public SKColor Foreground { get; }
}
