namespace Gatherers;

public abstract class DataGatherer<T> where T : class, new()
{
    public abstract void Gather(object gatherable, T data);

    public T Gather(object gatherable)
    {
        var data = new T();
        Gather(gatherable, data);
        return data;
    }
    
    public static T GatherData<TGatherer>(object gatherable) where TGatherer : DataGatherer<T>, new()
    {
        return new TGatherer().Gather(gatherable);
    }
    
    public static void GatherData<TGatherer>(object gatherable, T data) where TGatherer : DataGatherer<T>, new()
    {
        new TGatherer().Gather(gatherable, data);
    }
}