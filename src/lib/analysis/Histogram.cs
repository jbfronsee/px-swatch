using Lib.Colors;

namespace Lib.Analysis;

public class Histogram : KMeans<Histogram.Entry, PackedLab, VectorLab>
{
    private const int BucketCount = 256;

    public sealed record Entry : SafeClusterLab<Entry>
    {
        public Entry(VectorLab cluster, VectorLab mean, int count) : base(cluster, mean, count) { }
        
        public VectorLab Bucket { get => Cluster; set => Cluster = value; }

        public override Entry ParallelSafeCopy()
        {
            return new Entry(Cluster, Mean, Count);
        }
    }

    public override Entry[] Clusters { get; set; } = new Entry[BucketCount];

    public Entry[] Results => Clusters;

    public int TotalPixelsCounted { get; set; } = 0;

    public override void Cluster(SimpleColor.Rgb[] pixels, Dictionary<SimpleColor.Rgb, PackedLab> colormap)
    {
        TotalPixelsCounted = pixels.Length;
        base.Cluster(pixels, colormap);
    }

    public override void ClusterParallel(SimpleColor.Rgb[] pixels, Dictionary<SimpleColor.Rgb, PackedLab>? colormap = null)
    {
        TotalPixelsCounted = pixels.Length;
        base.ClusterParallel(pixels, colormap);
    }

    private List<Entry> Palette(Entry[] entries)
    {
        List<Entry> matches = [];
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

    public List<Entry> Palette()
    {
        return Palette(Results);
    }

    public List<Entry> PaletteWithFilter()
    {
        Entry[] largeValues = Results.Where(e => e.Count > (0 + 1e-5)).ToArray();
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
}