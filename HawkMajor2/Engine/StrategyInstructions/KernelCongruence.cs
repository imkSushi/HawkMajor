using HawkMajor2.Shadows;
using Valiant;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record KernelCongruence(ShadowTheorem AppTheorem, ShadowTheorem ArgTheorem) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        var theorems = Prove(AppTheorem, ArgTheorem, data);
        if (theorems is null)
            return null;
        
        var (appThm, argThm) = theorems.Value;
                
        if (!data.Kernel.Congruence(appThm, argThm).Deconstruct(out var thm, out _))
            return null;
                
        return TryTheorem(thm, data);
    }
}