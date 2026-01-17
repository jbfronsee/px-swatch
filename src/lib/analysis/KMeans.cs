using System.Collections.Concurrent;

using Lib.Colors;
using Lib.Colors.Interfaces;
using Lib.SimpleColor;
using Lib.Analysis.Interfaces;

namespace Lib.Analysis;

public abstract class KMeans<T, U> : IKMeans<T>
    where T: class, ICluster<U, T>
    where U: IColorVector<double>
{
    public virtual T[] Clusters { get; set; } = [];

    protected virtual int UpdateBestCluster(T[] clusters, SimpleColor.Rgb pixel, Dictionary<SimpleColor.Rgb, int> memoizedClusters)
    {
        U color = UnpackPixel(pixel);

        int bestClusterIndex = 0;
        if (!memoizedClusters.TryGetValue(pixel, out bestClusterIndex))
        {
            double bestDistance = double.MaxValue;
            for (int i = 0; i < clusters.Length; i++)
            {
                T cluster = clusters[i];
                U value = cluster.Cluster;
                double distance = ColorMath.CalculateDistanceSquared(value, color);
                if (distance < bestDistance)
                {
                    bestClusterIndex = i;
                    bestDistance = distance;
                }
            }

            memoizedClusters[pixel] = bestClusterIndex;
        }

        T bestCluster = clusters[bestClusterIndex];
        bestCluster.Mean = ColorMath.CalculateNewMean(bestCluster.Mean, bestCluster.Count, color);
        bestCluster.Count++;

        return bestClusterIndex;
    }

    protected abstract U UnpackPixel(SimpleColor.Rgb pixel);

    public virtual void Cluster(SimpleColor.Rgb[] pixels)
    {
        Dictionary<SimpleColor.Rgb, int> memoizedClusters = [];

        foreach(var pixel in pixels)
        {
            UpdateBestCluster(Clusters, pixel, memoizedClusters);
           // Console.WriteLine($"{bestClusterIndex} Count: {bestCluster.Count}");
        }

        // foreach(var cluster in clusters.Where(c => c.Count > 0))
        // {
        //     Console.WriteLine(cluster);
        // }
    }

    public virtual void ClusterParallel(SimpleColor.Rgb[] pixels)
    {
        //Console.WriteLine("still running this");
        ConcurrentBag<T[]> bag = [];
        Parallel.ForEach(
            pixels,
            () => (new Dictionary<SimpleColor.Rgb, int>(), Clusters.Select(c => c.ParallelSafeCopy()).ToArray()), 
            (pixel, _, threadLocals) =>
            {
                var (memoizedCluster, means) = threadLocals;
                UpdateBestCluster(means, pixel, memoizedCluster);
                return (memoizedCluster, means);
            }, 
            threadLocals =>
            {
                var (_, means) = threadLocals;
                bag.Add(means);
            }
        );


        foreach(var threadMeans in bag)
        {
            Clusters = Clusters.Zip(threadMeans).Select(z => 
                {
                    var (total, threadResults) = z;
                    total.Mean = ColorMath.AddMeans(total.Mean, total.Count, threadResults.Mean, threadResults.Count);
                    total.Count += threadResults.Count;
                    return total;
                }
            ).ToArray();
        }
    }
}

public class KMeansLab : KMeans<ClusterLab, VectorLab>
{
    public Dictionary<SimpleColor.Rgb, Lib.Colors.PackedLab> Colormap { get; set; } = [];

    public KMeansLab() {}

    public KMeansLab(ClusterLab[] clusters, Dictionary<Rgb, Colors.PackedLab> colormap)
    {
        Clusters = clusters;
        Colormap = colormap;
    }

    protected override VectorLab UnpackPixel(Rgb pixel)
    {
        return Colormap[pixel].Unpack();
    }
}
