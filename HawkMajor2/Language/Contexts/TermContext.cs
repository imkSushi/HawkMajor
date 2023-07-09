using HawkMajor2.Language.Inference.Terms;
using HawkMajor2.Language.Inference.Types;
using HawkMajor2.Language.Lexing;
using HawkMajor2.Language.Lexing.Tokens;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Contexts;

public class TermContext : LexingContext<InfTerm>
{
    public TypeContext TypeContext { get; }
    public Kernel Kernel { get; }
    
    public TermContext(Lexer lexer, Kernel kernel, TypeContext typeContext) : base(lexer)
    {
        Kernel = kernel;
        TypeContext = typeContext;
        
        var config = new LexerConfig();
        config.AddKeyword(":");
        TypeContext.AddParentRule(":");
        config.AddKeyword("(");
        TypeContext.AddParentRule("(");
        config.AddKeyword("\\");
        TypeContext.AddParentRule("\\");
        config.AddKeyword(".");
        TypeContext.AddParentRule(".");
        config.AddKeyword(")");
        TypeContext.AddParentRule(")");
        LexerConfig = config;

        Rules[":"] = (new PrefixNoRule(), new InfixBuiltInDummyRule(":"));
        Rules["("] = (new PrefixBuiltInRule(Grouping), new InfixBuiltInRule(Grouping, 0));
        Rules["\\"] = (new PrefixBuiltInRule(Lambda), new InfixBuiltInRule(Lambda, 0));
        Rules["."] = (new PrefixBuiltInDummyRule("."), new InfixBuiltInDummyRule("."));
        Rules[")"] = (new PrefixBuiltInDummyRule(")"), new InfixBuiltInDummyRule(")"));
    }
    public override void SetLexerConfig()
    {
        Lexer.Context = LexerConfig;
    }

    protected override LexerConfig LexerConfig { get; }
    protected override Result<Func<Result<InfTerm>>> PrefixIdentifierRule(string name)
    {
        return new Result<Func<Result<InfTerm>>>(() => Identifier(name, true));
    }

    private Result<InfTerm> Identifier(string name, bool allowConstant)
    {
        Lexer.MoveNext();
        
        InfType type;
        
        if (Lexer.Current is KeywordToken { Value: ":" })
        {
            Lexer.MoveNext();
            
            if (!TypeContext.Parse().Deconstruct(out var actualType, out var error))
                return error;
            
            type = actualType;
        }
        else
        {
            type = new InfTyUnbound("a");
        }
        
        if (allowConstant && Kernel.ConstantTypes.ContainsKey(name))
        {
            return new InfConst(name, type);
        }
        
        return new InfUnbound(name, type);
    }

    protected override Result<Func<InfTerm, Result<InfTerm>>, int> InfixIdentifierRule(string name)
    {
        return (left => Identifier(left, name), 0);
    }

    private Result<InfTerm> Identifier(InfTerm left, string name)
    {
        if (!Identifier(name, true).Deconstruct(out var right, out var error))
            return error;
        
        return new InfApp(left, right);
    }

    private Result<InfTerm> Grouping()
    {
        Lexer.MoveNext();
        
        if (!ParsePrecedence(int.MaxValue - 1).Deconstruct(out var term, out var error))
            return error;
        
        if (Lexer.Current is not KeywordToken { Value: ")" })
            return $"Expected ')' but got {Lexer.Current}";
        
        Lexer.MoveNext();
        
        return term;
    }

    private Result<InfTerm> Grouping(InfTerm left)
    {
        if (!Grouping().Deconstruct(out var right, out var error))
            return error;
        
        return new InfApp(left, right);
    }

