using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Inference;

public class TheoremTypeInference
{
    private TermTypeInference _termTypeInference;
    
    public TheoremTypeInference(Kernel kernel)
    {
        _termTypeInference = new TermTypeInference(kernel);
    }

    public  Result<Conjecture> FullInference(InfTheorem input)
    {
        return FullInference(input, new List<Term>());
    }

    public  Result<Conjecture> FullInference(InfTheorem input, List<Term> equivalentTerms)
    {
        if (!PartialInference(input, equivalentTerms).Deconstruct(out var partial, out var error))
            return error;
        
        return BindTypes(partial);
    }
    
    public Result<InfTheorem> PartialInference(InfTheorem input, List<Term> equivalentTerms)
    {
        var terms = input.Premises.ToList();
        terms.Add(input.Conclusion);
        
        var result = _termTypeInference.PartialInference(terms, true, equivalentTerms);
        
        if (!result.Deconstruct(out var termsWithTypes, out var error))
            return error;

        var premises = termsWithTypes.Take(termsWithTypes.Count - 1);
        
        return new InfTheorem(termsWithTypes.Last(), premises.ToArray());
    }

    private Result<Conjecture> BindTypes(InfTheorem input)
    {
        var terms = input.Premises.ToList();
        terms.Add(input.Conclusion);
        
        var result = _termTypeInference.BindTypes(terms);
        
        if (!result.Deconstruct(out var termsWithTypes, out var error))
            return error;
        
        var premises = termsWithTypes.Take(termsWithTypes.Count - 1);
        
        return new Conjecture(premises.ToArray(), termsWithTypes.Last());
    }
}