using HawkMajor2.Engine.Displays;
using Valiant;

namespace HawkMajor2.Engine;

public class Workspace
{
    public Workspace()
    {
        CurrentScope = BaseScope;
    }
    public Scope BaseScope { get; } = new(null);
    public Scope CurrentScope { get; private set; }
    public Kernel Kernel { get; } = new();
    public Stack<Conjecture> ConjectureStack { get; } = new();
    
    public HashSet<Theorem> GlobalTheorems { get; } = new();
    public HashSet<Strategy> GlobalStrategies { get; } = new();
    public Dictionary<string, Strategy> Strategies { get; } = new();
    public Dictionary<string, Theorem> Theorems { get; } = new();
    
    public void NewScope()
    {
        CurrentScope = new Scope(CurrentScope);
    }
    
    public void EndScope()
    {
        CurrentScope = CurrentScope.Parent ?? throw new InvalidOperationException("Cannot end the base scope.");
    }
    
    public void AddGlobalTheorem(Theorem theorem)
    {
        GlobalTheorems.Add(theorem);
        BaseScope.Theorems.Add(theorem);
    }
    
    public void AddGlobalStrategy(Strategy strategy)
    {
        GlobalStrategies.Add(strategy);
        BaseScope.Strategies.Add(strategy);
        Strategies[strategy.Name] = strategy;
    }

    public void AddGlobalTheorems(HashSet<Theorem> theorems)
    {
        GlobalTheorems.UnionWith(theorems);
        BaseScope.Theorems.UnionWith(theorems);
    }
    
    public void AddGlobalStrategies(HashSet<Strategy> strategies)
    {
        GlobalStrategies.UnionWith(strategies);
        BaseScope.Strategies.UnionWith(strategies);
    }
    
    public void AddLocalStrategy(Strategy strategy)
    {
        CurrentScope.Strategies.Add(strategy);
        Strategies[strategy.Name] = strategy;
    }
    
    public void AddLocalTheorem(Theorem theorem)
    {
        CurrentScope.Theorems.Add(theorem);
    }
    
    public void AddExplicitStrategy(Strategy strategy)
    {
        Strategies[strategy.Name] = strategy;
    }
    
    public void AddFileStrategy(Strategy strategy)
    {
        BaseScope.Strategies.Add(strategy);
        Strategies[strategy.Name] = strategy;
    }
    
    public void AddFileTheorem(Theorem theorem)
    {
        BaseScope.Theorems.Add(theorem);
    }

    public Theorem? Prove(Conjecture conjecture)
    {
        if (ConjectureStack.Contains(conjecture))
            return null;
        
        ConjectureStack.Push(conjecture);
        
        var output = CheckIfInstance(conjecture);
        if (output is not null)
        {
            ConjectureStack.Pop();
            return output;
        }

        foreach (var strategy in CurrentScope.EnumerateStrategies())
        {
            output = strategy.Apply(conjecture, this);
            if (output is not null)
            {
                ConjectureStack.Pop();
                return output;
            }
        }
        
        ConjectureStack.Pop();
        return null;
    }

    public Theorem? CheckIfInstance(Conjecture conjecture)
    {
        return CurrentScope.EnumerateTheorems()
            .Select(theorem => conjecture.CheckIfInstance(theorem, Kernel))
            .FirstOrDefault(output => output is not null);
    }
}