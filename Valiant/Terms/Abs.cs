using Results;
using Valiant.Types;

namespace Valiant.Terms;

public sealed record Abs : Term
{
    internal Abs(Type parameterType, Term body)
    {
        ParameterType = parameterType;
        Body = body;
    }
    public Type ParameterType { get; }
    internal Term Body { get; }

    public Term GetBody(string variableName)
    {
        return Body.FreeBoundVariable(variableName, 0);
    }

    public Term GetBody(string variableName, out Free free)
    {
        free = new Free(variableName, ParameterType);
        return Body.FreeBoundVariable(variableName, 0);
    }
    
    internal void Deconstruct(out Type parameterType, out Term body)
    {
        parameterType = ParameterType;
        body = Body;
    }

    public override TyApp TypeOf()
    {
        return new TyApp("fun", ParameterType, Body.TypeOf());
    }

    internal override Abs BindVariable(Free? variable, int index)
    {
        return new Abs(ParameterType, Body.BindVariable(variable, index + 1));
    }

    public override bool IsFree(Free variable)
    {
        return Body.IsFree(variable);
    }

    internal override Abs SafeInstantiate(Dictionary<Free, Term> map)
    {
        return new Abs(ParameterType, Body.SafeInstantiate(map));
    }

    public override Abs Instantiate(Dictionary<TyVar, Type> map)
    {
        return new Abs(ParameterType.Instantiate(map), Body.Instantiate(map));
    }

    public override bool ContainsFrees()
    {
        return Body.ContainsFrees();
    }

    internal override Abs FreeBoundVariable(string name, int depth)
    {
        var newBody = Body.FreeBoundVariable(name, depth + 1);
        return newBody == Body ? this : new Abs(ParameterType, newBody);
    }

    public override Result GenerateSubstitutionMapping(Term desired, Dictionary<Free, Term> varMap, Dictionary<TyVar, Type> typeMap)
    {
        if (desired is not Abs {ParameterType: var type, Body: var body})
            return $"Expected an abstraction. Found: {desired}";
        
        return ParameterType.GenerateSubstitutionMapping(type, typeMap) && Body.GenerateSubstitutionMapping(body, varMap, typeMap);
    }

    public override string DefaultPrint()
    {
        return $"(\\{ParameterType}. {Body})";
    }
}