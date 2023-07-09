using HawkMajor2.Language.Inference.Terms;
using HawkMajor2.Language.Inference.Types;
using HawkMajor2.NameGenerators;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Inference;

public sealed class TermTypeInference : TypeInference<List<InfTerm>, List<Term>>
{
    private Kernel _kernel;
    public TermTypeInference(Kernel kernel)
    {
        _kernel = kernel;
    }

    public override Result<List<InfTerm>> PartialInference(List<InfTerm> input)
    {
        return PartialInference(input, false);
    }
    
    public Result<List<InfTerm>> PartialInference(List<InfTerm> input, bool allBools)
    {
        string? error;
        
        var nameGenerator = new NameGenerator
        {
            FirstLetterConfig = new LetterConfig(true, false, false),
            LetterConfig = new LetterConfig(true, false, true)
        };
       
        var uniquelyTypeds = new List<InfTerm>();
        
        foreach (var inputTerm in input)
        {
            var uniquelyTyped = UniquifyUnboundTypeNames(inputTerm, nameGenerator);
            uniquelyTypeds.Add(uniquelyTyped);
        }
        
        var fixedCombs = new List<InfTerm>();
        
        foreach (var term in uniquelyTypeds)
        {
            if (!FixCombTypes(term).Deconstruct(out var fixedComb, out error))
                return error;
            
            fixedCombs.Add(fixedComb);
        }
        
        var fixedAbses = new List<InfTerm>();
        
        foreach (var term in fixedCombs)
        {
            var fixedAbs = FixAbsVariables(term);
            fixedAbses.Add(fixedAbs);
        }
        
        if (!GenerateMappings(fixedAbses, nameGenerator, allBools).Deconstruct(out var mappings, out error))
            return error;

        return EvaluateMappings(fixedAbses, mappings);
    }

    protected internal override Result<List<Term>> BindTypes(List<InfTerm> input)
    {
        var output = new List<Term>();
        
        foreach (var term in input)
        {
            if (!BindTypes(term).Deconstruct(out var boundTerm, out var error))
                return error;
            
            output.Add(boundTerm);
        }
        
        return output;
    }

    private static Result<InfTerm> EvaluateMappings(InfTerm term, HashSet<(InfType, InfType)> mappings)
    {
        while (mappings.Count > 0)
        {
            if (!IterateMappings(term, ref mappings).Deconstruct(out var newTerm, out var error))
                return error;
            
            term = newTerm;
        }
        
        return term;
    }

    private static Result<List<InfTerm>> EvaluateMappings(List<InfTerm> terms, HashSet<(InfType, InfType)> mappings)
    {
        while (mappings.Count > 0)
        {
            if (!IterateMappings(terms, ref mappings).Deconstruct(out var newTerms, out var error))
                return error;
            
            terms = newTerms;
        }
        
        return terms;
    }

    private static Result<InfTerm> IterateMappings(InfTerm term, ref HashSet<(InfType, InfType)> mappings)
    {
        var map = mappings.First();
        mappings.Remove(map);
        
        var (left, right) = map;

        if (left == right)
            return term;

        var leftType = left switch
        {
            InfTyUnbound => 0,
            InfTyFixed   => 1,
            InfTyApp     => 2,
            InfTyVar     => 3,
            _            => throw new ArgumentOutOfRangeException()
        };
        
        var rightType = right switch
        {
            InfTyUnbound => 0,
            InfTyFixed   => 1,
            InfTyApp     => 2,
            InfTyVar     => 3,
            _            => throw new ArgumentOutOfRangeException()
        };
        
        if (leftType > rightType)
            (left, right) = (right, left);

        switch (left, right)
        {
            case (InfTyUnbound {Name: var unbound}, _):
            {
                if (right.CheckUnboundNameLoop(unbound).IsError(out var error))
                    return error;

                var newHashSet = new HashSet<(InfType, InfType)>();
                foreach (var (lelt, relt) in mappings)
                {
                    var newLeft = lelt.SubstituteType(unbound, right);
                    var newRight = relt.SubstituteType(unbound, right);
                    if (newLeft != newRight)
                        newHashSet.Add((newLeft, newRight));
                }
                
                mappings = newHashSet;
                
                return term.SubstituteType(unbound, right);
            }
            case (InfTyFixed leftFixed, InfTyFixed rightFixed):
                return $"Incompatible types: {leftFixed} and {rightFixed}";
            case (InfTyFixed leftFixed, _):
                mappings.Add((leftFixed.SingleUnbind(), right));
                return term;
            case (InfTyApp leftApp, InfTyApp rightApp):
                if (leftApp.Name != rightApp.Name)
                    return $"Incompatible types: {leftApp} and {rightApp}";
                
                if (leftApp.Args.Length != rightApp.Args.Length)
                    return $"Incompatible types: {leftApp} and {rightApp}";
                
                for (var i = 0; i < leftApp.Args.Length; i++)
                {
                    var leftArg = leftApp.Args[i];
                    var rightArg = rightApp.Args[i];
                    
                    if (leftArg == rightArg)
                        continue;
                    
                    mappings.Add((leftArg, rightArg));
                }
                
                return term;
            default:
                return $"Incompatible types: {left} and {right}";
        }
    }