    private Result<InfTerm> Lambda(string? modifier, int precedence)
    {
        if (modifier != null && !Kernel.ConstantTypes.ContainsKey(modifier))
            return $"Unknown modifier '{modifier}'";
        
        Lexer.MoveNext();
        
        var start = Lexer.GetCurrentTokenData();
        
        var parameters = new List<(string Name, InfType Type)>();

        string? error;
        
        while (Lexer.Current is IdentifierToken { Value: var name })
        {
            if (!Identifier(name, false).Deconstruct(out var parameter, out error))
                return error;
            
            var parameterVar = (InfVar) parameter;

            parameters.Add((name, parameterVar.Type));
        }
        
        if (Lexer.Current is not KeywordToken { Value: "." })
        {
            if (modifier == null)
                return $"Expected '.' but got {Lexer.Current}";
            
            Lexer.SetTo(start);
            
            var infConst = new InfConst(modifier, new InfTyUnbound("a"));
            
            if (!ParsePrecedence(precedence).Deconstruct(out var term, out error))
                return error;
            
            return new InfApp(infConst, term);
        }

        Lexer.MoveNext();
        
        if (!ParsePrecedence(int.MaxValue - 1).Deconstruct(out var body, out error))
            return error;

        InfTerm lambda;
        
        if (parameters.Count == 0)
        {
            lambda = new InfAbs(new InfTyUnbound("a"), null, body);
        }
        else
        {
            lambda = body;
            
            for (var i = parameters.Count - 1; i >= 0; i--)
            {
                var (name, type) = parameters[i];
                
                lambda = new InfAbs(type, name, lambda);
            }
        }
        
        if (modifier != null)
        {
            var infConst = new InfConst(modifier, new InfTyUnbound("a"));
            
            lambda = new InfApp(infConst, lambda);
        }
        
        return lambda;
    }

    private Result<InfTerm> Lambda()
    {
        return Lambda(null, int.MaxValue - 2);
    }
    
    private Result<InfTerm> Lambda(InfTerm left)
    {
        var lambda = Lambda();
        
        if (!lambda.Deconstruct(out var right, out var error))
            return error;
        
        return new InfApp(left, right);
    }
    
    private Result<InfTerm> Lambda(InfTerm left, string modifier, int precedence)
    {
        var lambda = Lambda(modifier, precedence);
        
        if (!lambda.Deconstruct(out var right, out var error))
            return error;
        
        return new InfApp(left, right);
    }

    public override Result AddParentRule(string symbol, bool canInterruptIdentifier = true)
    {
        return base.AddParentRule(symbol, canInterruptIdentifier) && TypeContext.AddParentRule(symbol, canInterruptIdentifier);
    }
    
    public override Result RemoveParentRule(string symbol)
    {
        return base.RemoveParentRule(symbol) && TypeContext.RemoveParentRule(symbol);
    }
    
    public Result AddMacroRule(string symbol, Term term, bool canInterruptIdentifier)
    {
        if (Rules.TryGetValue(symbol, out var rule))
        {
            if (rule.prefixRule is NoOverwritePrefixRule)
                return $"Prefix rule for '{symbol}' already exists";
            
            if (rule.infixRule is NoOverwriteInfixRule)
                return $"Infix rule for '{symbol}' already exists";
            
            if (LexerConfig.CanInterruptIdentifier(symbol) != canInterruptIdentifier)
                return $"Infix rule for '{symbol}' already exists with different interrupt identifier setting";
        }
        else
        {
            if (!ParentRules.Contains(symbol))
            {
                if (TypeContext.AddParentRule(symbol, canInterruptIdentifier).IsError(out var error))
                    return error;
            }
            
            LexerConfig.AddKeyword(symbol, canInterruptIdentifier);
        }
        
        Rules[symbol] = (new MacroUsedPrefixRule(() => ParseMacro(term)), new MacroUsedInfixRule(left => ParseMacro(term, left), 0));

        return Result.Success;
    }
    
