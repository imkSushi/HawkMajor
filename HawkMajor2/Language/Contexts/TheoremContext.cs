using HawkMajor2.Language.Inference;
using HawkMajor2.Language.Inference.Terms;
using HawkMajor2.Language.Lexing;
using HawkMajor2.Language.Lexing.Tokens;
using Results;
using Valiant;

namespace HawkMajor2.Language.Contexts;

public sealed class TheoremContext : LexingContext<InfTheorem>
{
    public readonly TermContext TermContext;
    public readonly TypeContext TypeContext;

    public TheoremContext(Lexer lexer, Kernel kernel) : base(lexer)
    {
        TypeContext = new TypeContext(lexer, kernel);
        TermContext = new TermContext(lexer, kernel, TypeContext);
        
        var config = new LexerConfig();
        config.AddKeyword("|-");
        TermContext.AddParentRule("|-");
        config.AddKeyword(",");
        TermContext.AddParentRule(",");
        LexerConfig = config;
    }
    public override void SetLexerConfig()
    {
        Lexer.Context = LexerConfig;
    }

    protected override LexerConfig LexerConfig { get; }

    protected override Result<InfTheorem> ParseInContext()
    {
        var premises = new List<InfTerm>();

        string? error;
        
        if (Lexer.Current is not KeywordToken { Value: "|-" })
        {
            if (!TermContext.Parse().Deconstruct(out var premise, out error))
                return error;
            
            premises.Add(premise);

            while (Lexer.Current is KeywordToken { Value: "," })
            {
                Lexer.MoveNext();
                
                if (!TermContext.Parse().Deconstruct(out premise, out error))
                    return error;
                
                premises.Add(premise);
            }
            
            if (Lexer.Current is not KeywordToken { Value: "|-" })
                return "Expected |- before conclusion";
        }
        
        Lexer.MoveNext();
        
        if (!TermContext.Parse().Deconstruct(out var conclusion, out error))
            return error;
        
        return new InfTheorem(conclusion, premises.ToArray());
    }

    protected override Result<Func<Result<InfTheorem>>> PrefixIdentifierRule(string name)
    {
        throw new InvalidOperationException();
    }

    protected override Result<Func<InfTheorem, Result<InfTheorem>>, int> InfixIdentifierRule(string name)
    {
        throw new InvalidOperationException();
    }
}