using Lib.Colors;
using Lib.Colors.Interfaces;
using Lib.SimpleColor;
using Lib.Analysis.Interfaces;

namespace Lib.Analysis;

public abstract class Histogram<T, U>: KMeans<T, U>, IHistogram<T>
    where T: class, IEntry<U, T>
    where U: IColorVector<double>
{
    private const int BucketCount = 256;

    public override T[] Clusters { get; set; } = new T[BucketCount];

    public T[] Results => Clusters;

    public int TotalPixelsCounted { get; set; } = 0;

    public override void Cluster(SimpleColor.Rgb[] pixels)
    {
        TotalPixelsCounted = pixels.Length;
        base.Cluster(pixels);
    }

    public override void ClusterParallel(SimpleColor.Rgb[] pixels)
    {
        TotalPixelsCounted = pixels.Length;
        base.ClusterParallel(pixels);
    }

    private List<T> Palette(T[] entries)
    {
        List<T> matches = [];
        for (int i = 0, j = 1, k = 2; k < entries.Length; i++, j++, k++)
        {
            int prev = entries[i].Count;
            int curr = entries[j].Count;
            int next = entries[k].Count;

            if (prev < curr && curr > next)
            {
                matches.Add(entries[j]);
            }
        }

        return matches;
    }

    public List<T> Palette()
    {
        return Palette(Results);
    }

    public List<T> PaletteWithFilter()
    {
        T[] largeValues = Results.Where(e => e.Count > (0 + 1e-5)).ToArray();
        return Palette(largeValues);


        //var maxes = Results.OrderByDescending(h => h.Count).Take(8).Select(c => c.Mean);
        //return Results.Where(h => h.Count > 0).OrderBy(h => h.Count).Take(16).Select(c => c.Mean);

        // double minDistance = double.MaxValue;
        // double maxDistance = double.MinValue;
        // foreach (var min in mins)
        // {
        //     foreach (var max in maxes)
        //     {        
        //         double distance = ColorMath.CalculateDistance(min.Mean, max.Mean);


        //     }
        // }
    }

    public override string ToString()
    {
        string result = $"Total Pixels: {TotalPixelsCounted}";
        foreach (var entry in Results)
        {
            result += $"{entry.Count}\n";
        }

        return result;
    }
    // {
    //     return Colormap[pixel].Unpack();
    // }
}


public class HistogramLab : Histogram<EntryLab, VectorLab>
{
    public Dictionary<SimpleColor.Rgb, Lib.Colors.PackedLab> Colormap { get; set; } = [];

    public HistogramLab() {}

    public HistogramLab(Dictionary<Rgb, Colors.PackedLab> colormap)
    {
        Colormap = colormap;
    }

    protected override VectorLab UnpackPixel(Rgb pixel)
    {
        return Colormap[pixel].Unpack();
    }
}
