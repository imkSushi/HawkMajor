using HawkMajor2.Shadows.ShadowTypes.MatchData;
using Results;
using Valiant;
using Valiant.Types;

namespace HawkMajor2.Shadows.ShadowTypes;

public sealed record ShadowTyVar : ShadowType
{
    internal ShadowTyVar(string name)
    {
        Name = name;
    }
    public string Name { get; }
    public void Deconstruct(out string name)
    {
        name = Name;
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
        return type is TyVar { Name: var name } && name == Name;
    }

    public override bool Match(ShadowType type, MatchShadowTypeData maps)
    {
        return type is ShadowTyVar { Name: var name } && name == Name;
    }

    public override Result<ShadowType> RemoveFixedTerms(Dictionary<ShadowTyFixed, ShadowType> typeMap)
    {
        return this;
    }

    public override Result<Type> ConvertToType(Kernel kernel)
    {
        return kernel.MakeType(Name);
    }

    public override Result<Type> ConvertToType(Dictionary<ShadowTyFixed, Type> typeMap, Kernel kernel)
    {
        return kernel.MakeType(Name);
    }

    public override ShadowType FixTypes(HashSet<string> toFix)
    {
        return this;
    }
}