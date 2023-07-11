using System.Diagnostics.CodeAnalysis;
using HawkMajor2.Language.Lexing.Tokens;
using Results;

namespace HawkMajor2.Language.Lexing;

public partial class ScriptLexer : Lexer
{
    public ScriptLexer(string expression) : base(expression)
    {
        
    }

    public string GenerateError(string message)
    {
        if (Current is ErrorToken err)
            return err.ToString();
        
        return $"{GetCurrentTokenData()} Error: {message}";
    }
    
    [InlineResult]
    public Result ExpectEndOfLine()
    {
        switch (Current)
        {
            case NewLineToken:
                MoveNext();
                return Result.Success;
            case KeywordToken {Value: ";"}:
                MoveNext();
                return Result.Success;
            case EndOfExpressionToken:
                return Result.Success;
            default:
                return GenerateError("Expected end of line");
        }
    }

    public void SkipNewLines()
    {
        while (Current is NewLineToken)
            MoveNext();
    }

    public Result ExpectKeyword(string keyword)
    {
        SkipNewLines();
        
        if (Current is not KeywordToken keywordToken)
            return GenerateError("Expected keyword");

        if (keywordToken.Value != keyword)
            return GenerateError($"Expected keyword {keyword}");
        
        MoveNext();
        return Result.Success;
    }
    

    [InlineResult("identifier")]
    public StringResult ExpectIdentifier()
    {
        return !ExpectIdentifierToken().Deconstruct(out var identifierToken, out var error) ? (false, error) : (true, identifierToken.Value);
    }

    public bool ExpectIdentifiers([MaybeNullWhen(false)] out string identifier1, [MaybeNullWhen(false)] out string identifier2, [MaybeNullWhen(true)] out string error)
    {
        if (!ExpectIdentifier(out identifier1, out error))
        {
            identifier2 = null;
            return false;
        }
        
        return ExpectIdentifier(out identifier2, out error);
    }

    public bool ExpectIdentifiers([MaybeNullWhen(false)] out string identifier1, [MaybeNullWhen(false)] out string identifier2, [MaybeNullWhen(false)] out string identifier3, [MaybeNullWhen(true)] out string error)
    {
        if (!ExpectIdentifier(out identifier1, out error))
        {
            identifier2 = null;
            identifier3 = null;
            return false;
        }
        
        if (!ExpectIdentifier(out identifier2, out error))
        {
            identifier3 = null;
            return false;
        }

        return ExpectIdentifier(out identifier3, out error);
    }
    
    public Result<IdentifierToken> ExpectIdentifierToken()
    {
        SkipNewLines();

        if (Current is KeywordToken { Value: "\"" })
            return GetQuotedIdentifier();

        if (Current is not IdentifierToken identifier)
            return GenerateError("Expected identifier");
        
        MoveNext();
        return identifier;
    }

    public Result<IdentifierToken> GetQuotedIdentifier()
    {
        var data = GetCurrentTokenData();

        if (!ParseQuotedString(out var identifier, out var error))
            return error;
        
        return new IdentifierToken(identifier, data);
    }

    public Result Identifier(string identifier)
    {
        SkipNewLines();
        
        if (Current is not IdentifierToken identifierToken)
            return GenerateError("Expected identifier");

        if (identifierToken.Value != identifier)
            return GenerateError($"Expected identifier {identifier}");
        
        MoveNext();
        return Result.Success;
    }

    public Result<KeywordToken> ExpectKeyword()
    {
        SkipNewLines();
        
        if (Current is not KeywordToken keyword)
            return GenerateError("Expected keyword");
        
        MoveNext();
        return keyword;
    }

    [InlineResult("identifier")]
    public StringResult ParseQuotedString()
    {
        if (ExpectKeyword("\"").IsError(out var error))
            return (false, error);

        var output = new List<string>();
        while (Current is not KeywordToken { Value: "\"" })
        {
            switch (Current)
            {
                case ErrorToken { Message: var errorMessage }:
                    return (false, GenerateError(errorMessage));
                case EndOfExpressionToken:
                    return (false, GenerateError("Unexpected end of expression"));
                default:
                    output.Add(Current.Value);
                    break;
            }

            MoveNext();
        }

        MoveNext();
        
        return (true, string.Join(' ', output));
    }
}