using HawkMajor2.Language.Inference.Types;
using HawkMajor2.NameGenerators;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Inference.Terms;

public sealed record InfUnbound(string Name, InfType Type) : InfVar(Type)
{
    internal override InfTerm ConvertUnboundTypeToFunction(string name)
    {
        var newType = Type.ConvertUnboundTypeToFunction(name);
        if (newType == Type)
            return this;
        
        return new InfUnbound(Name, newType);
    }

    internal override InfTerm UniquifyUnboundTypeNames(NameGenerator generator)
    {
        var dict = new Dictionary<string, string>();
        var newType = Type.UniquifyUnboundTypeNames(generator, dict);
        
        if (dict.Count == 0)
            return this;
        
        return new InfUnbound(Name, newType);
    }

    internal override Result GenerateMappings(Kernel kernel, HashSet<(InfType, InfType)> mappings, NameGenerator nameGenerator)
    {
        throw new InvalidOperationException(); //Should be free or bound by now
    }

    internal override HashSet<InfType> GetTypesOfBoundVariable(int depth)
    {
        throw new InvalidOperationException(); //Should be free or bound by now
    }

    internal override InfTerm FixAbsVariables(Stack<string?> nameStack)
    {
        for (var i = nameStack.Count - 1; i >= 0; i--)
        {
            var name = nameStack.ElementAt(i);
            if (name != Name)
                continue;
            
            return new InfBound(nameStack.Count - 1 - i, Type);
        }

        return new InfFree(Name, Type);
    }

    internal override InfTerm SubstituteType(string name, InfType type)
    {
        var newType = Type.SubstituteType(name, type);
        if (newType == Type)
            return this;
        
        return new InfUnbound(Name, newType);
    }

    internal override InfTerm NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map)
    {
        var newType = Type.NicelyRename(nameGenerator, map);
        if (newType == Type)
            return this;
        
        return new InfUnbound(Name, newType);
    }

    internal override Result<Term> BindTerm(Kernel kernel, Stack<string> temporaryBoundVariableNames, NameGenerator nameGenerator)
    {
        return $"Expected {Name} to be bound by now";
    }

    internal override void GetUsedVarNames(HashSet<string> names)
    {
        names.Add(Name);
    }

    public override void GetFrees(HashSet<(string name, InfType type)> frees)
    {
        throw new InvalidOperationException(); //Should be free or bound by now
    }

    public bool Equals(InfUnbound? other)
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