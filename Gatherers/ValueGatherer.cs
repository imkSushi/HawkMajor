namespace Gatherers;

public abstract class ValueGatherer<T>
{
    public abstract (T value, bool shortCircuit) Gather(object gatherable, T current);

    public T Gather(object gatherable)
    {
        return Gather(gatherable, Default).value;
    }
    public abstract T Default { get; }
    
    public static T GatherValue<TGatherer>(object gatherable) where TGatherer : ValueGatherer<T>, new()
    {
        return new TGatherer().Gather(gatherable);
    }
    
    public static (T value, bool shortCircuit) GatherValue<TGatherer>(object gatherable, T current) where TGatherer : ValueGatherer<T>, new()
    {
        return new TGatherer().Gather(gatherable, current);
    }
}