using HawkMajor2.Language.Inference.Terms;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Inference;

public sealed class SingleTermTypeInference : TypeInference<InfTerm, Term>
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
    
    public override Result<InfTerm> PartialInference(InfTerm input)
    {
        if (!_termTypeInference.PartialInference(new List<InfTerm> {input}).Deconstruct(out var termWithTypes, out var error))
            return error;
        
        return termWithTypes.First();
    }

    protected internal override Result<Term> BindTypes(InfTerm input)
    {
        if (!_termTypeInference.BindTypes(new List<InfTerm> {input}).Deconstruct(out var termWithTypes, out var error))
            return error;
        
        return termWithTypes.First();
    }
}