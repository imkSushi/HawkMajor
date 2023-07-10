using HawkMajor2.Language.Inference.Types;
using HawkMajor2.NameGenerators;
using Results;
using Valiant;

namespace HawkMajor2.Language.Inference;

public class TypeTypeInference
{
    
    private Kernel _kernel;
    
    public TypeTypeInference(Kernel kernel)
    {
        _kernel = kernel;
    }
    
    public virtual Result<Type> ApplyInference(InfType input)
    {
        var unboundTypeNames = new Dictionary<string, string>();
        var nameGenerator = new NameGenerator
        {
            FirstLetterConfig = new LetterConfig(true, false, false),
            LetterConfig = new LetterConfig(true, false, true)
        };
        
        return input.BindTypes(_kernel, unboundTypeNames, nameGenerator);
    }
}