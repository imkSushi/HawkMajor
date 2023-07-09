using HawkMajor2.Language.Inference.Types;
using Results;

namespace HawkMajor2.Language.Inference.Terms;

public abstract record InfVar(InfType Type) : InfTerm
{
    internal override Result<InfTerm> FixCombTypes()
    {
        return this;
    }

    internal override InfType TypeOf()
    {
        return Type;
    }

    internal override void GetUsedTyVarNames(HashSet<string> names)
    {
        Type.GetUsedTyVarNames(names);
    }
}