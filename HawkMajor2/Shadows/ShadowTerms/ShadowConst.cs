using HawkMajor2.Shadows.ShadowTerms.MatchData;
using HawkMajor2.Shadows.ShadowTypes;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Shadows.ShadowTerms;

public sealed record ShadowConst : ShadowTerm
{
    internal ShadowConst(string name, ShadowType type)
    {
        Name = name;
        Type = type;
    }
    
    public string Name { get; }
    public ShadowType Type { get; }
    
    public void Deconstruct(out string name, out ShadowType type)
    {
        name = Name;
        type = Type;
    }

    public override string DefaultPrint()
    {
        return Name;
    }

    public override bool ContainsUnfixed()
    {
        return Type.ContainsUnfixed();
    }

    public override bool Match(Term term, MatchTermData maps)
    {
        return term is Const(var name, var type)
            && name == Name
            && Type.Match(type, maps.TypeData);
    }

    public override bool Match(ShadowTerm term, MatchShadowTermData maps)
    {
        return term is ShadowConst(var name, var type)
            && name == Name
            && Type.Match(type, maps.TypeData);
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
        if (!Type.RemoveFixedTerms(typeMap).Deconstruct(out var type, out var error))
            return error;
        
        return new ShadowConst(Name, type);
    }

    public override Result<Term> ConvertToTerm(Kernel kernel)
    {
        if (!Type.ConvertToType(kernel).Deconstruct(out var type, out var error))
            return error;
        
        if (!kernel.ConstantTypes.TryGetValue(Name, out var defaultType))
            return $"Constant {Name} not found";
        
        if (!TermMapGenerator.GenerateMap(type, defaultType, out var typeMap))
            return $"Constant {Name} has type {type} that doesn't match {defaultType}";

        return Result<Term>.Cast(kernel.MakeConst(Name, typeMap));
    }

    public override Result<Term> ConvertToTerm(Dictionary<ShadowFixed, Term> termMap, Dictionary<ShadowTyFixed, Type> typeMap, Kernel kernel)
    {
        if (!Type.ConvertToType(typeMap, kernel).Deconstruct(out var type, out var error))
            return error;
        
        if (!kernel.ConstantTypes.TryGetValue(Name, out var defaultType))
            return $"Constant {Name} not found";
        
        if (!TermMapGenerator.GenerateMap(type, defaultType, out var typeMap2))
            return $"Constant {Name} has type {type} that doesn't match {defaultType}";
        
        return Result<Term>.Cast(kernel.MakeConst(Name, typeMap2));
    }

    internal override ShadowTerm BindVariable(ShadowFree free, int depth)
    {
        return this;
    }
    
    public static ShadowConst FromConst(Const constant, Dictionary<string, ShadowVarType> fixedTerms, Dictionary<string, ShadowVarType> fixedTypes)
    {
        return new ShadowConst(constant.Name, ShadowType.ToShadowType(constant.Type, fixedTypes));
    }
}