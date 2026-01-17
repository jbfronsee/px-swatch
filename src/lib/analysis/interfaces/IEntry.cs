namespace Lib.Analysis.Interfaces;

public interface IEntry<T, U> : ICluster<T, U> where U: IEntry<T, U>
{
    public T Bucket { get; set; }
}