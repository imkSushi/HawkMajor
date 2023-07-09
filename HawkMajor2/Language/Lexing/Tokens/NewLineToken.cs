namespace HawkMajor2.Language.Lexing.Tokens;

public record NewLineToken(TokenData Data) : Token("\n", Data) {
    
}