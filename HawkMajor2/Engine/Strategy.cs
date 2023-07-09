using HawkMajor2.Shadows;
using HawkMajor2.Shadows.ShadowTerms;
using HawkMajor2.Shadows.ShadowTerms.MatchData;
using HawkMajor2.Shadows.ShadowTypes;
using HawkMajor2.Shadows.ShadowTypes.MatchData;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Engine;

public class Strategy
{
    public Strategy(string name, ShadowTheorem pattern, List<StrategyInstruction> instructions)
    {
        Name = name;
        Pattern = pattern;
        Instructions = instructions;
    }
    public string Name { get; }
    public ShadowTheorem Pattern { get; }
    public List<StrategyInstruction> Instructions { get; }

    public Theorem? Apply(Conjecture conjecture, Workspace workspace)
    {
        foreach (var matching in Pattern.Match(conjecture, new MatchTermData(), true, false))
        {
            var typeMap = matching.TypeData.UnfixedTypeMap.ToDictionary(
                kv => new ShadowTyFixed(kv.Key.Name),
                kv => kv.Value);
            
            var toFix = matching.TypeData.UnfixedTypeMap.Keys.Select(x => x.Name).ToHashSet();
            
            var termMap = matching.UnfixedTermMap.ToDictionary(
                kv => new ShadowFixed(kv.Key.Name, kv.Key.Type.FixTypes(toFix)),
                kv => kv.Value);
            
            var result = ApplyInstruction(new ProvingData(conjecture, workspace, termMap, typeMap, 0, new List<Theorem>()));
            
            if (result is not null)
                return result;
        }
        
        return null;
    }

    private Theorem? ApplyInstruction(ProvingData data)
    {
        if (data.InstructionIndex >= Instructions.Count)
            return null;
        
        var instruction = Instructions[data.InstructionIndex];

        switch (instruction)
        {
            case Prove(var shadowTheorem):
            {
                return Prove(shadowTheorem, data, true);
            }
            case ProveBy(var strategy, var shadowTheorem):
            {
                if (!shadowTheorem.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var conjecture, out _))
                    return null;
                
                var strategyProved = strategy.Apply(conjecture, data.Workspace);
                if (strategyProved is null)
                    return null;
                
                return TryTheorem(strategyProved, data);
            }
            case ProveLocal(var shadowTheorem):
            {
                foreach (var localThm in data.LocalTheorems.Concat(data.Workspace.CurrentScope.EnumerateTheorems()))
                {
                    var map = new MatchTermData(data.TermMap, new(), new MatchTypeData(data.TypeMap, new()));

                    foreach (var match in shadowTheorem.Match(localThm, map, true, false))
                    {
                        var newTypeMap = match.TypeData.UnfixedTypeMap
                            .Select(kv => new KeyValuePair<ShadowTyFixed, Type>(kv.Key.Fix(), kv.Value))
                            .Concat(match.TypeData.FixedTypeMap)
                            .ToDictionary(kv => kv.Key, kv => kv.Value);
            
                        var toFix = match.TypeData.UnfixedTypeMap.Keys.Select(x => x.Name).ToHashSet();
                        var newTermMap = match.UnfixedTermMap
                            .Select(kv => new KeyValuePair<ShadowFixed, Term>(kv.Key.Fix(toFix), kv.Value))
                            .Concat(match.FixedTermMap)
                            .ToDictionary(kv => kv.Key, kv => kv.Value);
                        
                        var result = ApplyInstruction(data with{TermMap = newTermMap, TypeMap = newTypeMap, InstructionIndex = data.InstructionIndex + 1});
                        if (result is not null)
                            return result;
                    }
                }
                
                return null;
            }
            case KernelReflexivity(var shadowTerm):
            {
                if (!shadowTerm.ConvertToTerm(data.TermMap, data.TypeMap, data.Kernel).Deconstruct(out var term, out _))
                    return null;
                
                var thm = data.Kernel.Reflexivity(term);

                return TryTheorem(thm, data);
            }
            case KernelCongruence(var shadowAppTheorem, var shadowArgTheorem):
            {
                if (!shadowAppTheorem.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var appConjecture, out _))
                    return null;

                if (!shadowArgTheorem.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel)
                        .Deconstruct(out var argConjecture, out _))
                    return null;

                var appTheorem = Prove(appConjecture, data, false);
                if (appTheorem is null)
                    return null;
                
                var argTheorem = Prove(argConjecture, data, false);
                if (argTheorem is null)
                    return null;
                
                if (!data.Kernel.Congruence(appTheorem, argTheorem).Deconstruct(out var thm, out _))
                    return null;
                
