using HawkMajor2.Language.Inference.Types;
using HawkMajor2.NameGenerators;
using Results;
using Valiant;
using Valiant.Terms;
using Valiant.Types;

namespace HawkMajor2.Language.Inference.Terms;

public sealed record InfApp(InfTerm Application, InfTerm Argument) : InfTerm
{
    internal override Result<InfTerm> FixCombTypes()
    {
        if (!Application.FixCombTypes().Deconstruct(out var application, out var error))
            return error;

        if (!Argument.FixCombTypes().Deconstruct(out var argument, out error))
            return error;
        
        var appType = application.TypeOf();

        switch (appType)
        {
            case InfTyVar:
            case InfTyApp {Name: not "fun"}:
            case InfTyFixed {Type: TyVar or TyApp {Name: not "fun"}}:
                return $"Type not convertible to function: {appType}";
            case InfTyUnbound { Name: var name }:
                var newApp = application.ConvertUnboundTypeToFunction(name);
                return new InfApp(newApp, argument);
            default:
                if (Application == application && Argument == argument)
                    return this;
                return new InfApp(application, argument);
        }
            
    }

    internal override InfType TypeOf()
    {
        if (Application.TypeOf() is InfTyApp {Name: "fun", Args: [_, var outputType]})
            return outputType;

        throw new ArgumentOutOfRangeException();
    }

    internal override InfTerm ConvertUnboundTypeToFunction(string name)
    {
        var newApp = Application.ConvertUnboundTypeToFunction(name);
        var newArg = Argument.ConvertUnboundTypeToFunction(name);
        
        if (newApp == Application && newArg == Argument)
            return this;
        
        return new InfApp(newApp, newArg);
    }

    internal override InfTerm UniquifyUnboundTypeNames(NameGenerator generator)
    {
        var newApp = Application.UniquifyUnboundTypeNames(generator);
        var newArg = Argument.UniquifyUnboundTypeNames(generator);
        
        if (newApp == Application && newArg == Argument)
            return this;
        
        return new InfApp(newApp, newArg);
    }

    internal override Result GenerateMappings(Kernel kernel, HashSet<(InfType, InfType)> mappings, NameGenerator nameGenerator)
    {
        var appType = Application.TypeOf();
        if (appType is not InfTyApp {Name: "fun", Args: [var inputType, _]})
            return $"Type not convertible to function: {appType}";
        
        var argType = Argument.TypeOf();
        
        if (inputType != argType)
            mappings.Add((inputType, argType));
        
        return Application.GenerateMappings(kernel, mappings, nameGenerator) && Argument.GenerateMappings(kernel, mappings, nameGenerator);
    }

    internal override HashSet<InfType> GetTypesOfBoundVariable(int depth)
    {
        var appTypes = Application.GetTypesOfBoundVariable(depth);
        var argTypes = Argument.GetTypesOfBoundVariable(depth);
        appTypes.UnionWith(argTypes);
        return appTypes;
    }

    internal override InfTerm FixAbsVariables(Stack<string?> nameStack)
    {
        var newApp = Application.FixAbsVariables(nameStack);
        var newArg = Argument.FixAbsVariables(nameStack);
        
        if (newApp == Application && newArg == Argument)
            return this;
        
        return new InfApp(newApp, newArg);
    }

    internal override InfTerm SubstituteType(string name, InfType type)
    {
        var newApp = Application.SubstituteType(name, type);
        var newArg = Argument.SubstituteType(name, type);
        
        if (newApp == Application && newArg == Argument)
            return this;
        
        return new InfApp(newApp, newArg);
    }

    internal override void GetUsedTyVarNames(HashSet<string> names)
    {
        Application.GetUsedTyVarNames(names);
        Argument.GetUsedTyVarNames(names);
    }

    internal override InfTerm NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map)
    {
        var newApp = Application.NicelyRename(nameGenerator, map);
        var newArg = Argument.NicelyRename(nameGenerator, map);
        
        if (newApp == Application && newArg == Argument)
            return this;
        
        return new InfApp(newApp, newArg);
    }

    internal override Result<Term> BindTerm(Kernel kernel, Stack<string> temporaryBoundVariableNames, NameGenerator nameGenerator)
    {
        if (!Application.BindTerm(kernel, temporaryBoundVariableNames, nameGenerator).Deconstruct(out var application, out var error))
            return error;

        if (!Argument.BindTerm(kernel, temporaryBoundVariableNames, nameGenerator).Deconstruct(out var argument, out error))
            return error;

        return Result<Term>.Cast(kernel.MakeApp(application, argument));
    }

    internal override void GetUsedVarNames(HashSet<string> names)
    {
        Application.GetUsedVarNames(names);
        Argument.GetUsedVarNames(names);
    }

    public override void GetFrees(HashSet<(string name, InfType type)> frees)
    {
        Application.GetFrees(frees);
        Argument.GetFrees(frees);
    }

    public bool Equals(InfApp? other)
    {
        if (other is null)
            return false;
        
        return Application == other.Application && Argument == other.Argument;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Application, Argument);
    }

    public override string DefaultPrint()
    {
        return $"({Application} {Argument})";
    }

    public static InfApp FromApp(App app)
    {
        return new InfApp(FromTerm(app.Application), FromTerm(app.Argument));
    }
}