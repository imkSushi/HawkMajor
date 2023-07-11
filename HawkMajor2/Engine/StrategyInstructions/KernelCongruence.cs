using HawkMajor2.Extensions;
using HawkMajor2.Shadows;
using Valiant;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record KernelCongruence(ShadowTheorem AppTheorem, ShadowTheorem ArgTheorem) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        if (Prove(AppTheorem, ArgTheorem, data).IsNull(out var appThm, out var argThm))
            return null;
                
        if (!data.Kernel.Congruence(appThm, argThm).Deconstruct(out var thm, out _))
            return null;
                
        return TryTheorem(thm, data);
    }
}