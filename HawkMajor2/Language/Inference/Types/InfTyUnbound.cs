using HawkMajor2.NameGenerators;
using Results;
using Valiant;

namespace HawkMajor2.Language.Inference.Types;

public sealed record InfTyUnbound(string Name) : InfType
{
    public override Result<Type> BindTypes(Kernel kernel, Dictionary<string, string> unboundTypeNames, NameGenerator nameGenerator)
    {
        if (unboundTypeNames.TryGetValue(Name, out var boundName))
            return kernel.MakeType(boundName);

        nameGenerator.MoveNext();
        var newName = nameGenerator.Current;
        
        unboundTypeNames[Name] = newName;
        
        return kernel.MakeType(newName);
    }

    internal override InfType ConvertUnboundTypeToFunction(string name)
    {
        if (Name == name)
            return new InfTyApp("fun", new InfType[] {new InfTyUnbound($"{name}.argument"), new InfTyUnbound($"{name}.result")});
        
        return this;
    }

    internal override InfType UniquifyUnboundTypeNames(NameGenerator generator, Dictionary<string, string> unboundTypeNames)
    {
        if (unboundTypeNames.TryGetValue(Name, out var newName))
            return new InfTyUnbound(newName);
        
        generator.MoveNext();
        newName = generator.Current;
        unboundTypeNames[Name] = newName;
        
        return new InfTyUnbound(newName);
    }

    internal override InfType SubstituteType(string name, InfType type)
    {
        if (Name == name)
            return type;
        
        return this;
    }

    internal override Result CheckUnboundNameLoop(string name)
    {
        if (Name == name)
            return $"Unbound type name '{name}' is recursive";
        
        return Result.Success;
    }

    internal override void GetUsedTyVarNames(HashSet<string> names)
    {
        
    }

    internal override InfType NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map)
    {
        if (map.TryGetValue(Name, out var newName))
            return new InfTyVar(newName);
        
        nameGenerator.MoveNext();
        newName = nameGenerator.Current;
        map[Name] = newName;
        
        return new InfTyVar(newName);
    }

    internal override Result<Type> BindType(Kernel kernel)
    {
        return kernel.MakeType(Name);
    }

    public override string DefaultPrint()
    {
        return Name;
    }

    public bool Equals(InfTyUnbound? other)
    {
        if (other is null)
            return false;
        
        return other.Name == Name;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Name);
    }
}