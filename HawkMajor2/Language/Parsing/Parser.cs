using HawkMajor2.Language.Contexts;
using HawkMajor2.Language.Inference;
using HawkMajor2.Language.Lexing;
using Results;

namespace HawkMajor2.Language.Parsing;

public class Parser<TContext, TTypeInference, TMid, TOut> where TContext : Context<TMid> where TTypeInference : TypeInference<TMid, TOut>
{
    public readonly TContext Context;
    public readonly TTypeInference TypeInference;
    public readonly Lexer Lexer;
    public Parser(TContext context, TTypeInference typeInference, Lexer lexer)
    {
        Context = context;
        TypeInference = typeInference;
        Lexer = lexer;
    }
    
    public virtual Result<TOut> Parse(string input)
    {
        Context.SetLexerConfig();
        
        Lexer.Reset(input);

        Lexer.MoveNext();
        
        if (!Context.Parse().Deconstruct(out var mid, out var error)) 
            return error;
        
        return TypeInference.FullInference(mid);
    }
}