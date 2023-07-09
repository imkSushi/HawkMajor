using Results;
using Valiant.Types;

namespace Valiant.Terms;

public sealed record App : Term
{
    internal App(Term application, Term argument)
    {
        Application = application;
        Argument = argument;
    }
    public Term Application { get; }
    public Term Argument { get; }
    public void Deconstruct(out Term application, out Term argument)
    {
        application = Application;
        argument = Argument;
    }

    public override Type TypeOf()
    {
        return ((TyApp)Application.TypeOf()).Args[1];
    }

    internal override App BindVariable(Free? variable, int index)
    {
        return new App(Application.BindVariable(variable, index), Argument.BindVariable(variable, index));
    }

    public override bool IsFree(Free variable)
    {
        return Application.IsFree(variable) || Argument.IsFree(variable);
    }

    internal override App SafeInstantiate(Dictionary<Free, Term> map)
    {
        return new App(Application.SafeInstantiate(map), Argument.SafeInstantiate(map));
    }

    public override App Instantiate(Dictionary<TyVar, Type> map)
    {
        return new App(Application.Instantiate(map), Argument.Instantiate(map));
    }

    public override bool ContainsFrees()
    {
        return Application.ContainsFrees() || Argument.ContainsFrees();
    }

    internal override App FreeBoundVariable(string name, int depth)
    {
        var application = Application.FreeBoundVariable(name, depth);
        var argument = Argument.FreeBoundVariable(name, depth);
        
        return application == Application && argument == Argument ? this : new App(application, argument);
    }

    public override Result GenerateSubstitutionMapping(Term desired, Dictionary<Free, Term> varMap, Dictionary<TyVar, Type> typeMap)
    {
        if (desired is not App {Application: var app, Argument: var arg})
            return $"Mismatched types: {this} and {desired}";
        
        return Application.GenerateSubstitutionMapping(app, varMap, typeMap) &&
               Argument.GenerateSubstitutionMapping(arg, varMap, typeMap);
    }

    public override string DefaultPrint()
    {
        return $"({Application} {Argument})";
    }
}