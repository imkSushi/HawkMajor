using Results;
using Valiant;

namespace HawkMajor2.Language.Inference;

public class TheoremTypeInference : TypeInference<InfTheorem, Conjecture>
{
    private TermTypeInference _termTypeInference;
    
    public TheoremTypeInference(Kernel kernel)
    {
        _termTypeInference = new TermTypeInference(kernel);
    }
    
    public override Result<InfTheorem> PartialInference(InfTheorem input)
    {
        var terms = input.Premises.ToList();
        terms.Add(input.Conclusion);
        
        var result = _termTypeInference.PartialInference(terms, true);
        
        if (!result.Deconstruct(out var termsWithTypes, out var error))
            return error;

        var premises = termsWithTypes.Take(termsWithTypes.Count - 1);
        
        return new InfTheorem(termsWithTypes.Last(), premises.ToArray());
    }

    protected internal override Result<Conjecture> BindTypes(InfTheorem input)
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