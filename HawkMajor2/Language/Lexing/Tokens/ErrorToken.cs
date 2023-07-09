namespace HawkMajor2.Language.Lexing.Tokens;

public record ErrorToken(string Message, TokenData Data) : Token(Message, Data)
{
    public override string ToString()
    {
        return $"{Data} Error: {Message}";
    }
}