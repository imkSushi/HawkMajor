using HawkMajor2.Shadows.ShadowTypes.MatchData;

namespace HawkMajor2.Shadows.ShadowTerms.MatchData;

public struct MatchShadowTermData
{
    public MatchShadowTermData(Dictionary<ShadowFixed, ShadowTerm> fixedTermMap, Dictionary<ShadowUnfixed, ShadowTerm> unfixedTermMap, MatchShadowTypeData typeData)
    {
        FixedTermMap = fixedTermMap;
        UnfixedTermMap = unfixedTermMap;
        TypeData = typeData;
    }
    
    public MatchShadowTermData()
    {
        FixedTermMap = new();
        UnfixedTermMap = new();
        TypeData = new();
    }
    
    public Dictionary<ShadowFixed, ShadowTerm> FixedTermMap { get; }
    public Dictionary<ShadowUnfixed, ShadowTerm> UnfixedTermMap { get; }
    public MatchShadowTypeData TypeData { get; }
    
    public MatchShadowTermData Clone()
    {
        return new(new Dictionary<ShadowFixed, ShadowTerm>(FixedTermMap), new Dictionary<ShadowUnfixed, ShadowTerm>(UnfixedTermMap), TypeData.Clone());
    }
}