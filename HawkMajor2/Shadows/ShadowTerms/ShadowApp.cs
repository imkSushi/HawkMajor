using HawkMajor2.Shadows.ShadowTerms.MatchData;
using HawkMajor2.Shadows.ShadowTypes;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Shadows.ShadowTerms;

public sealed record ShadowApp : ShadowTerm
{
    internal ShadowApp(ShadowTerm application, ShadowTerm argument)
    {
        Application = application;
        Argument = argument;
    }
    
    public ShadowTerm Application { get; }
    public ShadowTerm Argument { get; }
    
    public void Deconstruct(out ShadowTerm app, out ShadowTerm arg)
    {
        app = Application;
        arg = Argument;
    }
    
    public override string DefaultPrint()
    {
        return $"({Application.DefaultPrint()} {Argument.DefaultPrint()})";
    }
    
    public override bool ContainsUnfixed()
    {
        return Application.ContainsUnfixed() || Argument.ContainsUnfixed();
    }
    
    public override bool Match(Term term, MatchTermData maps)
    {
        return term is App(var app, var arg)
            && Application.Match(app, maps)
            && Argument.Match(arg, maps);
    }
    
    public override bool Match(ShadowTerm term, MatchShadowTermData maps)
    {
        return term is ShadowApp(var app, var arg)
            && Application.Match(app, maps)
            && Argument.Match(arg, maps);
    }

    public override bool ContainsUnboundBound(int depth = 0)
    {
        return Application.ContainsUnboundBound(depth) || Argument.ContainsUnboundBound(depth);
    }

    public override ShadowTerm FreeBoundVariable(string name, int depth)
    {
        return new ShadowApp(Application.FreeBoundVariable(name, depth), Argument.FreeBoundVariable(name, depth));
    }

    public override Result<ShadowTerm> RemoveFixedTerms(Dictionary<ShadowFixed, ShadowTerm> termMap, Dictionary<ShadowTyFixed, ShadowType> typeMap)
    {
        if (!Application.RemoveFixedTerms(termMap, typeMap).Deconstruct(out var app, out var error))
            return error;
        
        if (!Argument.RemoveFixedTerms(termMap, typeMap).Deconstruct(out var arg, out error))
            return error;
        
        return new ShadowApp(app, arg);
    }

    public override Result<Term> ConvertToTerm(Kernel kernel)
    {
        if (!Application.ConvertToTerm(kernel).Deconstruct(out var app, out var error))
            return error;
        
        if (!Argument.ConvertToTerm(kernel).Deconstruct(out var arg, out error))
            return error;
        
        return Result<Term>.Cast(kernel.MakeApp(app, arg));
    }

    public override Result<Term> ConvertToTerm(Dictionary<ShadowFixed, Term> termMap, Dictionary<ShadowTyFixed, Type> typeMap, Kernel kernel)
    {
        if (!Application.ConvertToTerm(termMap, typeMap, kernel).Deconstruct(out var app, out var error))
            return error;
        
        if (!Argument.ConvertToTerm(termMap, typeMap, kernel).Deconstruct(out var arg, out error))
            return error;
        
        return Result<Term>.Cast(kernel.MakeApp(app, arg));
    }

    internal override ShadowTerm BindVariable(ShadowFree free, int depth)
    {
        return new ShadowApp(Application.BindVariable(free, depth), Argument.BindVariable(free, depth));
    }
    
    public static ShadowApp FromApp(App app, Dictionary<string, ShadowVarType> fixedTerms, Dictionary<string, ShadowVarType> fixedTypes)
    {
        return new ShadowApp(ToShadowTerm(app.Application, fixedTerms, fixedTypes), ToShadowTerm(app.Argument, fixedTerms, fixedTypes));
    }
}