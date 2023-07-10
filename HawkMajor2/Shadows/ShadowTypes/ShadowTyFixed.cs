using HawkMajor2.Shadows.ShadowTypes.MatchData;
using Results;
using Valiant;

namespace HawkMajor2.Shadows.ShadowTypes;

public sealed record ShadowTyFixed : ShadowTyMeta
{
    internal ShadowTyFixed(string name) : base(name)
    {
        
    }
    
    public override string DefaultPrint()
    {
        return Name;
    }

    public override bool ContainsUnfixed()
    {
        return false;
    }

    public override bool Match(Type type, MatchTypeData maps)
    {
        return maps.FixedTypeMap.TryGetValue(this, out var fixedType) && fixedType == type;
    }

    public override bool Match(ShadowType type, MatchShadowTypeData maps)
    {
        return maps.FixedTypeMap.TryGetValue(this, out var fixedType) && fixedType == type;
    }

    public override Result<ShadowType> RemoveFixedTerms(Dictionary<ShadowTyFixed, ShadowType> typeMap)
    {
        if (typeMap.TryGetValue(this, out var type))
            return type;
        
        return $"Could not find fixed type '{Name}' in map";
    }

    public override Result<Type> ConvertToType(Kernel kernel)
    {
        return $"Meta type '{Name}' cannot be converted to a type";
    }

    public override Result<Type> ConvertToType(Dictionary<ShadowTyFixed, Type> typeMap, Kernel kernel)
    {
        if (typeMap.TryGetValue(this, out var type))
            return type;
        
        return $"Could not find fixed type '{Name}' in map";
    }

    public override ShadowType FixTypes(HashSet<string> toFix)
    {
        return this;
    }
    
    public override ShadowTyFixed FixMeta()
    {
        return this;
    }
}