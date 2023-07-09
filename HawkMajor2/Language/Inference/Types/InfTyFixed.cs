using HawkMajor2.NameGenerators;
using Results;
using Valiant;

namespace HawkMajor2.Language.Inference.Types;

public sealed record InfTyFixed(Type Type) : InfType
{
    public override Result<Type> BindTypes(Kernel kernel, Dictionary<string, string> unboundTypeNames, NameGenerator nameGenerator)
    {
        return Type;
    }

    internal override InfType ConvertUnboundTypeToFunction(string name)
    {
        return this;
    }

    internal override InfType UniquifyUnboundTypeNames(NameGenerator generator, Dictionary<string, string> unboundTypeNames)
    {
        return this;
    }

    internal override InfType SubstituteType(string name, InfType type)
    {
        return this;
    }

    internal override Result CheckUnboundNameLoop(string name)
    {
        return Result.Success;
    }

    internal override void GetUsedTyVarNames(HashSet<string> names)
    {
        GetUsedTyVarNames(Type, names);
    }

    internal override InfType NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map)
    {
        return this;
    }

    internal override Result<Type> BindType(Kernel kernel)
    {
        return Type;
    }

    public override string DefaultPrint()
    {
        return Type.ToString();
    }

    internal InfType Unbind(NameGenerator generator, Dictionary<string, string> typeMap)
    {
        return UnboundFromType(Type, generator, typeMap);
    }
    
    internal InfType SingleUnbind()
    {
        return SingleUnboundFromType(Type);
    }

    public bool Equals(InfTyFixed? other)
    {
        if (other is null)
            return false;
        
        return other.Type == Type;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type);
    }
}