    private static Result<List<InfTerm>> IterateMappings(List<InfTerm> terms, ref HashSet<(InfType, InfType)> mappings)
    {
        var map = mappings.First();
        mappings.Remove(map);
        
        var (left, right) = map;

        if (left == right)
            return terms;

        var leftType = left switch
        {
            InfTyUnbound => 0,
            InfTyFixed   => 1,
            InfTyApp     => 2,
            InfTyVar     => 3,
            _            => throw new ArgumentOutOfRangeException()
        };
        
        var rightType = right switch
        {
            InfTyUnbound => 0,
            InfTyFixed   => 1,
            InfTyApp     => 2,
            InfTyVar     => 3,
            _            => throw new ArgumentOutOfRangeException()
        };
        
        if (leftType > rightType)
            (left, right) = (right, left);

        switch (left, right)
        {
            case (InfTyUnbound {Name: var unbound}, _):
            {
                if (right.CheckUnboundNameLoop(unbound).IsError(out var error))
                    return error;

                var newHashSet = new HashSet<(InfType, InfType)>();
                foreach (var (lelt, relt) in mappings)
                {
                    var newLeft = lelt.SubstituteType(unbound, right);
                    var newRight = relt.SubstituteType(unbound, right);
                    if (newLeft != newRight)
                        newHashSet.Add((newLeft, newRight));
                }
                
                mappings = newHashSet;
                
                return terms.Select(term => term.SubstituteType(unbound, right)).ToList();
            }
            case (InfTyFixed leftFixed, InfTyFixed rightFixed):
                return $"Incompatible types: {leftFixed} and {rightFixed}";
            case (InfTyFixed leftFixed, _):
                mappings.Add((leftFixed.SingleUnbind(), right));
                return terms;
            case (InfTyApp leftApp, InfTyApp rightApp):
                if (leftApp.Name != rightApp.Name)
                    return $"Incompatible types: {leftApp} and {rightApp}";
                
                if (leftApp.Args.Length != rightApp.Args.Length)
                    return $"Incompatible types: {leftApp} and {rightApp}";
                
                for (var i = 0; i < leftApp.Args.Length; i++)
                {
                    var leftArg = leftApp.Args[i];
                    var rightArg = rightApp.Args[i];
                    
                    if (leftArg == rightArg)
                        continue;
                    
                    mappings.Add((leftArg, rightArg));
                }
                
                return terms;
            default:
                return $"Incompatible types: {left} and {right}";
        }
    }

    private Result<Term> BindTypes(InfTerm input)
    {
        var nicelyNamed = NicelyRename(input);
        return BindTerm(nicelyNamed);
    }
    
    private InfTerm NicelyRename(InfTerm term)
    {
        var usedTyVars = _kernel.TypeArities.Keys.ToHashSet();
        term.GetUsedTyVarNames(usedTyVars);
        var nameGenerator = new NameGenerator
        {
            ToAvoid = usedTyVars,
            FirstLetterConfig = new LetterConfig(true, false, false),
            LetterConfig = new LetterConfig(true, false, true)
        };
        return term.NicelyRename(nameGenerator, new Dictionary<string, string>());
    }
    
    private Result<Term> BindTerm(InfTerm term)
    {
        return term.BindTerm(_kernel);
    }

    private Result<InfTerm> FixCombTypes(InfTerm term)
    {
        return term.FixCombTypes();
    }
    
    private InfTerm UniquifyUnboundTypeNames(InfTerm term, NameGenerator generator)
    {
        return term.UniquifyUnboundTypeNames(generator);
    }
    
    private InfTerm FixAbsVariables(InfTerm term)
    {
        return term.FixAbsVariables(new Stack<string?>());
    }

    private Result<HashSet<(InfType, InfType)>> GenerateMappings(InfTerm term, NameGenerator nameGenerator)
    {
        var mappings = new HashSet<(InfType, InfType)>();
        
        if (term.GenerateMappings(_kernel, mappings, nameGenerator).IsError(out var error))
            return error;
        
        return mappings;
    }

    private Result<HashSet<(InfType, InfType)>> GenerateMappings(List<InfTerm> terms, NameGenerator nameGenerator, bool allBools)
    {
        var mappings = new HashSet<(InfType, InfType)>();
        var frees = new HashSet<(string name, InfType type)>();
        
        foreach (var term in terms)
        {
            if (term.GenerateMappings(_kernel, mappings, nameGenerator).IsError(out var error))
                return error;

            if (allBools)
            {
                var termType = term.TypeOf();
                mappings.Add((termType, InfType.FromType(_kernel.Bool)));
            }
            
            term.GetFrees(frees);
        }
        
        var freeMappings = new Dictionary<string, InfType>();
        
        foreach (var (name, type) in frees)
        {
            if (freeMappings.TryGetValue(name, out var otherType))
            {
                if (otherType == type)
                    continue;
                
                mappings.Add((otherType, type));
            }
            else
            {
                freeMappings[name] = type;
            }
        }
        
        return mappings;
    }
}