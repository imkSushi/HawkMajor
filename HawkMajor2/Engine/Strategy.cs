using HawkMajor2.Engine.StrategyInstructions;
using HawkMajor2.Shadows;
using HawkMajor2.Shadows.ShadowTerms;
using HawkMajor2.Shadows.ShadowTerms.MatchData;
using HawkMajor2.Shadows.ShadowTypes;
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
            var typeMap = GenerateUnfixedTypeMap(matching);
            
            var termMap = GenerateUnfixedTermMap(matching);

            var result = ApplyFirstInstruction(conjecture, workspace, termMap, typeMap);
            
            if (result is not null)
                return result;
        }
        
        return null;
    }

    private Theorem? ApplyFirstInstruction(Conjecture conjecture, Workspace workspace, Dictionary<ShadowFixed, Term> termMap, Dictionary<ShadowTyFixed, Type> typeMap)
    {
        return StrategyInstruction.ApplyInstruction(new ProvingData(conjecture, workspace, termMap, typeMap, 0, new List<Theorem>(), Instructions));
    }

    private static Dictionary<ShadowFixed, Term> GenerateUnfixedTermMap(MatchTermData matching)
    {
        var toFix = matching.TypeData.UnfixedTypeMap.Keys.Select(x => x.Name).ToHashSet();

        return matching.UnfixedTermMap.ToDictionary(
            kv => new ShadowFixed(kv.Key.Name, kv.Key.Type.FixTypes(toFix)),
            kv => kv.Value);
    }

    private static Dictionary<ShadowTyFixed, Type> GenerateUnfixedTypeMap(MatchTermData matching)
    {
        return matching.TypeData.UnfixedTypeMap.ToDictionary(
            kv => new ShadowTyFixed(kv.Key.Name),
            kv => kv.Value);
    }

    public override string ToString()
    {
        return $"Strategy {Name}";
    }
}