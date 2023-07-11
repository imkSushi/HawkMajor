using HawkMajor2.Extensions;
using HawkMajor2.Shadows;
using HawkMajor2.Shadows.ShadowTypes;
using Valiant;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record KernelTypeAbstraction(ShadowType Type, ShadowTheorem Theorem) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        if (!Type.ConvertToType(data.TypeMap, data.Kernel).Deconstruct(out var type, out _))
            return null;
                
        if (Prove(Theorem, data, false).IsNull(out var theorem))
            return null;
                
        if (!data.Kernel.Abstraction(type, theorem).Deconstruct(out var thm, out _))
            return null;
                
        return TryTheorem(thm, data);
    }
}