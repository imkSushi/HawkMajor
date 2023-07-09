using HawkMajor2.NameGenerators;
using Results;
using Valiant;

namespace HawkMajor2.Language.Inference.Types;

public sealed record InfTyVar(string Name) : InfType
{
    public override Result<Type> BindTypes(Kernel kernel, Dictionary<string, string> unboundTypeNames, NameGenerator nameGenerator)
    {
        return kernel.MakeType(Name);
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
        names.Add(Name);
    }

    internal override InfType NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map)
    {
        return this;
    }

    internal override Result<Type> BindType(Kernel kernel)
    {
        return kernel.MakeType(Name);
    }

    public override string DefaultPrint()
    {
        return Name;
    }

    public bool Equals(InfTyVar? other)
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