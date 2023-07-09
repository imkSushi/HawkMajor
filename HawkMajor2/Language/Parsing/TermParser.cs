using HawkMajor2.Language.Contexts;
using HawkMajor2.Language.Inference;
using HawkMajor2.Language.Inference.Terms;
using HawkMajor2.Language.Lexing;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Parsing;

public class TermParser : Parser<TermContext, SingleTermTypeInference, InfTerm, Term>
{
    public TermParser(Kernel kernel, Lexer lexer, TermContext context) : base(context, new SingleTermTypeInference(kernel), lexer)
    {
        
    }
    
    public TermParser(Kernel kernel, Lexer lexer, TypeContext typeContext) : this(kernel, lexer, new TermContext(lexer, kernel, typeContext))
    {
        
    }
    
    public TermParser(Kernel kernel, Lexer lexer) : this(kernel, lexer, new TypeContext(lexer, kernel))
    {
        
    }
}