    public Result RemoveMacroRule(string symbol)
    {
        if (!Rules.TryGetValue(symbol, out var rule))
            return $"Rules for '{symbol}' does not exist";
        
        if (rule.prefixRule is not MacroUsedPrefixRule)
            return $"Rule for '{symbol}' is not a macro rule";
        
        if (rule.infixRule is not MacroUsedInfixRule)
            return $"Rule for '{symbol}' is not a macro rule";
        
        if (!ParentRules.Contains(symbol))
        {
            if (TypeContext.RemoveParentRule(symbol).IsError(out var error))
                return error;
        }
        
        Rules[symbol] = (new PrefixNoRule(), new InfixNoRule());
        LexerConfig.RemoveKeyword(symbol);

        return Result.Success;
    }
    
    public Result<InfTerm> ParseMacro(Term term)
    {
        Lexer.MoveNext();
        
        return new InfFixed(term);
    }
    
    public Result<InfTerm> ParseMacro(Term term, InfTerm left)
    {
        return new InfApp(left, new InfFixed(term));
    }
    
    private sealed record MacroUsedPrefixRule(Func<Result<InfTerm>> Rule) : SetPrefixRule(Rule);
    private sealed record MacroUsedInfixRule(Func<InfTerm, Result<InfTerm>> Rule, int Precedence) : SetInfixRule(Rule,
        Precedence);
    private sealed record InfixUsedInfixRule(Func<InfTerm, Result<InfTerm>> Rule, int Precedence) : SetInfixRule(Rule,
        Precedence);
    private sealed record PrefixUsedPrefixRule(Func<Result<InfTerm>> Rule) : SetPrefixRule(Rule);
    private sealed record PostfixUsedInfixRule(Func<InfTerm, Result<InfTerm>> Rule, int Precedence) : SetInfixRule(Rule,
        Precedence);
    private sealed record LambdaUsedInfixRule(Func<InfTerm, Result<InfTerm>> Rule, int Precedence) : SetInfixRule(Rule,
        Precedence);
    private sealed record LambdaUsedPrefixRule(Func<Result<InfTerm>> Rule) : SetPrefixRule(Rule);
    
    public Result AddInfixRule(string symbol, string constant, int precedence, bool leftAssociative, bool canInterruptIdentifier, bool verify = true)
    {
        if (verify)
        {
            if (!Kernel.ConstantTypes.ContainsKey(constant))
                return $"Unknown constant '{constant}'";
        }

        PrefixRule prefixRule;
        
        if (Rules.TryGetValue(symbol, out var rule))
        {
            if (rule.infixRule is NoOverwriteInfixRule)
                return $"Infix rule for '{symbol}' already exists";
            
            prefixRule = rule.prefixRule;
            
            if (LexerConfig.CanInterruptIdentifier(symbol) != canInterruptIdentifier)
                return $"Infix rule for '{symbol}' already exists with different interrupt identifier setting";
        }
        else
        {
            prefixRule = new PrefixNoRule();
            
            if (!ParentRules.Contains(symbol))
            {
                if (TypeContext.AddParentRule(symbol, canInterruptIdentifier).IsError(out var error))
                    return error;
            }
            
            LexerConfig.AddKeyword(symbol, canInterruptIdentifier);
        }
        
        var parsePrecedence = leftAssociative ? precedence : precedence + 1;
        
        Rules[symbol] = (prefixRule, new InfixUsedInfixRule(left => ParseInfixRule(constant, left, parsePrecedence), precedence));

        return Result.Success;
    }
    
    public Result RemoveInfixRule(string symbol)
    {
        if (!Rules.TryGetValue(symbol, out var rule))
            return $"Infix rule for '{symbol}' does not exist";
        
        if (rule.infixRule is InfixNoRule)
            return $"Infix rule for '{symbol}' does not exist";
        
        if (rule.infixRule is not InfixUsedInfixRule)
            return $"Rule for '{symbol}' is not an infix rule";

        if (rule.prefixRule is PrefixNoRule)
        {
            if (!ParentRules.Contains(symbol))
            {
                if (TypeContext.RemoveParentRule(symbol).IsError(out var error))
                    return error;
                
                LexerConfig.RemoveKeyword(symbol);
            }
            Rules.Remove(symbol);
        }
        else
        {
            Rules[symbol] = (rule.prefixRule, new InfixNoRule());
        }

        return Result.Success;
    }

