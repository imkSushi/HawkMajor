using HawkMajor2.Shadows.ShadowTerms;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record KernelBetaReduction(ShadowTerm Free, ShadowTerm Body) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        if (!Free.ConvertToTerm(data.TermMap, data.TypeMap, data.Kernel).Deconstruct(out var freeTerm, out _))
            return null;
                
        if (freeTerm is not Free free)
            return null;
                
        if (!Body.ConvertToTerm(data.TermMap, data.TypeMap, data.Kernel).Deconstruct(out var body, out _))
            return null;
                
        var thm = data.Kernel.BetaReduction(free, body);
                
        return TryTheorem(thm, data);
    }
}