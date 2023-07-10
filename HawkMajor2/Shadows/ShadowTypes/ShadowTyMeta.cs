using Valiant.Types;

namespace HawkMajor2.Shadows.ShadowTypes;

public abstract record ShadowTyMeta : ShadowType
{
    internal ShadowTyMeta(string name)
    {
        Name = name;
    }
    public string Name { get; }
    
    public void Deconstruct(out string name)
    {
        name = Name;
    }
    
    public static ShadowType FromTyVar(TyVar type, Dictionary<string, ShadowTyMeta> fixedTypes)
    {
        if (!fixedTypes.TryGetValue(type.Name, out var shadowType))
            return new ShadowTyVar(type.Name);

        return shadowType;
    }

    public abstract override ShadowTyMeta FixMeta();
}