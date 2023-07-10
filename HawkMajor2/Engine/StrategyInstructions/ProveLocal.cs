using HawkMajor2.Shadows;
using HawkMajor2.Shadows.ShadowTerms;
using HawkMajor2.Shadows.ShadowTerms.MatchData;
using HawkMajor2.Shadows.ShadowTypes;
using HawkMajor2.Shadows.ShadowTypes.MatchData;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Engine.StrategyInstructions;

public sealed record ProveLocal(ShadowTheorem Theorem) : StrategyInstruction
{
    protected override Theorem? Apply(ProvingData data)
    {
        foreach (var localThm in data.LocalTheorems.Concat(data.Workspace.CurrentScope.EnumerateTheorems()))
        {
            var map = new MatchTermData(data.TermMap, new(), new MatchTypeData(data.TypeMap, new()));

            foreach (var match in Theorem.Match(localThm, map, true, false))
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
}