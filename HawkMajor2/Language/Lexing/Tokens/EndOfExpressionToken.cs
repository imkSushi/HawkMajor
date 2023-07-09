namespace HawkMajor2.Language.Lexing.Tokens;

public record EndOfExpressionToken(TokenData Data) : Token("\0", Data)
{
    public override string ToString()
    {
        return $"{Data} End of expression";
    }
}