using System.Globalization;
using SkiaSharp;
using SkiaSharp.HarfBuzz;
using Topten.RichTextKit;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace Gouda.Bot;

public class RtlEnabledCanvas(SKBitmap bitmap) : SKCanvas(bitmap)
{
    public bool IsRtl { get; set; } = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;

    public new void Clear(SKColor color) => base.Clear(color);

    public new void DrawRect(float x, float y, float width, float height, SKPaint paint) => base.DrawRect(x, y, width, height, paint);

    public new void DrawText(string text, float x, float y, SKPaint paint)
    {
        using SKShaper s = new(paint.Typeface);
        var rs = new RichString().FontFamily("Asap Condensed").FontSize(paint.TextSize).Add(text);

        if (IsRtl)
        {
            x = bitmap.Width - x - rs.MeasuredWidth;
        }

        rs.Paint(this, new SKPoint(x, y));
        // this.DrawShapedText(s, text, x, y, paint);
    }

    public void DrawRichString(RichString richString, float x, float y)
    {
        if (IsRtl)
        {
            x = bitmap.Width - x - richString.MeasuredWidth;
        }

        richString.Paint(this, new SKPoint(x, y));
    }

    public void DrawSvg(SKSvg svg, float x, float y, float width, float height, SKPaint? paint = null)
    {
        if (IsRtl)
        {
            x = bitmap.Width - x - width;
        }

        Save();
        Translate(x, y);
        Scale(width / svg.CanvasSize.Width, height / svg.CanvasSize.Height);
        DrawPicture(svg.Picture, 0, 0, paint);
        Restore();
    }
}
