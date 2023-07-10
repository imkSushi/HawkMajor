using HawkMajor2.Shadows;
using Valiant;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record KernelEqModusPonens(ShadowTheorem Major, ShadowTheorem Minor) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        var theorems = Prove(Major, Minor, data);
        if (theorems is null)
            return null;
        
        var (majorThm, minorThm) = theorems.Value;
                
        if (!data.Kernel.EqModusPonens(majorThm, minorThm).Deconstruct(out var thm, out _))
            return null;
                
        return TryTheorem(thm, data);
    }
}