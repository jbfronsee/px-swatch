namespace Lib.Analysis.Interfaces;

public interface IKMeans<T>
{
    public T[] Clusters { get; set; }

    public void Cluster(SimpleColor.Rgb[] pixels);

    public void ClusterParallel(SimpleColor.Rgb[] pixels);
}
