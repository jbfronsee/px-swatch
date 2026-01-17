using Lib.Colors;
using Lib.Analysis.Interfaces;

namespace Lib.Analysis;

public sealed record EntryLab : SafeClusterLab<EntryLab>, IEntry<VectorLab, EntryLab>
{
    public EntryLab(VectorLab cluster, VectorLab mean, int count) : base(cluster, mean, count) { }
    
    public VectorLab Bucket { get => Cluster; set => Cluster = value; }

    public override EntryLab ParallelSafeCopy()
    {
        return new EntryLab(Cluster, Mean, Count);
    }
}