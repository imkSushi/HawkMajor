namespace HawkMajor2.Language.Lexing.Tokens;

public record IdentifierToken(string Value, TokenData Data) : Token(Value, Data)
{
    public override string ToString()
    {
        return $"{Data} Identifier: {Value}";
    }
}