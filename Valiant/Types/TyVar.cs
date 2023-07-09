using Results;

namespace Valiant.Types;

public sealed record TyVar : Type
{
    internal TyVar(string name)
    {
        Name = name;
    }
    public string Name { get; }
    public void Deconstruct(out string name)
    {
        name = Name;
    }

    public override Type Instantiate(Dictionary<TyVar, Type> map)
    {
        return map.TryGetValue(this, out var type) ? type : this;
    }

    public override void FreeTypesIn(HashSet<TyVar> frees)
    {
        frees.Add(this);
    }

    public override void FreeTypeNamesIn(HashSet<string> frees)
    {
        frees.Add(Name);
    }

    public override Result GenerateSubstitutionMapping(Type desired, Dictionary<TyVar, Type> typeMap)
    {
        if (typeMap.TryGetValue(this, out var value))
        {
            if (typeMap[this] == desired)
                return Result.Success;
            return $"Type variable {Name} already mapped to {value}";
        }
        
        typeMap[this] = desired;
        return Result.Success;
    }

    public override string DefaultPrint()
    {
        return Name;
    }
}