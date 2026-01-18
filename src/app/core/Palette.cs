using ImageMagick;

using SimpleColor = Lib.SimpleColor;
using App.Extensions;
using Lib.Analysis;
using Lib.Colors;

namespace App.Core;

public static class Palette
{
    /// <summary>
    /// Creates Palette from pixels using Histogram
    /// </summary>
    /// 
    /// <param name="pixels">The pixels of the image in HSV space.</param>
    /// <returns>The palette as a list of MagickColors ordered by Hue then Saturation then Value.</returns>
    public static HistogramLab CalculateHistogramFromPixels(SimpleColor.Rgb[] pixels, Dictionary<SimpleColor.Rgb, PackedLab> colormap, Tolerances tolerances, Buckets buckets)
    {
        HistogramLab histogram = new(colormap);

        int j = 0;
        foreach (var bucket in buckets.PaletteRgb())
        {
            VectorLab labBucket = Colors.Convert.ToLab(bucket);
            EntryLab entry = new(labBucket, labBucket, 0);
            histogram.Results[j] = entry;
            j++;
        }

        histogram.Cluster(pixels);

        return histogram;
    }


    public static HistogramLab CalculateHistogram(IMagickImage<byte> image, Tolerances tolerances, Buckets buckets)
    {
        SimpleColor.Rgb[] pixels = new SimpleColor.Rgb[image.Width * image.Height];
        
        Dictionary<SimpleColor.Rgb, PackedLab> colormap = [];
        int i = 0;
        foreach(var pixelBytes in image.GetPixelBytes())
        {
            for (uint j = 0; j < pixelBytes.Length; j += image.ChannelCount)
            {
                SimpleColor.Rgb pixel = new(pixelBytes[j], pixelBytes[j + 1], pixelBytes[j + 2]);
                if (!colormap.ContainsKey(pixel))
                {
                    colormap[pixel] = Colors.Convert.ToLab(pixel).Pack();
                }

                pixels[i] = pixel;
                i++;
            }
        }
        
        return CalculateHistogramFromPixels(pixels, colormap, tolerances, buckets);
    }

    /// <summary>
    /// Generates a palette using a Histogram.
    /// </summary>
    /// <param name="image">The image to generate from.</param>
    /// <param name="tolerances">The tolerances that represent threshold for histogram to find a match.</param>
    /// <exception cref="MagickException">Thrown when an error is raised by ImageMagick.</exception>
    /// <returns>The palette as a list of MagickColors ordered by Hue then Saturation then Value.</returns>
    public static HistogramLab CalculateHistogramFromSample(MagickImage image, Tolerances tolerances, Buckets buckets)
    {
        double largePixels = 640 * 480;
        double imageLength = image.Width * image.Height;

        if (imageLength > largePixels)
        {
            using var sample = image.Clone();
            sample.Sample(new Percentage(100 / Math.Sqrt(imageLength / largePixels)));
            return CalculateHistogram(sample, tolerances, buckets);
        }
        
        return CalculateHistogram(image, tolerances, buckets);
    }

    /// <summary>
    /// Generates a palette using K-Means Clustering from an array of LAB space pixels.
    /// </summary>
    /// <param name="pixels">The pixels of the image in LAB space.</param>
    /// <param name="seeds">The seed values to make initial clusters from.</param>
    /// <param name="verbose">Flag that enables printing K-Means progress message.</param>
    /// <returns>The palette as a list of MagickColors ordered by Hue then Saturation then Value.</returns>
    public static List<IMagickColor<byte>> FromPixelsKmeans(SimpleColor.Rgb[] pixels, List<IMagickColor<byte>> seeds, Dictionary<SimpleColor.Rgb, PackedLab> colormap, bool verbose = false)
    {        
        int maxIterations = 32;
        KMeansLab kmeans = new(seeds.Select(Colors.Convert.ToLab).Select(c => new ClusterLab(c, c, 0)).ToArray(), colormap);

        if (verbose)
        {
            Console.WriteLine($"K-Means Cluster Index: ");
        }

        bool finished = false;
        for (int i = 0; (i < maxIterations) && !finished; i++)
        {
            if (verbose)
            {
                Console.WriteLine(i);
            }

            kmeans.ClusterParallel(pixels);
    
            finished = true;
            foreach (var cluster in kmeans.Clusters)
            {
                finished = finished && (ColorMath.CalculateDistance(cluster.Cluster, cluster.Mean) <= 1.0);

                
                cluster.Cluster = cluster.Mean;
            }
        }

        List<SimpleColor.Hsv> palette = kmeans.Clusters.Select(c => Colors.Convert.ToHsv(c.Mean)).ToList();
        palette.Sort();
        
        return palette.Select(Colors.Convert.ToMagickColor).ToList();
    }

    /// <summary>
    /// Generates a palette using K-Means Clustering.
    /// </summary>
    /// <param name="image">The image to generate from.</param>
    /// <param name="seeds">The seed values to make initial clusters from.</param>
    /// <param name="verbose">Flag that enables printing K-Means progress message.</param>
    /// <exception cref="MagickException">Thrown when an error is raised by ImageMagick.</exception>
    /// <returns>The palette as a list of MagickColors ordered by Hue then Saturation then Value.</returns>
    public static List<IMagickColor<byte>> FromImageKmeans(MagickImage image, List<IMagickColor<byte>> seeds, Dictionary<SimpleColor.Rgb, PackedLab>? colormap = null,  bool verbose = false)
    {
        if (colormap is null)
        {
            colormap = [];
        }

        // pixels could be extremely large if the image is 4K or higher. But it only has 3 bytes each.
        SimpleColor.Rgb[] pixels = new SimpleColor.Rgb[image.Width * image.Height];

        int i = 0;
        foreach(var pixelBytes in image.GetPixelBytes())
        {
            for (uint j = 0; j < pixelBytes.Length; j += image.ChannelCount)
            {
                SimpleColor.Rgb pixel = new(pixelBytes[j], pixelBytes[j + 1], pixelBytes[j + 2]);
                if (!colormap.ContainsKey(pixel))
                {
                    colormap[pixel] = Colors.Convert.ToLab(pixel).Pack();
                }

                pixels[i] = pixel;
                i++;
            }
        }

        return FromPixelsKmeans(pixels, seeds, colormap, verbose);
    }
}