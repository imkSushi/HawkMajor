using HawkMajor2.Shadows;
using Valiant;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record KernelAntisymmetry(ShadowTheorem Left, ShadowTheorem Right) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        var theorems = Prove(Left, Right, data);
        if (theorems is null)
            return null;
        
        var (leftThm, rightThm) = theorems.Value;

        var thm = data.Kernel.Antisymmetry(leftThm, rightThm);
                
        return TryTheorem(thm, data);
    }
}