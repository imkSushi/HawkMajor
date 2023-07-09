using HawkMajor2.Shadows.ShadowTerms.MatchData;
using HawkMajor2.Shadows.ShadowTypes;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Shadows.ShadowTerms;

public sealed record ShadowFixed : ShadowMeta
{
    internal ShadowFixed(string name, ShadowType type) : base(name, type)
    {
        
    }
    
    public override string DefaultPrint()
    {
        return Name;
    }
    
    public override bool ContainsUnfixed()
    {
        return false;
    }

    public override bool Match(Term term, MatchTermData maps)
    {
        return maps.FixedTermMap.TryGetValue(this, out var fixedTerm) && fixedTerm == term;
    }
    
    public override bool Match(ShadowTerm term, MatchShadowTermData maps)
    {
        return maps.FixedTermMap.TryGetValue(this, out var fixedTerm) && fixedTerm == term;
    }

    public override bool ContainsUnboundBound(int depth = 0)
    {
        return false;
    }

    public override ShadowTerm FreeBoundVariable(string name, int depth)
    {
        return this;
    }

    public override Result<ShadowTerm> RemoveFixedTerms(Dictionary<ShadowFixed, ShadowTerm> termMap, Dictionary<ShadowTyFixed, ShadowType> typeMap)
    {
        if (termMap.TryGetValue(this, out var term))
            return term;

        return $"Could not find fixed variable {Name} in term map.";
    }

    public override Result<Term> ConvertToTerm(Kernel kernel)
    {
        return $"Could not convert meta variable {Name} to term.";
    }

    public override Result<Term> ConvertToTerm(Dictionary<ShadowFixed, Term> termMap, Dictionary<ShadowTyFixed, Type> typeMap, Kernel kernel)
    {
        if (termMap.TryGetValue(this, out var term))
            return term;
        
        return $"Could not find fixed variable {Name} in term map.";
    }

    internal override ShadowTerm BindVariable(ShadowFree free, int depth)
    {
        return this;
    }
}