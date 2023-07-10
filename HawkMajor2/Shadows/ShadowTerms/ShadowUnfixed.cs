using HawkMajor2.Shadows.ShadowTerms.MatchData;
using HawkMajor2.Shadows.ShadowTypes;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Shadows.ShadowTerms;

public sealed record ShadowUnfixed : ShadowMeta
{
    internal ShadowUnfixed(string name, ShadowType type) : base(name, type)
    {
        
    }
    
    public override string DefaultPrint()
    {
        return Name;
    }
    
    public override bool ContainsUnfixed()
    {
        return true;
    }

    public override bool Match(Term term, MatchTermData maps)
    {
        var freesIn = new HashSet<Free>();
        term.FreesIn(freesIn);
        if (freesIn.Intersect(maps.BoundVariables).Any())
            return false;
        
        if (maps.UnfixedTermMap.TryGetValue(this, out var unfixedTerm))
            return unfixedTerm == term;

        if (!Type.Match(term.TypeOf(), maps.TypeData))
            return false;
        
        maps.UnfixedTermMap[this] = term;
        
        return true;
    }
    
    public override bool Match(ShadowTerm term, MatchShadowTermData maps)
    {
        if (maps.UnfixedTermMap.TryGetValue(this, out var unfixedTerm))
            return unfixedTerm == term;

        if (term.ContainsUnboundBound())
            return false;
        
        maps.UnfixedTermMap[this] = term;
        
        return true;
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
        return $"Could not remove Unfixed variable {Name} from term.";
    }

    public override Result<Term> ConvertToTerm(Kernel kernel)
    {
        return $"Could not convert meta variable {Name} to term.";
    }

    public override Result<Term> ConvertToTerm(Dictionary<ShadowFixed, Term> termMap, Dictionary<ShadowTyFixed, Type> typeMap, Kernel kernel)
    {
        return $"Could not convert unfixed variable {Name} to term.";
    }

    internal override ShadowTerm BindVariable(ShadowFree free, int depth)
    {
        return this;
    }

    public override ShadowTerm FixTerms(HashSet<string> terms, HashSet<string> types)
    {
        if (terms.Contains(Name))
            return new ShadowFixed(Name, Type.FixTypes(types));
        
        return new ShadowUnfixed(Name, Type.FixTypes(types));
    }

    public ShadowFixed Fix(HashSet<string> toFix)
    {
        return new ShadowFixed(Name, Type.FixTypes(toFix));
    }

    public override ShadowFixed FixMeta()
    {
        return new ShadowFixed(Name, Type.FixMeta());
    }
}