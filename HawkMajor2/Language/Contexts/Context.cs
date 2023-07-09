using HawkMajor2.Language.Lexing;
using Results;

namespace HawkMajor2.Language.Contexts;

public abstract class Context<T>
{
    protected Lexer Lexer;
    
    public Context(Lexer lexer)
    {
        Lexer = lexer;
    }

    public abstract Result<T> Parse();

    public abstract void SetLexerConfig();
}