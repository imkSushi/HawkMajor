using HawkMajor2.Shadows.ShadowTerms.MatchData;
using HawkMajor2.Shadows.ShadowTypes;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Shadows.ShadowTerms;

public sealed record ShadowBound : ShadowVar
{
    internal ShadowBound(int index, ShadowType type) : base(type)
    {
        Index = index;
    }
    
    public int Index { get; }
    
    public void Deconstruct(out int index, out ShadowType type)
    {
        index = Index;
        type = Type;
    }
    
    public override string DefaultPrint()
    {
        return Index.ToString();
    }

    public override bool ContainsUnfixed()
    {
        return Type.ContainsUnfixed();
    }

    public override bool Match(Term term, MatchTermData maps)
    {
        return maps.BoundVariables[Index] == term;
    }

    public override bool Match(ShadowTerm term, MatchShadowTermData maps)
    {
        return term is ShadowBound(var index, var type)
            && index == Index
            && Type.Match(type, maps.TypeData);
    }

    public override bool ContainsUnboundBound(int depth = 0)
    {
        return depth >= Index;
    }

    public override ShadowTerm FreeBoundVariable(string name, int depth)
    {
        return depth == Index ? new ShadowFree(name, Type) : this;
    }

    public override Result<ShadowTerm> RemoveFixedTerms(Dictionary<ShadowFixed, ShadowTerm> termMap, Dictionary<ShadowTyFixed, ShadowType> typeMap)
    {
        if (!Type.RemoveFixedTerms(typeMap).Deconstruct(out var type, out var error))
            return error;
        
        return new ShadowBound(Index, type);
    }

    public override Result<Term> ConvertToTerm(Kernel kernel)
    {
        return $"Unbound bound variable at index {Index}";
    }

    public override Result<Term> ConvertToTerm(Dictionary<ShadowFixed, Term> termMap, Dictionary<ShadowTyFixed, Type> typeMap, Kernel kernel)
    {
        return $"Unbound bound variable at index {Index}";
    }

    internal override ShadowTerm BindVariable(ShadowFree free, int depth)
    {
        return this;
    }

    public override ShadowTerm FixTerms(HashSet<string> terms, HashSet<string> types)
    {
        return new ShadowBound(Index, Type.FixTypes(types));
    }

    public override ShadowBound FixMeta()
    {
        return new ShadowBound(Index, Type.FixMeta());
    }
}