    public Result<InfTerm> ParseInfixRule(string constant, InfTerm left, int precedence)
    {
        var infConst = new InfConst(constant, new InfTyUnbound("a"));
        
        Lexer.MoveNext();
        if (!ParsePrecedence(precedence).Deconstruct(out var right, out var error))
            return error;
        
        return new InfApp(new InfApp(infConst, left), right);
    }
    
    public Result AddLambdaRule(string symbol, string constant, int precedence, bool canInterruptIdentifier, bool verify = true)
    {
        if (verify)
        {
            if (!Kernel.ConstantTypes.ContainsKey(constant))
                return $"Unknown constant '{constant}'";
        }

        if (Rules.TryGetValue(symbol, out var rule))
        {
            if (rule.prefixRule is NoOverwritePrefixRule)
                return $"Prefix rule for '{symbol}' already exists";
            
            if (rule.infixRule is NoOverwriteInfixRule)
                return $"Infix rule for '{symbol}' already exists";
            
            if (LexerConfig.CanInterruptIdentifier(symbol) != canInterruptIdentifier)
                return $"Infix rule for '{symbol}' already exists with different interrupt identifier setting";
        }
        else
        {
            if (!ParentRules.Contains(symbol))
            {
                if (TypeContext.AddParentRule(symbol, canInterruptIdentifier).IsError(out var error))
                    return error;
            }
            
            LexerConfig.AddKeyword(symbol, canInterruptIdentifier);
        }
        
        Rules[symbol] = (new LambdaUsedPrefixRule(() => Lambda(constant, precedence)), new LambdaUsedInfixRule(left => Lambda(left, constant, precedence), precedence));

        return Result.Success;
    }
    
    public Result RemoveLambdaRule(string symbol)
    {
        if (!Rules.TryGetValue(symbol, out var rule))
            return $"Rules for '{symbol}' does not exist";
        
        if (rule.prefixRule is not LambdaUsedPrefixRule)
            return $"Rule for '{symbol}' is not a lambda rule";
        
        if (rule.infixRule is not LambdaUsedInfixRule)
            return $"Rule for '{symbol}' is not a lambda rule";
        
        if (!ParentRules.Contains(symbol))
        {
            if (TypeContext.RemoveParentRule(symbol).IsError(out var error))
                return error;
            
            LexerConfig.RemoveKeyword(symbol);
        }
        
        Rules[symbol] = (new PrefixNoRule(), new InfixNoRule());

        return Result.Success;
    }
    
    public Result AddPrefixRule(string symbol, string constant, int precedence, bool canInterruptIdentifier, bool verify = true)
    {
        if (verify)
        {
            if (!Kernel.ConstantTypes.ContainsKey(constant))
                return $"Unknown constant '{constant}'";
        }
        
        InfixRule infixRule;

        if (Rules.TryGetValue(symbol, out var rule))
        {
            if (rule.prefixRule is NoOverwritePrefixRule)
                return $"Prefix rule for '{symbol}' already exists";
            
            if (LexerConfig.CanInterruptIdentifier(symbol) != canInterruptIdentifier)
                return $"Infix rule for '{symbol}' already exists with different interrupt identifier setting";
            
            infixRule = rule.infixRule;
        }
        else
        {
            if (!ParentRules.Contains(symbol))
            {
                if (TypeContext.AddParentRule(symbol, canInterruptIdentifier).IsError(out var error))
                    return error;
            }
            
            LexerConfig.AddKeyword(symbol, canInterruptIdentifier);
            
            infixRule = new InfixNoRule();
        }
        
        Rules[symbol] = (new PrefixUsedPrefixRule(() => ParsePrefixRule(constant, precedence)), infixRule);

        return Result.Success;
    }
    
