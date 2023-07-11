using HawkMajor2.Extensions;
using HawkMajor2.Shadows;
using Valiant;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record ProveBy(Strategy Strategy, ShadowTheorem Theorem) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        if (!Theorem.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var conjecture, out _))
            return null;
                
        if (Strategy.Apply(conjecture, data.Workspace).IsNull(out var strategyProved))
            return null;
                
        return TryTheorem(strategyProved, data);
    }
}