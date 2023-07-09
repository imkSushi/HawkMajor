namespace HawkMajor2.Shadows.ShadowTypes.MatchData;

public struct MatchTypeData
{
    public MatchTypeData(Dictionary<ShadowTyFixed, Type> fixedTypeMap, Dictionary<ShadowTyUnfixed, Type> unfixedTypeMap)
    {
        FixedTypeMap = fixedTypeMap;
        UnfixedTypeMap = unfixedTypeMap;
    }
    
    public MatchTypeData()
    {
        FixedTypeMap = new();
        UnfixedTypeMap = new();
    }
    
    public Dictionary<ShadowTyFixed, Type> FixedTypeMap { get; }
    public Dictionary<ShadowTyUnfixed, Type> UnfixedTypeMap { get; }
    
    public MatchTypeData Clone()
    {
        return new(new Dictionary<ShadowTyFixed, Type>(FixedTypeMap), new Dictionary<ShadowTyUnfixed, Type>(UnfixedTypeMap));
    }
}