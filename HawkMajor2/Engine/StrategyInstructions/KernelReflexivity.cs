using HawkMajor2.Shadows.ShadowTerms;
using Valiant;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record KernelReflexivity(ShadowTerm Term) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        if (!Term.ConvertToTerm(data.TermMap, data.TypeMap, data.Kernel).Deconstruct(out var term, out _))
            return null;
                
        var thm = data.Kernel.Reflexivity(term);

        return TryTheorem(thm, data);
    }
}