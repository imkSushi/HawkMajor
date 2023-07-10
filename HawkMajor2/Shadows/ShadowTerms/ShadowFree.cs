using HawkMajor2.Shadows.ShadowTerms.MatchData;
using HawkMajor2.Shadows.ShadowTypes;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Shadows.ShadowTerms;

public sealed record ShadowFree : ShadowVar
{
    internal ShadowFree(string name, ShadowType type) : base(type)
    {
        Name = name;
    }
    
    public string Name { get; }
    
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
        return term is Free(var name, var type)
            && name == Name
            && Type.Match(type, maps.TypeData);
    }
    
    public override bool Match(ShadowTerm term, MatchShadowTermData maps)
    {
        return term is ShadowFree(var name, var type)
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
        
        return new ShadowFree(Name, type);
    }

    public override Result<Term> ConvertToTerm(Kernel kernel)
    {
        if (!Type.ConvertToType(kernel).Deconstruct(out var type, out var error))
            return error;

        return kernel.MakeVar(Name, type);
    }

    public override Result<Term> ConvertToTerm(Dictionary<ShadowFixed, Term> termMap, Dictionary<ShadowTyFixed, Type> typeMap, Kernel kernel)
    {
        if (!Type.ConvertToType(typeMap, kernel).Deconstruct(out var type, out var error))
            return error;

        return kernel.MakeVar(Name, type);
    }

    internal override ShadowTerm BindVariable(ShadowFree free, int depth)
    {
        if (this == free)
            return new ShadowBound(depth, Type);
        
        return this;
    }

    public override ShadowTerm FixTerms(HashSet<string> terms, HashSet<string> types)
    {
        return new ShadowFree(Name, Type.FixTypes(types));
    }

    public override ShadowFree FixMeta()
    {
        return new ShadowFree(Name, Type.FixMeta());
    }
}