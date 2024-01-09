using SkiaSharp;

public static class ImageHelper
{
    public static byte[] ScaleImage(byte[] imageBytes, int maxWidth, int maxHeight)
    {
        SKBitmap image = SKBitmap.Decode(imageBytes);

        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);

        var info = new SKImageInfo(newWidth, newHeight);
        image = image.Resize(info, SKFilterQuality.Low);

        using var ms = new MemoryStream();
        image.Encode(ms, SKEncodedImageFormat.Png, 100);
        return ms.ToArray();
    }

}