    public Result RemovePrefixRule(string symbol)
    {
        if (!Rules.TryGetValue(symbol, out var rule))
            return $"Rules for '{symbol}' does not exist";
        
        if (rule.prefixRule is not PrefixUsedPrefixRule)
            return $"Rule for '{symbol}' is not a prefix rule";
        
        if (rule.infixRule is not NoOverwriteInfixRule)
            return $"Rule for '{symbol}' is not a prefix rule";
        
        if (rule.infixRule is InfixNoRule)
        {
            if (!ParentRules.Contains(symbol))
            {
                if (TypeContext.RemoveParentRule(symbol).IsError(out var error))
                    return error;
                Rules.Remove(symbol);
            }
            
            LexerConfig.RemoveKeyword(symbol);
        }
        else
        {
            Rules[symbol] = (new PrefixNoRule(), rule.infixRule);
        }

        return Result.Success;
    }
    
    public Result<InfTerm> ParsePrefixRule(string constant, int precedence)
    {
        if (!Kernel.ConstantTypes.TryGetValue(constant, out _))
            return $"Unknown constant '{constant}'";
        
        Lexer.MoveNext();
        if (!ParsePrecedence(precedence).Deconstruct(out var term, out var error))
            return error;
        
        return new InfApp(new InfConst(constant, new InfTyUnbound("a")), term);
    }
    
    public Result AddPostfixRule(string symbol, string constant, int precedence, bool canInterruptIdentifier, bool verify = true)
    {
        if (verify)
        {
            if (!Kernel.ConstantTypes.ContainsKey(constant))
                return $"Unknown constant '{constant}'";
        }
        
        PrefixRule prefixRule;

        if (Rules.TryGetValue(symbol, out var rule))
        {
            if (rule.infixRule is NoOverwriteInfixRule)
                return $"Infix rule for '{symbol}' already exists";
            
            if (LexerConfig.CanInterruptIdentifier(symbol) != canInterruptIdentifier)
                return $"Infix rule for '{symbol}' already exists with different interrupt identifier setting";
            
            prefixRule = rule.prefixRule;
        }
        else
        {
            if (!ParentRules.Contains(symbol))
            {
                if (TypeContext.AddParentRule(symbol, canInterruptIdentifier).IsError(out var error))
                    return error;
            }
            
            LexerConfig.AddKeyword(symbol, canInterruptIdentifier);
            
            prefixRule = new PrefixNoRule();
        }
        
        Rules[symbol] = (prefixRule, new PostfixUsedInfixRule(left => ParsePostfixRule(left, constant), precedence));

        return Result.Success;
    }
    
    public Result RemovePostfixRule(string symbol)
    {
        if (!Rules.TryGetValue(symbol, out var rule))
            return $"Rules for '{symbol}' does not exist";
        
        if (rule.infixRule is not PostfixUsedInfixRule)
            return $"Rule for '{symbol}' is not a postfix rule";
        
        if (rule.prefixRule is PrefixNoRule)
        {
            if (!ParentRules.Contains(symbol))
            {
                if (TypeContext.RemoveParentRule(symbol).IsError(out var error))
                    return error;
                LexerConfig.RemoveKeyword(symbol);
            }
            Rules.Remove(symbol);
        }
        else
        {
            Rules[symbol] = (rule.prefixRule, new InfixNoRule());
        }

        return Result.Success;
    }
    
    public Result<InfTerm> ParsePostfixRule(InfTerm left, string constant)
    {
        if (!Kernel.ConstantTypes.TryGetValue(constant, out _))
            return $"Unknown constant '{constant}'";
        
        Lexer.MoveNext();
        return new InfApp(new InfConst(constant, new InfTyUnbound("a")), left);
    }
}