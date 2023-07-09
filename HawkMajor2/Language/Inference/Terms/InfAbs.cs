using HawkMajor2.Language.Inference.Types;
using HawkMajor2.NameGenerators;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Inference.Terms;

public sealed record InfAbs(InfType ParameterType, string? ParameterName, InfTerm Body) : InfTerm
{
    internal override Result<InfTerm> FixCombTypes()
    {
        if (!Body.FixCombTypes().Deconstruct(out var fixedBody, out var error))
            return error;

        if (fixedBody == Body)
            return this;
        
        return new InfAbs(ParameterType, ParameterName, fixedBody);
    }

    internal override InfType TypeOf()
    {
        return new InfTyApp("fun", new []{ParameterType, Body.TypeOf()});
    }

    internal override InfTerm ConvertUnboundTypeToFunction(string name)
    {
        var newType = ParameterType.ConvertUnboundTypeToFunction(name);
        var newBody = Body.ConvertUnboundTypeToFunction(name);
        
        if (newType == ParameterType && newBody == Body)
            return this;
        
        return new InfAbs(newType, ParameterName, newBody);
    }

    internal override InfTerm UniquifyUnboundTypeNames(NameGenerator generator)
    {
        var dict = new Dictionary<string, string>();
        var newType = ParameterType.UniquifyUnboundTypeNames(generator, dict);
        
        var newBody = Body.UniquifyUnboundTypeNames(generator);
        
        if (dict.Count == 0 && newBody == Body)
            return this;
        
        return new InfAbs(newType, ParameterName, newBody);
    }

    internal override Result GenerateMappings(Kernel kernel, HashSet<(InfType, InfType)> mappings, NameGenerator nameGenerator)
    {
        var paramTypes = Body.GetTypesOfBoundVariable(0);
        
        foreach (var paramType in paramTypes)
        {
            mappings.Add((paramType, ParameterType));
        }
        
        return Body.GenerateMappings(kernel, mappings, nameGenerator);
    }

    internal override HashSet<InfType> GetTypesOfBoundVariable(int depth)
    {
        return Body.GetTypesOfBoundVariable(depth + 1);
    }

    internal override InfTerm FixAbsVariables(Stack<string?> nameStack)
    {
        nameStack.Push(ParameterName);
        var body = Body.FixAbsVariables(nameStack);
        nameStack.Pop();

        if (ParameterName == null && body == Body)
            return this;
        
        return new InfAbs(ParameterType, null, body);
    }

    internal override InfTerm SubstituteType(string name, InfType type)
    {
        var newType = ParameterType.SubstituteType(name, type);
        var newBody = Body.SubstituteType(name, type);
        
        if (newType == ParameterType && newBody == Body)
            return this;
        
        return new InfAbs(newType, ParameterName, newBody);
    }

    internal override void GetUsedTyVarNames(HashSet<string> names)
    {
        ParameterType.GetUsedTyVarNames(names);
        Body.GetUsedTyVarNames(names);
    }

    internal override InfTerm NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map)
    {
        var newType = ParameterType.NicelyRename(nameGenerator, map);
        var newBody = Body.NicelyRename(nameGenerator, map);
        
        if (newType == ParameterType && newBody == Body)
            return this;
        
        return new InfAbs(newType, ParameterName, newBody);
    }

    internal override Result<Term> BindTerm(Kernel kernel, Stack<string> temporaryBoundVariableNames, NameGenerator nameGenerator)
    {
        if (!ParameterType.BindType(kernel).Deconstruct(out var paramType, out var error))
            return error;
        
        string tempParamName;
        
        if (ParameterName != null)
        {
            tempParamName = ParameterName;
        }
        else
        {
            nameGenerator.MoveNext();
            tempParamName = nameGenerator.Current;
        }
        temporaryBoundVariableNames.Push(tempParamName);
        
        if (!Body.BindTerm(kernel, temporaryBoundVariableNames, nameGenerator).Deconstruct(out var body, out error))
            return error;
        
        temporaryBoundVariableNames.Pop();
        
        return kernel.MakeAbs(kernel.MakeVar(tempParamName, paramType), body);
    }

    internal override void GetUsedVarNames(HashSet<string> names)
    {
        if (ParameterName != null)
            names.Add(ParameterName);
        
        Body.GetUsedVarNames(names);
    }

    public override void GetFrees(HashSet<(string name, InfType type)> frees)
    {
        Body.GetFrees(frees);
    }

    public bool Equals(InfAbs? other)
    {
        if (other is null)
            return false;
        
        if (other.ParameterType != ParameterType)
            return false;

        if (other.ParameterName != ParameterName)
            return false;

        if (other.Body != Body)
            return false;
        
        return true;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ParameterType, ParameterName, Body);
    }

    public override string DefaultPrint()
    {
        return $"\\{ParameterName ?? ""}. {ParameterType}. {Body}";
    }
}