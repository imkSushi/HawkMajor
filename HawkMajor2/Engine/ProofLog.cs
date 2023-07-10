using Valiant;

namespace HawkMajor2.Engine;

public struct ProofLog
{
    public readonly Theorem Theorem;
    public readonly ProofMethod Method;
    public ProofLog(Theorem theorem, ProofMethod method)
    {
        Theorem = theorem;
        Method = method;
    }
}

public struct StrategyLog
{
    public readonly Strategy Strategy;
    public StrategyLog(Strategy strategy)
    {
        Strategy = strategy;
    }
}

public abstract record ProofMethod;
public sealed record ProofByInstantiation(Theorem Theorem) : ProofMethod;
public sealed record ProofByAssertion(Theorem Theorem) : ProofMethod;
public sealed record ProofByStrategy(StrategyLog Strategy) : ProofMethod;
public sealed record ProofByPreviouslyProved(Theorem Theorem) : ProofMethod;

public abstract record StrategyMethod;
public sealed record StrategyByInstantiation(StrategyMethod Theorem) : StrategyMethod;