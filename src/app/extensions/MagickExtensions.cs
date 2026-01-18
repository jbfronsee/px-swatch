using ImageMagick;

namespace App.Extensions;

public static class MagickExtensions
{
    /// <summary>
    /// Gets pixels as byte[].
    /// </summary>
    /// <param name="image">The image to grab pixels from.</param>
    /// <returns></returns>
    /// <exception cref="MagickImageErrorException">Throws when library has an error or when channels < 3</exception>
    public static IEnumerable<byte[]> GetPixelBytes(this IMagickImage<byte> image)
    {
        int channels = (int)image.ChannelCount;
        if (channels < 3)
        {
            throw new MagickImageErrorException("GetPixelBytes() requires RGB channels to be present.");
        }

        IPixelCollection<byte> pixels = image.GetPixels();
        for (int y = 0; y < image.Height; y++)
        {
            byte[] pixelBytes = pixels.GetReadOnlyArea(0, y, image.Width, 1).ToArray();

            yield return pixelBytes;
        }
    }
}
