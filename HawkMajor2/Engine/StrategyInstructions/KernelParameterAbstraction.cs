using HawkMajor2.Extensions;
using HawkMajor2.Shadows;
using HawkMajor2.Shadows.ShadowTerms;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record KernelParameterAbstraction(ShadowTerm Free, ShadowTheorem Theorem) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        if (!Free.ConvertToTerm(data.TermMap, data.TypeMap, data.Kernel).Deconstruct(out var freeTerm, out _))
            return null;

        if (freeTerm is not Free free)
            return null;
                
        if (Prove(Theorem, data, false).IsNull(out var theorem))
            return null;
                
        if (!data.Kernel.Abstraction(free, theorem).Deconstruct(out var thm, out _))
            return null;
                
        return TryTheorem(thm, data);
    }
}