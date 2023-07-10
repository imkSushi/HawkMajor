using HawkMajor2.Language.Contexts;
using HawkMajor2.Language.Inference;
using HawkMajor2.Language.Inference.Terms;
using HawkMajor2.Language.Lexing;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Parsing;

public class TermParser
{
    public TermParser(Kernel kernel, Lexer lexer, TermContext context)
    {
        Context = context;
        TypeInference = new SingleTermTypeInference(kernel);
        Lexer = lexer;
    }
    
    public TermParser(Kernel kernel, Lexer lexer, TypeContext typeContext) : this(kernel, lexer, new TermContext(lexer, kernel, typeContext))
    {
        
    }
    
    public TermParser(Kernel kernel, Lexer lexer) : this(kernel, lexer, new TypeContext(lexer, kernel))
    {
        
    }
    
    public readonly TermContext Context;
    public readonly SingleTermTypeInference TypeInference;
    public readonly Lexer Lexer;
    
    public virtual Result<Term> Parse(string input, List<Term>? previousTerms = null)
    {
        Context.SetLexerConfig();
        
        Lexer.Reset(input);

        Lexer.MoveNext();
        
        if (!Context.Parse().Deconstruct(out var mid, out var error)) 
            return error;
        
        return previousTerms == null ? TypeInference.ApplyInference(mid) : TypeInference.ApplyInference(mid, previousTerms);
    }
}