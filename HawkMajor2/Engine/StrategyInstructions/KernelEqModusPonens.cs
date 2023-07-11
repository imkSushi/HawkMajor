using HawkMajor2.Extensions;
using HawkMajor2.Shadows;
using Valiant;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record KernelEqModusPonens(ShadowTheorem Major, ShadowTheorem Minor) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        if (Prove(Major, Minor, data).IsNull(out var majorThm, out var minorThm))
            return null;
                
        if (!data.Kernel.EqModusPonens(majorThm, minorThm).Deconstruct(out var thm, out _))
            return null;
                
        return TryTheorem(thm, data);
    }
}