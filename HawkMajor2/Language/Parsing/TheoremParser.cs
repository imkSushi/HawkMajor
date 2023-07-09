using HawkMajor2.Language.Contexts;
using HawkMajor2.Language.Inference;
using HawkMajor2.Language.Lexing;

namespace HawkMajor2.Language.Parsing;

public class TheoremParser : Parser<TheoremContext, TheoremTypeInference, InfTheorem, Conjecture>
{
    public TheoremParser(TheoremContext context, TheoremTypeInference typeInference, Lexer lexer) : base(context,
        typeInference, lexer)
    {
        
    }
}