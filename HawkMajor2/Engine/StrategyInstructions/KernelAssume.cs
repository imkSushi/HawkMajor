using HawkMajor2.Shadows.ShadowTerms;
using Valiant;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record KernelAssume(ShadowTerm Term) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        if (!Term.ConvertToTerm(data.TermMap, data.TypeMap, data.Kernel).Deconstruct(out var term, out _))
            return null;
                
        if (!data.Kernel.Assume(term).Deconstruct(out var thm, out _))
            return null;
                
        return TryTheorem(thm, data);
    }
}