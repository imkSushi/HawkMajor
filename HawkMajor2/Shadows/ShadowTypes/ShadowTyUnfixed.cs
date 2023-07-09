using HawkMajor2.Shadows.ShadowTypes.MatchData;
using Results;
using Valiant;

namespace HawkMajor2.Shadows.ShadowTypes;

public sealed record ShadowTyUnfixed : ShadowTyMeta
{
    internal ShadowTyUnfixed(string name) : base(name)
    {
        
    }
    
    public override string DefaultPrint()
    {
        return Name;
    }

    public override bool ContainsUnfixed()
    {
        return true;
    }

    public override bool Match(Type type, MatchTypeData maps)
    {
        if (maps.UnfixedTypeMap.TryGetValue(this, out var unfixedType))
            return unfixedType == type;

        maps.UnfixedTypeMap[this] = type;
        
        return true;
    }
    
    public override bool Match(ShadowType type, MatchShadowTypeData maps)
    {
        if (maps.UnfixedTypeMap.TryGetValue(this, out var unfixedType))
            return unfixedType == type;

        maps.UnfixedTypeMap[this] = type;
        
        return true;
    }

    public override Result<ShadowType> RemoveFixedTerms(Dictionary<ShadowTyFixed, ShadowType> typeMap)
    {
        return $"Unfixed type '{Name}' cannot be converted to a non-meta type";
    }

    public override Result<Type> ConvertToType(Kernel kernel)
    {
        return $"Meta type '{Name}' cannot be converted to a type";
    }

    public override Result<Type> ConvertToType(Dictionary<ShadowTyFixed, Type> typeMap, Kernel kernel)
    {
        return $"Unfixed type '{Name}' cannot be converted to a non-meta type";
    }

    public override ShadowType FixTypes(HashSet<string> toFix)
    {
        if (toFix.Contains(Name))
            return new ShadowTyFixed(Name);
        
        return this;
    }

    public ShadowTyFixed Fix()
    {
        return new ShadowTyFixed(Name);
    }
}