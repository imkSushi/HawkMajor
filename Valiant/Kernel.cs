using System.Collections.ObjectModel;
using Results;
using Valiant.Terms;
using Valiant.Types;

namespace Valiant;

public sealed class Kernel
{
    private readonly Dictionary<string, int> _typeArities = new()
    {
        ["fun"] = 2,
        ["bool"] = 0
    };
    public ReadOnlyDictionary<string, int> TypeArities => _typeArities.AsReadOnly();
    
    private readonly Dictionary<string, Type> _constantTypes = new()
    {
        ["="] = new TyApp("fun", new TyVar("a"), new TyApp("fun", new TyVar("a"), new TyApp("bool")))
    };
    public ReadOnlyDictionary<string, Type> ConstantTypes => _constantTypes.AsReadOnly();
    
    public Type Bool => new TyApp("bool");
    public TyVar Aty => new("a");
    
    public Result DefineType(string name, int arity)
    {
        if (_typeArities.ContainsKey(name))
            return "Type is already defined.";
        
        _typeArities.Add(name, arity);
        return Result.Success;
    }
    
    public Result DefineConstant(string name, Type type)
    {
        if (_constantTypes.ContainsKey(name))
            return "Constant is already defined.";
        
        _constantTypes.Add(name, type);
        return Result.Success;
    }

    public TyVar MakeType(string name)
    {
        return new TyVar(name);
    }
    
    public Result<TyApp> MakeType(string name, params Type[] args)
    {
        if (!_typeArities.TryGetValue(name, out var arity)) 
            return $"Type {name} is not defined.";
        
        if (arity != args.Length)
            return $"Type {name} expects {arity} arguments, but {args.Length} were given.";
        
        return new TyApp(name, args);
    }
    
    public Abs MakeAbs(Free parameter, Term body)
    {
        var bound = body.BindVariable(parameter, 0);
        return new Abs(parameter.Type, bound);
    }

    public Abs MakeAbs(Type parameterType, Term body)
    {
        return new Abs(parameterType, body);
    }

    public Result<App> MakeApp(Term application, Term argument)
    {
        if (application.TypeOf() is not TyApp {Name: "fun", Args: [var inputType, _]})
            return $"Term {application} is not a function.";
        
        if (argument.TypeOf() != inputType)
            return $"Term {argument} is not of application input type {inputType}.";
        
        return new App(application, argument);
    }

    public Result<Const> MakeConst(string name, Dictionary<TyVar, Type> map)
    {
        if (!_constantTypes.TryGetValue(name, out var type))
            return $"Constant {name} is not defined.";
        
        return new Const(name, type.Instantiate(map));
    }

    public Result<Const> MakeConst(string name)
    {
        if (!_constantTypes.TryGetValue(name, out var type))
            return $"Constant {name} is not defined.";
        
        return new Const(name, type);
    }
    
    public Free MakeVar(string name, Type type)
    {
        return new Free(name, type);
    }

    public Theorem Reflexivity(Term term) // p becomes |- p = p
    {
        return new Theorem(SafeMakeEquals(term, term, term.TypeOf()));
    }

    public Result<Theorem> Congruence(Theorem applications, Theorem arguments) // |- f = g and |- x = y becomes |- (f x) = (g y)
    {
        if (!DeconstructEquality(applications.Conclusion).Deconstruct(out var left, out var right, out var error))
            return error;
        
        if (left.TypeOf() is not TyApp {Name: "fun", Args: [var inputType, var outputType]})
            return $"Term {applications} is not an equality of two functions.";
        
        if (!DeconstructEquality(arguments.Conclusion).Deconstruct(out var leftArg, out var rightArg, out error))
            return error;
        
        if (leftArg.TypeOf() != inputType)
            return $"Application type {leftArg.TypeOf()} does not take input type argument type {inputType} as input.";
        
        return new Theorem(applications.Premises.Union(arguments.Premises), SafeMakeEquals(new App(left, leftArg), new App(right, rightArg), outputType));
    }
    
    public Result<Theorem> Abstraction(Free parameter, Theorem theorem) // x and |- p = q becomes |- (\x.p) = (\x.q)
    {
        var (premises, conclusion) = theorem;
        
        if (premises.Any(premise => premise.IsFree(parameter)))
            return $"Premise {premises} contains free variable {parameter}.";
        
        if (!DeconstructEquality(conclusion).Deconstruct(out var left, out var right, out var error))
            return error;
        
        var leftBound = MakeAbs(parameter, left);
        var rightBound = MakeAbs(parameter, right);
        
        return new Theorem(premises, SafeMakeEquals(leftBound, rightBound, leftBound.TypeOf()));
    }
    
    public Result<Theorem> Abstraction(Type parameterType, Theorem theorem) // a type and |- p = q becomes |- (\x.p) = (\x.q)
    {
        var (premises, conclusion) = theorem;
        
        if (!DeconstructEquality(conclusion).Deconstruct(out var left, out var right, out var error))
            return error;
        
        var leftBound = MakeAbs(parameterType, left);
        var rightBound = MakeAbs(parameterType, right);
        
        return new Theorem(premises, SafeMakeEquals(leftBound, rightBound, leftBound.TypeOf()));
    }
    
    public Theorem BetaReduction(Free parameter, Term body) // x and p becomes |- (\x.p) x = p
    {
        var abs = MakeAbs(parameter, body);
        var app = new App(abs, parameter);
        
        return new Theorem(SafeMakeEquals(app, body, body.TypeOf()));
    }
    
