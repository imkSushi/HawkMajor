using HawkMajor2.Shadows.ShadowTypes.MatchData;
using Valiant.Terms;

namespace HawkMajor2.Shadows.ShadowTerms.MatchData;

public struct MatchTermData
{
    public MatchTermData(Dictionary<ShadowFixed, Term> fixedTermMap, Dictionary<ShadowUnfixed, Term> unfixedTermMap, MatchTypeData typeData)
    {
        FixedTermMap = fixedTermMap;
        UnfixedTermMap = unfixedTermMap;
        TypeData = typeData;
    }
    
    public MatchTermData()
    {
        FixedTermMap = new();
        UnfixedTermMap = new();
        TypeData = new();
    }
    
    public Dictionary<ShadowFixed, Term> FixedTermMap { get; }
    public Dictionary<ShadowUnfixed, Term> UnfixedTermMap { get; }
    public List<Free> BoundVariables { get; } = new();
    public MatchTypeData TypeData { get; }
    
    public MatchTermData Clone()
    {
        return new(new Dictionary<ShadowFixed, Term>(FixedTermMap), new Dictionary<ShadowUnfixed, Term>(UnfixedTermMap), TypeData.Clone());
    }
}