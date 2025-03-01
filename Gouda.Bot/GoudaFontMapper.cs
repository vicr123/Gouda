using SkiaSharp;
using Topten.RichTextKit;

namespace Gouda.Bot;

public class GoudaFontMapper : FontMapper
{
    public override SKTypeface TypefaceFromStyle(IStyle style, bool ignoreFontVariants)
    {
        if (style.FontFamily == "Asap Condensed")
        {
            return SKTypeface.FromStream(
                typeof(Program).Assembly.GetManifestResourceStream("Gouda.Bot.Weather.AsapCondensed-Regular.ttf"));
        }

        return base.TypefaceFromStyle(style, ignoreFontVariants);
    }
}
