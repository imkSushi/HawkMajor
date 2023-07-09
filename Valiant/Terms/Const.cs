using Results;
using Valiant.Types;

namespace Valiant.Terms;

public sealed record Const : Term
{
    internal Const(string name, Type type)
    {
        Name = name;
        Type = type;
    }
    public string Name { get; }
    public Type Type { get; }
    public void Deconstruct(out string name, out Type type)
    {
        name = Name;
        type = Type;
    }
    
    public override Type TypeOf()
    {
        return Type;
    }

    internal override Const BindVariable(Free? variable, int index)
    {
        return this;
    }

    public override bool IsFree(Free variable)
    {
        return false;
    }

    internal override Const SafeInstantiate(Dictionary<Free, Term> map)
    {
        return this;
    }

    public override Const Instantiate(Dictionary<TyVar, Type> map)
    {
        return new Const(Name, Type.Instantiate(map));
    }

    public override bool ContainsFrees()
    {
        return false;
    }

    internal override Const FreeBoundVariable(string name, int depth)
    {
        return this;
    }

    public override Result GenerateSubstitutionMapping(Term desired, Dictionary<Free, Term> varMap, Dictionary<TyVar, Type> typeMap)
    {
        if (desired is not Const c)
            return $"Expected constant {Name} to be substituted with constant, but got {desired}";
        
        if (Name != c.Name)
            return $"Mismatch between {Name} and {c.Name}";
        
        return Type.GenerateSubstitutionMapping(c.Type, typeMap);
    }

    public override string DefaultPrint()
    {
        return Name;
    }
}