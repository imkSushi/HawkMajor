using HawkMajor2.Shadows.ShadowTypes;

namespace HawkMajor2.Shadows.ShadowTerms;

public abstract record ShadowMeta : ShadowVar
{
    internal ShadowMeta(string name, ShadowType type) : base(type)
    {
        Name = name;
    }
    public string Name { get; }
    
    public void Deconstruct(out string name, out ShadowType type)
    {
        name = Name;
        type = Type;
    }
}