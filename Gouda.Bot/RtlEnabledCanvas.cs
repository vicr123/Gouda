using System.Globalization;
using SkiaSharp;

namespace Gouda.Bot;

public class RtlEnabledCanvas(SKBitmap bitmap) : SKCanvas(bitmap)
{
    public bool IsRtl { get; set; } = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;

    public new void Clear(SKColor color) => base.Clear(color);

    public new void DrawRect(float x, float y, float width, float height, SKPaint paint) => base.DrawRect(x, y, width, height, paint);

    public new void DrawText(string text, float x, float y, SKPaint paint)
    {
        if (IsRtl)
        {
            var width = paint.MeasureText(text);
            x = bitmap.Width - x - width;
        }

        base.DrawText(text, x, y, paint);
    }
}
