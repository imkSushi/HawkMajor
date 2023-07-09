using HawkMajor2.Language.Inference.Types;
using HawkMajor2.NameGenerators;
using Results;
using Valiant;
using Valiant.Gatherers;
using Valiant.Terms;

namespace HawkMajor2.Language.Inference.Terms;

public sealed record InfFixed(Term Term) : InfTerm
{
    internal override Result<InfTerm> FixCombTypes()
    {
        return this;
    }

    internal override InfType TypeOf()
    {
        return new InfTyFixed(Term.TypeOf());
    }

    internal override InfTerm ConvertUnboundTypeToFunction(string name)
    {
        return this;
    }

    internal override InfTerm UniquifyUnboundTypeNames(NameGenerator generator)
    {
        return this;
    }

    internal override Result GenerateMappings(Kernel kernel,
        HashSet<(InfType, InfType)> mappings,
        NameGenerator nameGenerator)
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
        return this;
    }

    internal override void GetUsedTyVarNames(HashSet<string> names)
    {
        GetUsedTyVarNames(Term, names);
    }

    internal override InfTerm NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map)
    {
        return this;
    }

    internal override Result<Term> BindTerm(Kernel kernel, Stack<string> temporaryBoundVariableNames, NameGenerator nameGenerator)
    {
        return Term;
    }

    internal override void GetUsedVarNames(HashSet<string> names)
    {
        var freeVars = FreesInGatherer.GatherData<FreesInGatherer>(Term);
        names.UnionWith(freeVars.Select(free => free.Name));
    }

    public override void GetFrees(HashSet<(string name, InfType type)> frees)
    {
        var termFrees = new HashSet<Free>();
        Term.FreesIn(termFrees);
        frees.UnionWith(termFrees.Select(free => (free.Name, InfType.FromType(free.Type))));
    }

    public bool Equals(InfFixed? other)
    {
        if (other is null)
            return false;
        
        return other.Term == Term;
    }
    
    public override int GetHashCode()
    {
        return Term.GetHashCode();
    }

    public override string DefaultPrint()
    {
        return Term.ToString();
    }
}