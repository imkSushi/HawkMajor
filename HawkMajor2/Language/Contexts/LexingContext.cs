using HawkMajor2.Language.Lexing;
using HawkMajor2.Language.Lexing.Tokens;
using Results;

namespace HawkMajor2.Language.Contexts;

public abstract class LexingContext<T> : Context<T>
{
    public LexingContext(Lexer lexer) : base(lexer) { }
    
    protected abstract LexerConfig LexerConfig { get; }
    
    public override Result<T> Parse()
    {
        var oldCtx = Lexer.Context;
        Lexer.Context = LexerConfig;
        var result = ParseInContext();
        Lexer.Context = oldCtx;
        return result;
    }

    protected virtual Result<T> ParseInContext()
    {
        var lexerState = Lexer.GetCurrentTokenData();

        var output = ParsePrecedence(int.MaxValue - 1);
        
        if (output.Deconstruct(out var result, out var error))
            return result;
        
        Lexer.SetTo(lexerState);
        return error;
    }

    protected virtual Result<T> ParsePrecedence(int precedence)
    {
        if (!GetPrefixRule().Deconstruct(out var prefixRule, out var error))
            return error;
        
        if (!prefixRule().Deconstruct(out var output, out error))
            return error;
        
        while (precedence >= GetPrecedence())
        {
            if (!GetInfixRule().Deconstruct(out var infixRule, out _, out error))
                return error;
            
            if (!infixRule(output).Deconstruct(out output, out error))
                return error;
        }
        
        return output;
    }
    
    protected virtual int GetPrecedence()
    {
        if (GetInfixRule().IsSuccess(out _, out var precedence))
            return precedence;
        
        return int.MaxValue;
    }
    
    protected virtual Result<Func<Result<T>>> GetPrefixRule()
    {
        var token = Lexer.Current;
        switch (token)
        {
            case IdentifierToken { Value: var name }:
                return PrefixIdentifierRule(name);
            case ErrorToken { Message: var error }:
                return error;
            case EndOfExpressionToken:
                return "Unexpected end of expression";
            case KeywordToken { Value: var keyword }:
                return PrefixKeywordRule(keyword);
            default:
                return $"Unrecognised token: {token}";
        }
    }

    protected virtual Result<Func<Result<T>>> PrefixKeywordRule(string keyword)
    {
        if (!Rules.TryGetValue(keyword, out var rule))
            return $"Unexpected keyword: {keyword}";
        
        if (rule.prefixRule is ActivePrefixRule activeRule)
            return activeRule.Rule;
        
        return $"Unexpected keyword: {keyword}";
    }

    protected abstract Result<Func<Result<T>>> PrefixIdentifierRule(string name);
    
    protected virtual Result<Func<T, Result<T>>, int> GetInfixRule()
    {
        var token = Lexer.Current;
        switch (token)
        {
            case IdentifierToken { Value: var name }:
                return InfixIdentifierRule(name);
            case ErrorToken { Message: var error }:
                return error;
            case EndOfExpressionToken:
                return EndOfExpressionRule();
            case KeywordToken { Value: var keyword }:
                return InfixKeywordRule(keyword);
            default:
                return $"Unrecognised token: {token}";
        }
    }
    
    protected virtual Result<Func<T, Result<T>>, int> InfixKeywordRule(string keyword)
    {
        if (!Rules.TryGetValue(keyword, out var rule))
            return $"Unexpected keyword: {keyword}";
        
        if (rule.infixRule is ActiveInfixRule activeRule)
            return (activeRule.Rule, activeRule.Precedence);

        return $"Unexpected keyword: {keyword}";
    }
    
    protected virtual Result<Func<T, Result<T>>, int> EndOfExpressionRule()
    {
        return (_ => "Unexpected end of expression", int.MaxValue);
    }
    
    protected abstract Result<Func<T, Result<T>>, int> InfixIdentifierRule(string name);

    protected Dictionary<string, (PrefixRule prefixRule, InfixRule infixRule)> Rules = new();
    protected HashSet<string> ParentRules = new();

    public virtual Result AddParentRule(string symbol, bool canInterruptIdentifier = true)
    {
        var currentInterrupt = LexerConfig.CanInterruptIdentifier(symbol);
        if (currentInterrupt == canInterruptIdentifier)
            return Result.Success;

        if (currentInterrupt != null) 
            return "Cannot change whether a keyword can interrupt an identifier";
        
        LexerConfig.AddKeyword(symbol, canInterruptIdentifier);
        ParentRules.Add(symbol);

        return Result.Success;
    }
    
    public virtual Result RemoveParentRule(string symbol)
    {
        if (!ParentRules.Remove(symbol))
            return "Rule does not exist";
        
        if (Rules.ContainsKey(symbol))
            return Result.Success;
        
        LexerConfig.RemoveKeyword(symbol);
        
        return Result.Success;
    }

    protected abstract record PrefixRule;
    protected sealed record PrefixNoRule : PrefixRule;
    protected abstract record ActivePrefixRule(Func<Result<T>> Rule) : PrefixRule;
    protected abstract record NoOverwritePrefixRule(Func<Result<T>> Rule) : ActivePrefixRule(Rule);
    protected sealed record PrefixBuiltInRule(Func<Result<T>> Rule) : ActivePrefixRule(Rule);
    protected abstract record SetPrefixRule(Func<Result<T>> Rule) : NoOverwritePrefixRule(Rule);
    protected sealed record PrefixBuiltInDummyRule : NoOverwritePrefixRule
    {
        public PrefixBuiltInDummyRule(string symbol) : base(() => $"Unexpected keyword: {symbol}") { }
    }

    protected abstract record InfixRule;
    protected sealed record InfixNoRule : InfixRule;
    protected abstract record ActiveInfixRule(Func<T, Result<T>> Rule, int Precedence) : InfixRule;
    protected abstract record NoOverwriteInfixRule(Func<T, Result<T>> Rule, int Precedence) : ActiveInfixRule(Rule, Precedence);
    protected sealed record InfixBuiltInRule(Func<T, Result<T>> Rule, int Precedence) : ActiveInfixRule(Rule, Precedence);
    protected abstract record SetInfixRule(Func<T, Result<T>> Rule, int Precedence) : NoOverwriteInfixRule(Rule, Precedence);
    protected sealed record InfixBuiltInDummyRule : NoOverwriteInfixRule
    {
        public InfixBuiltInDummyRule(string symbol) : base(_ => $"Unexpected keyword: {symbol}", int.MaxValue) { }
    }
}