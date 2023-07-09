using HawkMajor2.Language.Contexts;
using HawkMajor2.Language.Inference;
using HawkMajor2.Language.Inference.Types;
using HawkMajor2.Language.Lexing;
using Valiant;

namespace HawkMajor2.Language.Parsing;

public class TypeParser : Parser<TypeContext, TypeTypeInference, InfType, Type>
{
    public TypeParser(Kernel kernel, Lexer lexer, TypeContext context) : base(context, new TypeTypeInference(kernel),
        lexer)
    {
        
    }
    
    public TypeParser(Kernel kernel, Lexer lexer) : this(kernel, lexer, new TypeContext(lexer, kernel))
    {
        
    }
}