                return TryTheorem(thm, data);
            }
            case KernelParameterAbstraction(var shadowFreeTerm, var shadowTheorem):
            {
                if (!shadowFreeTerm.ConvertToTerm(data.TermMap, data.TypeMap, data.Kernel).Deconstruct(out var freeTerm, out _))
                    return null;

                if (freeTerm is not Free free)
                    return null;
                
                if (!shadowTheorem.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var conjecture, out _))
                    return null;
                
                var workspaceProved = data.Workspace.Prove(conjecture);
                if (workspaceProved is null)
                    return null;
                
                if (!data.Kernel.Abstraction(free, workspaceProved).Deconstruct(out var thm, out _))
                    return null;
                
                return TryTheorem(thm, data);
            }
            case KernelTypeAbstraction(var shadowType, var shadowTheorem):
            {
                if (!shadowType.ConvertToType(data.TypeMap, data.Kernel).Deconstruct(out var type, out _))
                    return null;
                
                if (!shadowTheorem.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var conjecture, out _))
                    return null;
                
                var workspaceProved = data.Workspace.Prove(conjecture);
                if (workspaceProved is null)
                    return null;
                
                if (!data.Kernel.Abstraction(type, workspaceProved).Deconstruct(out var thm, out _))
                    return null;
                
                return TryTheorem(thm, data);
            }
            case KernelBetaReduction(var shadowFreeTerm, var shadowBody):
            {
                if (!shadowFreeTerm.ConvertToTerm(data.TermMap, data.TypeMap, data.Kernel).Deconstruct(out var freeTerm, out _))
                    return null;
                
                if (freeTerm is not Free free)
                    return null;
                
                if (!shadowBody.ConvertToTerm(data.TermMap, data.TypeMap, data.Kernel).Deconstruct(out var body, out _))
                    return null;
                
                var thm = data.Kernel.BetaReduction(free, body);
                
                return TryTheorem(thm, data);
            }
            case KernelAssume(var shadowTerm):
            {
                if (!shadowTerm.ConvertToTerm(data.TermMap, data.TypeMap, data.Kernel).Deconstruct(out var term, out _))
                    return null;
                
                if (!data.Kernel.Assume(term).Deconstruct(out var thm, out _))
                    return null;
                
                return TryTheorem(thm, data);
            }
            case KernelEqModusPonens(var shadowMajor, var shadowMinor):
            {
                if (!shadowMajor.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var major, out _))
                    return null;
                
                if (!shadowMinor.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var minor, out _))
                    return null;
                
                var majorThm = Prove(major, data, false);
                if (majorThm is null)
                    return null;
                
                var minorThm = Prove(minor, data, false);
                if (minorThm is null)
                    return null;
                
                if (!data.Kernel.EqModusPonens(majorThm, minorThm).Deconstruct(out var thm, out _))
                    return null;
                
                return TryTheorem(thm, data);
            }
            case KernelAntisymmetry(var shadowLeft, var shadowRight):
            {
                if (!shadowLeft.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var left, out _))
                    return null;
                
                if (!shadowRight.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var right, out _))
                    return null;
                
                var leftThm = Prove(left, data, false);
                if (leftThm is null)
                    return null;
                
                var rightThm = Prove(right, data, false);
                if (rightThm is null)
                    return null;

                var thm = data.Kernel.Antisymmetry(leftThm, rightThm);
                
                return TryTheorem(thm, data);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(instruction));
        }
    }

    private Theorem? Prove(ShadowTheorem shadowTheorem, ProvingData data, bool applyNextInstruction)
    {
        if (!shadowTheorem.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var output, out _))
            return null;

        return Prove(output, data, applyNextInstruction);
    }

    private Theorem? Prove(Conjecture output, ProvingData data, bool applyNextInstruction)
    {
        for (var i = data.LocalTheorems.Count - 1; i >= 0; i--)
        {
            var localThm = data.LocalTheorems[i];
            var thm = output.CheckIfInstance(localThm, data.Kernel);

            if (thm is null)
                continue;
            if (!applyNextInstruction)
                return thm;

            var result = TryTheorem(thm, data);
            if (result is not null)
                return result;
        }

        var workspaceProved = data.Workspace.Prove(output);
        if (workspaceProved is null)
            return null;
        
        if (!applyNextInstruction)
            return workspaceProved;

        return TryTheorem(workspaceProved, data);
    }

    private Theorem? TryTheorem(Theorem theorem, ProvingData data)
    {
        var output = data.Conjecture.CheckIfInstance(theorem, data.Kernel);
        if (output is not null)
            return output;
                
        data.LocalTheorems.Add(theorem);
        var result = ApplyInstruction(data with {InstructionIndex = data.InstructionIndex + 1});
        if (result is not null)
            return result;
        data.LocalTheorems.RemoveAt(data.LocalTheorems.Count - 1);
                
        return null;
    }
}

public abstract record StrategyInstruction;
public sealed record Prove(ShadowTheorem Theorem) : StrategyInstruction;
public sealed record ProveLocal(ShadowTheorem Theorem) : StrategyInstruction;
public sealed record ProveBy(Strategy Strategy, ShadowTheorem Theorem) : StrategyInstruction;
public sealed record KernelReflexivity(ShadowTerm Term) : StrategyInstruction;
public sealed record KernelCongruence(ShadowTheorem AppTheorem, ShadowTheorem ArgTheorem) : StrategyInstruction;
public sealed record KernelParameterAbstraction(ShadowTerm Free, ShadowTheorem Theorem) : StrategyInstruction;
public sealed record KernelTypeAbstraction(ShadowType Type, ShadowTheorem Theorem) : StrategyInstruction;
public sealed record KernelBetaReduction(ShadowTerm Free, ShadowTerm Body) : StrategyInstruction;
public sealed record KernelAssume(ShadowTerm Term) : StrategyInstruction;
public sealed record KernelEqModusPonens(ShadowTheorem Major, ShadowTheorem Minor) : StrategyInstruction;
public sealed record KernelAntisymmetry(ShadowTheorem Left, ShadowTheorem Right) : StrategyInstruction;

public struct ProvingData
{
    public Conjecture Conjecture;
    public Workspace Workspace;
    public Dictionary<ShadowFixed, Term> TermMap;
    public Dictionary<ShadowTyFixed, Type> TypeMap;
    public int InstructionIndex;
    public List<Theorem> LocalTheorems;
    
    public Kernel Kernel => Workspace.Kernel;

    public ProvingData(Conjecture conjecture,
        Workspace workspace,
        Dictionary<ShadowFixed, Term> termMap,
        Dictionary<ShadowTyFixed, Type> typeMap,
        int instructionIndex,
        List<Theorem> localTheorems)
    {
        Conjecture = conjecture;
        Workspace = workspace;
        TermMap = termMap;
        TypeMap = typeMap;
        InstructionIndex = instructionIndex;
        LocalTheorems = localTheorems;
    }
}