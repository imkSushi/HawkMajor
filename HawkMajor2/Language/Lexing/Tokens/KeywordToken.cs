namespace HawkMajor2.Language.Lexing.Tokens;

public record KeywordToken(string Value, TokenData Data) : Token(Value, Data)
{
    public override string ToString()
    {
        return $"{Data} Keyword: {Value}";
    }
}