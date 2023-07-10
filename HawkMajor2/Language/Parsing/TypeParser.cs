using HawkMajor2.Language.Contexts;
using HawkMajor2.Language.Inference;
using HawkMajor2.Language.Inference.Types;
using HawkMajor2.Language.Lexing;
using Results;
using Valiant;

namespace HawkMajor2.Language.Parsing;

public class TypeParser
{
    public readonly TypeContext Context;
    public readonly TypeTypeInference TypeInference;
    public readonly Lexer Lexer;
    public TypeParser(Kernel kernel, Lexer lexer, TypeContext context)
    {
        Context = context;
        TypeInference = new TypeTypeInference(kernel);
        Lexer = lexer;
    }
    
    public Result<Type> Parse(string input)
    {
        Context.SetLexerConfig();
        
        Lexer.Reset(input);

        Lexer.MoveNext();
        
        if (!Context.Parse().Deconstruct(out var type, out var error)) 
            return error;
        
        return TypeInference.ApplyInference(type);
    }
}