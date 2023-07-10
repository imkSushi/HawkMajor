using HawkMajor2.Extensions;
using HawkMajor2.NameGenerators;
using HawkMajor2.Shadows.Gatherers;
using HawkMajor2.Shadows.ShadowTerms.MatchData;
using HawkMajor2.Shadows.ShadowTypes;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Shadows.ShadowTerms;

public sealed record ShadowAbs : ShadowTerm
{
    internal ShadowAbs(ShadowType parameterType, ShadowTerm body)
    {
        ParameterType = parameterType;
        Body = body;
    }

    internal ShadowAbs(ShadowTerm body, ShadowFree free)
    {
        Body = body.BindVariable(free, 0);
        ParameterType = free.Type;
    }
    
    public ShadowType ParameterType { get; }
    internal ShadowTerm Body { get; }
    
    public ShadowTerm GetBody(string variableName)
    {
        return Body.FreeBoundVariable(variableName, 0);
    }
    
    public (ShadowTerm body, string boundVariable) GetBody()
    {
        var frees = ShadowFreesInNamesGatherer.GatherData<ShadowFreesInNamesGatherer>(this);

        var uniqueName = NameGenerator.GetUniqueWord(frees);

        var body = GetBody(uniqueName);
        
        return (body, uniqueName);
    }
    
    public void Deconstruct(out ShadowType parameterType, out ShadowTerm body)
    {
        parameterType = ParameterType;
        body = Body;
    }

    public override string DefaultPrint()
    {
        return $"(\\{ParameterType}. {Body})";
    }

    public override bool ContainsUnfixed()
    {
        return ParameterType.ContainsUnfixed() || Body.ContainsUnfixed();
    }

    public override bool Match(Term term, MatchTermData maps)
    {
        if (term is not Abs abs)
        {
            return false;
        }

        if (!ParameterType.Match(abs.ParameterType, maps.TypeData))
            return false;

        var freesIn = new HashSet<Free>();
        term.FreesIn(freesIn);
        var nameGen = new NameGenerator {ToAvoid = freesIn.Select(x => x.Name).ToHashSet()};
        nameGen.MoveNext();
        var newName = nameGen.Current;
        var body = abs.GetBody(newName, out var free);
        
        maps.BoundVariables.Add(free);
        var output = Body.Match(body, maps);
        maps.BoundVariables.RemoveAt(maps.BoundVariables.Count - 1);
        return output;
    }
    
    public override bool Match(ShadowTerm term, MatchShadowTermData maps)
    {
        if (term is not ShadowAbs abs)
        {
            return false;
        }

        return ParameterType.Match(abs.ParameterType, maps.TypeData) && Body.Match(abs.Body, maps);
    }

    public override bool ContainsUnboundBound(int depth = 0)
    {
        return Body.ContainsUnboundBound(depth + 1);
    }

    public override ShadowTerm FreeBoundVariable(string name, int depth)
    {
        return new ShadowAbs(ParameterType, Body.FreeBoundVariable(name, depth + 1));
    }

    public override Result<ShadowTerm> RemoveFixedTerms(Dictionary<ShadowFixed, ShadowTerm> termMap, Dictionary<ShadowTyFixed, ShadowType> typeMap)
    {
        if (!ParameterType.RemoveFixedTerms(typeMap).Deconstruct(out var newParameterType, out var error))
            return error;
        
        if (!Body.RemoveFixedTerms(termMap, typeMap).Deconstruct(out var newBody, out error))
            return error;
        
        return new ShadowAbs(newParameterType, newBody);
    }

    public override Result<Term> ConvertToTerm(Kernel kernel)
    {
        if (!ParameterType.ConvertToType(kernel).Deconstruct(out var parameterType, out var error))
            return error;

        var (shadowBody, variableName) = GetBody();
        
        if (!shadowBody.ConvertToTerm(kernel).Deconstruct(out var body, out error))
            return error;
        
        var free = kernel.MakeVar(variableName, parameterType);

        return kernel.MakeAbs(free, body);
    }

    public override Result<Term> ConvertToTerm(Dictionary<ShadowFixed, Term> termMap, Dictionary<ShadowTyFixed, Type> typeMap, Kernel kernel)
    {
        if (!ParameterType.ConvertToType(typeMap, kernel).Deconstruct(out var parameterType, out var error))
            return error;
        
        var freesIn = new HashSet<string>();
        
        var nameGatherer = new ShadowFreesInNamesGatherer();
        nameGatherer.Gather(this, freesIn);
        foreach (var term in termMap.Values)
        {
            nameGatherer.Gather(term, freesIn);
        }
        
        var nameGen = new NameGenerator {ToAvoid = freesIn};
        nameGen.MoveNext();
        
        var variableName = nameGen.Current;

        var shadowBody = GetBody(variableName);
        
        if (!shadowBody.ConvertToTerm(termMap, typeMap, kernel).Deconstruct(out var body, out error))
            return error;
        
        var free = kernel.MakeVar(variableName, parameterType);

        return kernel.MakeAbs(free, body);
    }

    internal override ShadowTerm BindVariable(ShadowFree free, int depth)
    {
        var body = Body.BindVariable(free, depth + 1);
        return new ShadowAbs(ParameterType, body);
    }

    public static ShadowAbs FromAbs(Abs abs, Dictionary<string, ShadowVar> fixedTerms, Dictionary<string, ShadowTyMeta> fixedTypes)
    {
        var parameterType = ShadowType.ToShadowType(abs.ParameterType, fixedTypes);
        var (body, free) = abs.GetBody();

        ShadowTerm shadowBody;

        if (fixedTerms.TryGetValue(free.Name, out var shadowFreeValue))
        {
            fixedTerms.Remove(free.Name);
            shadowBody = ToShadowTerm(body, fixedTerms, fixedTypes);
            fixedTerms[free.Name] = shadowFreeValue;
        }
        else
        {
            shadowBody = ToShadowTerm(body, fixedTerms, fixedTypes);
        }

        return new ShadowAbs(shadowBody, new ShadowFree(free.Name, parameterType));
    }

    public override ShadowTerm FixTerms(HashSet<string> terms, HashSet<string> types)
    {
        return new ShadowAbs(ParameterType.FixTypes(types), Body.FixTerms(terms, types));
    }

    public override ShadowAbs FixMeta()
    {
        return new ShadowAbs(ParameterType.FixMeta(), Body.FixMeta());
    }
}