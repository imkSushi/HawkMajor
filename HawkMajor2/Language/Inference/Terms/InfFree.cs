using HawkMajor2.Language.Inference.Types;
using HawkMajor2.NameGenerators;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Inference.Terms;

public sealed record InfFree(string Name, InfType Type) : InfVar(Type)
{
    internal override InfTerm ConvertUnboundTypeToFunction(string name)
    {
        var newType = Type.ConvertUnboundTypeToFunction(name);
        if (newType == Type)
            return this;
        
        return new InfFree(Name, newType);
    }

    internal override InfTerm UniquifyUnboundTypeNames(NameGenerator generator)
    {
        var dict = new Dictionary<string, string>();
        var newType = Type.UniquifyUnboundTypeNames(generator, dict);
        
        if (dict.Count == 0)
            return this;
        
        return new InfFree(Name, newType);
    }

    internal override Result GenerateMappings(Kernel kernel, HashSet<(InfType, InfType)> mappings, NameGenerator nameGenerator)
    {
        return Result.Success;
    }

    internal override HashSet<InfType> GetTypesOfBoundVariable(int depth)
    {
        return new HashSet<InfType>();
    }

    internal override InfTerm FixAbsVariables(Stack<string?> nameStack)
    {
        return this;
    }

    internal override InfTerm SubstituteType(string name, InfType type)
    {
        var newType = Type.SubstituteType(name, type);
        if (newType == Type)
            return this;
        
        return new InfFree(Name, newType);
    }

    internal override InfTerm NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map)
    {
        var newType = Type.NicelyRename(nameGenerator, map);
        if (newType == Type)
            return this;
        
        return new InfFree(Name, newType);
    }

    internal override Result<Term> BindTerm(Kernel kernel, Stack<string> temporaryBoundVariableNames, NameGenerator nameGenerator)
    {
        if (!Type.BindType(kernel).Deconstruct(out var type, out var error))
            return error;
        
        return kernel.MakeVar(Name, type);
    }

    internal override void GetUsedVarNames(HashSet<string> names)
    {
        names.Add(Name);
    }

    public override void GetFrees(HashSet<(string name, InfType type)> frees)
    {
        frees.Add((Name, Type));
    }

    public bool Equals(InfFree? other)
    {
        if (other is null)
            return false;
        
        return other.Name == Name && other.Type == Type;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Type);
    }

    public override string DefaultPrint()
    {
        return Name;
    }
}