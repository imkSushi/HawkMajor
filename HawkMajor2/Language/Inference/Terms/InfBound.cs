using HawkMajor2.Language.Inference.Types;
using HawkMajor2.NameGenerators;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Inference.Terms;

public sealed record InfBound(int Index, InfType Type) : InfVar(Type)
{
    internal override Result GenerateMappings(Kernel kernel, HashSet<(InfType, InfType)> mappings, NameGenerator nameGenerator)
    {
        return Result.Success;
    }

    internal override HashSet<InfType> GetTypesOfBoundVariable(int depth)
    {
        if (depth == Index)
            return new HashSet<InfType> { Type };
        
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
        
        return new InfBound(Index, newType);
    }

    internal override InfTerm NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map)
    {
        var newType = Type.NicelyRename(nameGenerator, map);
        if (newType == Type)
            return this;
        
        return new InfBound(Index, newType);
    }

    internal override Result<Term> BindTerm(Kernel kernel, Stack<string> temporaryBoundVariableNames, NameGenerator nameGenerator)
    {
        if (!Type.BindType(kernel).Deconstruct(out var type, out var error))
            return error;
        
        var temporaryBoundVariableNamesCount = temporaryBoundVariableNames.Count;
        if (Index >= temporaryBoundVariableNamesCount)
            return $"{Index} is higher than possible index of {temporaryBoundVariableNamesCount}";
        
        var name = temporaryBoundVariableNames.ElementAt(Index);

        return kernel.MakeVar(name, type);
    }

    internal override void GetUsedVarNames(HashSet<string> names)
    {
        
    }

    public override void GetFrees(HashSet<(string name, InfType type)> frees)
    {
        
    }

    internal override InfTerm ConvertUnboundTypeToFunction(string name)
    {
        var newType = Type.ConvertUnboundTypeToFunction(name);
        if (newType == Type)
            return this;
        
        return new InfBound(Index, newType);
    }

    internal override InfTerm UniquifyUnboundTypeNames(NameGenerator generator)
    {
        return this;
    }

    public bool Equals(InfBound? other)
    {
        if (other is null)
            return false;
        
        return other.Index == Index && other.Type == Type;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Index, Type);
    }

    public override string DefaultPrint()
    {
        return $"${Index}";
    }
}