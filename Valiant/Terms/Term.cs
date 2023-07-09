using Printing;
using Results;
using Valiant.Gatherers;
using Valiant.Types;

namespace Valiant.Terms;

public abstract record Term : IPrintable
{
    internal Term()
    {
        
    }
    
    public abstract Type TypeOf();

    internal abstract Term BindVariable(Free? variable, int index);

    public abstract bool IsFree(Free variable);

    public Result<Term> Instantiate(Dictionary<Free, Term> map)
    {
        foreach (var (free, value) in map)
        {
            if (free.Type != value.TypeOf())
                return $"Type of {value} does not match type of {free}.";
        }
        
        return SafeInstantiate(map);
    }

    internal abstract Term SafeInstantiate(Dictionary<Free, Term> map);
    
    public abstract Term Instantiate(Dictionary<TyVar, Type> map);

    public void FreesIn(HashSet<Free> frees)
    {
        frees.UnionWith(FreesInGatherer.GatherData<FreesInGatherer>(this));
    }

    public abstract bool ContainsFrees();

    public void FreeTypesIn(HashSet<TyVar> frees)
    {
        frees.UnionWith(FreeTypesInGatherer.GatherData<FreeTypesInGatherer>(this));
    }
    
    internal abstract Term FreeBoundVariable(string name, int depth);
    
    public abstract Result GenerateSubstitutionMapping(Term desired, Dictionary<Free, Term> varMap, Dictionary<TyVar, Type> typeMap);
    public abstract string DefaultPrint();

    public sealed override string ToString()
    {
        return Printer.UniversalPrinter.Print(this);
    }
}