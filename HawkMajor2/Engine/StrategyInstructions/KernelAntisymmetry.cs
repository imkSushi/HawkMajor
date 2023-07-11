using HawkMajor2.Extensions;
using HawkMajor2.Shadows;
using Valiant;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record KernelAntisymmetry(ShadowTheorem Left, ShadowTheorem Right) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        if (Prove(Left, Right, data).IsNull(out var leftThm, out var rightThm))
            return null;

        var thm = data.Kernel.Antisymmetry(leftThm, rightThm);
                
        return TryTheorem(thm, data);
    }
}