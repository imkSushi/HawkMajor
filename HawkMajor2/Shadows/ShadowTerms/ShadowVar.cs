using HawkMajor2.Shadows.ShadowTypes;
using Valiant.Terms;

namespace HawkMajor2.Shadows.ShadowTerms;

public abstract record ShadowVar : ShadowTerm
{
    internal ShadowVar(ShadowType type)
    {
        Type = type;
    }
    public ShadowType Type { get; }
    
    public void Deconstruct(out ShadowType type)
    {
        type = Type;
    }
    
    public static ShadowVar FromFree(Free free, Dictionary<string, ShadowVarType> fixedTerms, Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!fixedTerms.TryGetValue(free.Name, out var type))
            return new ShadowFree(free.Name, ShadowType.ToShadowType(free.Type, fixedTypes));
        
        return type switch
        {
            ShadowVarType.Free => new ShadowFree(free.Name, ShadowType.ToShadowType(free.Type, fixedTypes)),
            ShadowVarType.Fixed => new ShadowFixed(free.Name, ShadowType.ToShadowType(free.Type, fixedTypes)),
            ShadowVarType.Unfixed => new ShadowUnfixed(free.Name, ShadowType.ToShadowType(free.Type, fixedTypes)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}