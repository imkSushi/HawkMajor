using Results;

namespace HawkMajor2.Language.Inference;

public abstract class TypeInference<TIn, TOut>
{
    public abstract Result<TIn> PartialInference(TIn input);

    public virtual Result<TOut> FullInference(TIn input)
    {
        if (!PartialInference(input).Deconstruct(out var partial, out var error))
            return error;
        
        return BindTypes(partial);
    }

    protected internal abstract Result<TOut> BindTypes(TIn input);
}