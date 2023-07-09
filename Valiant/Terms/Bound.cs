using Results;
using Valiant.Types;

namespace Valiant.Terms;

public sealed record Bound : Var
{
    internal Bound(int index, Type type) : base(type)
    {
        Index = index;
    }
    public int Index { get; }
    public void Deconstruct(out int index, out Type type)
    {
        index = Index;
        type = Type;
    }

    public bool Equals(Bound? other)
    {
        if (other is null)
            return false;

        return Index == other.Index;
    }

    public override int GetHashCode()
    {
        return Index.GetHashCode();
    }

    public override Type TypeOf()
    {
        return Type;
    }

    internal override Bound BindVariable(Free? variable, int index)
    {
        return this;
    }

    public override bool IsFree(Free variable)
    {
        return false;
    }

    internal override Term SafeInstantiate(Dictionary<Free, Term> map)
    {
        return this;
    }

    public override Bound Instantiate(Dictionary<TyVar, Type> map)
    {
        return new Bound(Index, Type.Instantiate(map));
    }

    public override bool ContainsFrees()
    {
        return false;
    }

    internal override Var FreeBoundVariable(string name, int depth)
    {
        if (depth == Index)
            return new Free(name, Type);
        
        return this;
    }

    public override Result GenerateSubstitutionMapping(Term desired, Dictionary<Free, Term> varMap, Dictionary<TyVar, Type> typeMap)
    {
        if (desired is not Bound bound)
            return $"Bound variable {Index} cannot be substituted for {desired}";
        
        if (Index != bound.Index)
            return $"Bound variable {Index} cannot be substituted for {bound.Index}";
        
        return Type.GenerateSubstitutionMapping(bound.Type, typeMap);
    }

    public override string DefaultPrint()
    {
        return Index.ToString();
    }
}