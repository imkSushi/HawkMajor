using HawkMajor2.Language.Contexts;
using HawkMajor2.Language.Inference;
using HawkMajor2.Language.Lexing;
using Results;
using Valiant.Terms;

namespace HawkMajor2.Language.Parsing;

public class TheoremParser
{
    public readonly TheoremContext Context;
    public readonly TheoremTypeInference TypeInference;
    public readonly Lexer Lexer;
    public TheoremParser(TheoremContext context, TheoremTypeInference typeInference, Lexer lexer)
    {
        Context = context;
        TypeInference = typeInference;
        Lexer = lexer;
    }
    
    public virtual Result<Conjecture> Parse(string input, List<Term> previousTerms)
    {
        Context.SetLexerConfig();
        
        Lexer.Reset(input);

        Lexer.MoveNext();
        
        if (!Context.Parse().Deconstruct(out var mid, out var error)) 
            return error;
        
        return TypeInference.FullInference(mid, previousTerms);
    }
}