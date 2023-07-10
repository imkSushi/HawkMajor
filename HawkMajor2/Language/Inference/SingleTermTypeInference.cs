using HawkMajor2.Language.Inference.Terms;
using HawkMajor2.Language.Inference.Types;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Inference;

public sealed class SingleTermTypeInference
{
    private TermTypeInference _termTypeInference;
    
    public SingleTermTypeInference(Kernel kernel)
    {
        _termTypeInference = new TermTypeInference(kernel);
    }
    
    public SingleTermTypeInference(TermTypeInference termTypeInference)
    {
        _termTypeInference = termTypeInference;
    }

    public Result<Term> ApplyInference(InfTerm input, List<Term>? previousTerms = null)
    {
        List<InfTerm>? partial;
        string? error;
        
        if (previousTerms == null)
        {
            if (!_termTypeInference.PartialInference(new List<InfTerm> {input}).Deconstruct(out partial, out error))
                return error;
        }
        else
        {
            if (!_termTypeInference.PartialInference(new List<InfTerm> {input}, false, previousTerms).Deconstruct(out partial, out error))
                return error;
        }
        
        if (!_termTypeInference.BindTypes(partial).Deconstruct(out var termWithTypes, out error))
            return error;
        
        return termWithTypes.First();
    }
}