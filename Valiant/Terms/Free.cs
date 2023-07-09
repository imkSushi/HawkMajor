using Results;
using Valiant.Types;

namespace Valiant.Terms;

public sealed record Free : Var
{
    internal Free(string name, Type type) : base(type)
    {
        Name = name;
    }
    public string Name { get; }
    public void Deconstruct(out string name, out Type type)
    {
        name = Name;
        type = Type;
    }
    
    public override Type TypeOf()
    {
        return Type;
    }

    internal override Var BindVariable(Free? variable, int index)
    {
        if (this != variable)
            return this;
        
        return new Bound(index, Type);
    }

    public override bool IsFree(Free variable)
    {
        return variable == this;
    }

    internal override Term SafeInstantiate(Dictionary<Free, Term> map)
    {
        return map.TryGetValue(this, out var term) ? term : this;
    }

    public override Free Instantiate(Dictionary<TyVar, Type> map)
    {
        return new Free(Name, Type.Instantiate(map));
    }

    public override bool ContainsFrees()
    {
        return true;
    }

    internal override Free FreeBoundVariable(string name, int depth)
    {
        return this;
    }

    public override Result GenerateSubstitutionMapping(Term desired, Dictionary<Free, Term> varMap, Dictionary<TyVar, Type> typeMap)
    {
        if (varMap.TryGetValue(this, out var term))
        {
            if (term == desired)
                return Result.Success;
            
            return $"Term {desired} does not match {term}";
        }

        varMap[this] = desired;
        
        var desiredType = desired.TypeOf();

        return Type.GenerateSubstitutionMapping(desiredType, typeMap);
    }

    public override string DefaultPrint()
    {
        return Name;
    }
}