namespace HawkMajor2.Shadows.ShadowTypes.MatchData;

public struct MatchShadowTypeData
{
    public MatchShadowTypeData(Dictionary<ShadowTyFixed, ShadowType> fixedTypeMap, Dictionary<ShadowTyUnfixed, ShadowType> unfixedTypeMap)
    {
        FixedTypeMap = fixedTypeMap;
        UnfixedTypeMap = unfixedTypeMap;
    }
    
    public MatchShadowTypeData()
    {
        FixedTypeMap = new();
        UnfixedTypeMap = new();
    }
    
    public Dictionary<ShadowTyFixed, ShadowType> FixedTypeMap { get; }
    public Dictionary<ShadowTyUnfixed, ShadowType> UnfixedTypeMap { get; }
    
    public MatchShadowTypeData Clone()
    {
        return new(new Dictionary<ShadowTyFixed, ShadowType>(FixedTypeMap), new Dictionary<ShadowTyUnfixed, ShadowType>(UnfixedTypeMap));
    }
}