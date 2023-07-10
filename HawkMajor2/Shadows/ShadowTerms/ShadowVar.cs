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
    
    public static ShadowVar FromFree(Free free, Dictionary<string, ShadowVar> fixedTerms, Dictionary<string, ShadowTyMeta> fixedTypes)
    {
        if (fixedTerms.TryGetValue(free.Name, out var type))
            return type;
        
        return new ShadowFree(free.Name, ShadowType.ToShadowType(free.Type, fixedTypes));

    }

    public abstract override ShadowVar FixMeta();
}