    public Result<Theorem> Assume(Term term) // p becomes p |- p
    {
        if (term.TypeOf() != Bool)
            return $"Term {term} is not a boolean.";
        
        return new Theorem(term, term);
    }
    
    public Result<Theorem> EqModusPonens(Theorem major, Theorem minor) // |- p = q and |- p becomes |- q
    {
        var (majorPremises, majorConclusion) = major;
        var (minorPremises, minorConclusion) = minor;
        
        if (!DeconstructEquality(majorConclusion).Deconstruct(out var left, out var right, out var error))
            return error;
        
        if (left != minorConclusion)
            return $"Major premise left {left} does not match minor premise {minor}.";
        
        return new Theorem(majorPremises.Union(minorPremises), right);
    }
    
    public Theorem Antisymmetry(Theorem left, Theorem right) // q |- p and p |- q becomes |- p = q
    {
        var (leftPremises, leftConclusion) = left;
        var (rightPremises, rightConclusion) = right;
        
        var leftWithoutRight = leftPremises.ToHashSet();
        leftWithoutRight.Remove(rightConclusion);
        
        var rightWithoutLeft = rightPremises.ToHashSet();
        rightWithoutLeft.Remove(leftConclusion);
        
        return new Theorem(leftWithoutRight.Union(rightWithoutLeft), SafeMakeEquals(leftConclusion, rightConclusion, Bool));
    }
    
    public Result<Theorem> Instantiate(Theorem theorem, Dictionary<Free, Term> map)
    {
        foreach (var (free, value) in map)
        {
            if (free.Type != value.TypeOf())
                return $"Type of {value} does not match type of {free}.";
        }
        
        var (premises, conclusion) = theorem;
        
        return new Theorem(premises.Select( premise => premise.SafeInstantiate(map)), conclusion.SafeInstantiate(map));
    }
    
    public Result<Theorem> Instantiate(Theorem theorem, Dictionary<TyVar, Type> map)
    {
        var (premises, conclusion) = theorem;
        
        return new Theorem(premises.Select( premise => premise.Instantiate(map)), conclusion.Instantiate(map));
    }

    private Term SafeMakeEquals(Term left, Term right, Type type)
    {
        var eqType = new TyApp("fun", type, new TyApp("fun", type, new TyApp("bool")));
        var eq = new Const("=", eqType);
        
        return new App(new App(eq, left), right);
    }

    private Result<Term, Term> DeconstructEquality(Term term)
    {
        if (term is not App {Application: App {Application: Const {Name: "="}, Argument: var left}, Argument: var right})
            return $"Term {term} is not an equality.";
        
        return (left, right);
    }

    private List<Theorem> _axioms = new();
    public ReadOnlyCollection<Theorem> Axioms => _axioms.AsReadOnly();
    
    public Result<Theorem> NewAxiom(Term term)
    {
        if (term.TypeOf() != Bool)
            return $"Term {term} is not a boolean.";
        
        var theorem = new Theorem(term);

        if (!_axioms.Contains(theorem)) 
            _axioms.Add(theorem);

        return theorem;
    }
    
    private List<Theorem> _definitions = new();
    public ReadOnlyCollection<Theorem> Definitions => _definitions.AsReadOnly();

    public Result<Theorem> NewBasicDefinition(string name, Term definition)
    {
        if (definition.ContainsFrees())
            return $"Definition of {name} contains free variables.";

        var type = definition.TypeOf();

        if (DefineConstant(name, type).IsError(out var error))
            return error;
        
        var constant = new Const(name, type);
        
        var theorem = new Theorem(SafeMakeEquals(constant, definition, type));
        
        _definitions.Add(theorem);
        
        return theorem;
    }
    
    private List<Theorem> _constructors = new();
    public ReadOnlyCollection<Theorem> Constructors => _constructors.AsReadOnly();
    
    private List<Theorem> _destructors = new();
    public ReadOnlyCollection<Theorem> Destructors => _destructors.AsReadOnly();

    public Result<Theorem, Theorem> NewBasicTypeDefinition(string name,
        string constructorName,
        string destructorName,
        Term indicator)
    {
        if (_constantTypes.ContainsKey(constructorName))
            return $"Constructor name {constructorName} already in use.";
        
        if (_constantTypes.ContainsKey(destructorName))
            return $"Destructor name {destructorName} already in use.";
        
        if (_typeArities.ContainsKey(name))
            return $"Type name {name} already in use.";
        
        if (indicator.TypeOf() is not TyApp {Name: "fun", Args: [var rty, var boolTy]} || boolTy != Bool)
            return $"Indicator function {indicator} does not have type (a -> bool).";
        
        if (indicator.ContainsFrees())
            return "Indicator function contains free variables.";

        var freeTypes = new HashSet<TyVar>();
        indicator.FreeTypesIn(freeTypes);
        
        DefineType(name, freeTypes.Count);
        
        var aty = new TyApp(name, freeTypes.Cast<Type>().ToArray());
        
        var conType = new TyApp("fun", rty, aty);
        var destType = new TyApp("fun", aty, rty);
        
        DefineConstant(constructorName, conType);
        DefineConstant(destructorName, destType);
        
        var con = new Const(constructorName, conType);
        var dest = new Const(destructorName, destType);
        
        var a = new Free("a", aty);
        var r = new Free("r", rty);

        var constructor = new Theorem(SafeMakeEquals(new App(con, new App(dest, a)), a, aty));
        var destructor = new Theorem(SafeMakeEquals(new App(indicator, r), SafeMakeEquals(new App(dest, new App(con, r)), r, rty), Bool));
        
        _constructors.Add(constructor);
        _destructors.Add(destructor);
        
        return (constructor, destructor);
    }
}