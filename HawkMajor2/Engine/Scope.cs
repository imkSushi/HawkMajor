using Valiant;

namespace HawkMajor2.Engine;

public class Scope
{
    public Scope? Parent { get; }
    public HashSet<Theorem> Theorems { get; } = new();
    public HashSet<Strategy> Strategies { get; } = new();
    
    public Scope(Scope? parent)
    {
        Parent = parent;
    }

    public IEnumerable<Theorem> EnumerateTheorems()
    {
        if (Parent is null)
            return Theorems;
        
        return Theorems.Concat(Parent.EnumerateTheorems());
    }
    
    public IEnumerable<Strategy> EnumerateStrategies()
    {
        if (Parent is null)
            return Strategies;
        
        return Strategies.Concat(Parent.EnumerateStrategies());
    }
}