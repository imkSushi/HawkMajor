namespace HawkMajor2.Language.Lexing.Tokens;

public record struct TokenData(int Index, int Line, int Column)
{
    public override string ToString()
    {
        return $"({Index}, {Line}:{Column})";
    }
}