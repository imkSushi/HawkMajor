using HawkMajor2.Language.Inference.Types;
using HawkMajor2.Language.Lexing;
using HawkMajor2.Language.Lexing.Tokens;
using Results;
using Valiant;

namespace HawkMajor2.Language.Contexts;

public class TypeContext : LexingContext<InfType>
{
    public Kernel Kernel { get; }

    public TypeContext(Lexer lexer, Kernel kernel) : base(lexer)
    {
        Kernel = kernel;
        
        var lexerConfig = new LexerConfig();
        lexerConfig.AddKeyword("(");
        lexerConfig.AddKeyword(")");
        LexerConfig = lexerConfig;

        Rules["("] = (new PrefixBuiltInRule(Grouping), new InfixNoRule());
        Rules[")"] = (new PrefixBuiltInDummyRule(")"), new InfixNoRule());
    }
    public override void SetLexerConfig()
    {
        Lexer.Context = LexerConfig;
    }

    protected override LexerConfig LexerConfig { get; }

    protected override Result<Func<Result<InfType>>> PrefixIdentifierRule(string name)
    {
        return new Result<Func<Result<InfType>>>(() => Identifier(name));
    }

    private Result<InfType> Identifier(string name)
    {
        Lexer.MoveNext();
        
        if (!Kernel.TypeArities.TryGetValue(name, out var arity))
            return new InfTyVar(name);
        
        var args = new InfType[arity];
        for (var i = 0; i < arity; i++)
        {
            if (!ParsePrecedence(int.MaxValue - 1).Deconstruct(out var arg, out var error))
                return error;
            args[i] = arg;
        }

        return new InfTyApp(name, args);
    }

    private Result<InfType> Grouping()
    {
        Lexer.MoveNext();
        
        var result = ParsePrecedence(int.MaxValue - 1);
        
        if (Lexer.Current is not KeywordToken {Value: ")"})
            return "Expected closing parenthesis";
        Lexer.MoveNext();
        
        return result;
    }

    protected override Result<Func<InfType, Result<InfType>>, int> InfixIdentifierRule(string name)
    {
        return $"Unexpected type infix operator '{name}'";
    }

    public Result AddInfixRule(string symbol, string tyAppName, int precedence, bool leftAssociative, bool canInterruptIdentifier, bool verify = true)
    {
        if (verify)
        {
            if (!Kernel.TypeArities.TryGetValue(tyAppName, out var arity))
                return $"Type '{tyAppName}' does not exist";
            
            if (arity != 2)
                return $"Type '{tyAppName}' does not have arity 2. It has arity {arity}";
        }
        
        PrefixRule prefixRule;
        
        if (Rules.TryGetValue(symbol, out var rule))
        {
            if (rule.infixRule is NoOverwriteInfixRule) 
                return $"Infix rule for '{symbol}' already exists";
            
            prefixRule = rule.prefixRule;
            
            if (LexerConfig.CanInterruptIdentifier(symbol) != canInterruptIdentifier)
                return new Result($"Infix rule for '{symbol}' already exists with different interrupt identifier setting");
        }
        else
        {
            prefixRule = new PrefixNoRule();
            
            LexerConfig.AddKeyword(symbol, canInterruptIdentifier);
        }
        
        var parsePrecedence = leftAssociative ? precedence : precedence + 1;
        
        Rules[symbol] = (prefixRule, new InfixUsedInfixRule(left => ParseInfixRule(tyAppName, left, parsePrecedence), precedence));

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
            Rules.Remove(symbol);
            LexerConfig.RemoveKeyword(symbol);
        }
        else
        {
            Rules[symbol] = (rule.prefixRule, new InfixNoRule());
        }

        return Result.Success;
    }

    private Result<InfType> ParseInfixRule(string tyAppName, InfType left, int parsePrecedence)
    {
        if (!Kernel.TypeArities.TryGetValue(tyAppName, out var arity))
            return $"Unknown type application '{tyAppName}'";
        
        if (arity != 2)
            return $"Unexpected arity for type application '{tyAppName}'. Expected 2, got {arity}";

        Lexer.MoveNext();
        
        if (!ParsePrecedence(parsePrecedence).Deconstruct(out var right, out var error))
            return error;
        
        return new InfTyApp(tyAppName, new[] {left, right});
    }
    
    public Result AddPrefixRule(string symbol, string tyAppName, int precedence, bool canInterruptIdentifier, bool verify = true)
    {
        if (verify)
        {
            if (!Kernel.TypeArities.TryGetValue(tyAppName, out var arity))
                return $"Type '{tyAppName}' does not exist";
            
            if (arity != 1)
                return $"Type '{tyAppName}' does not have arity 1. It has arity {arity}";
        }
        
        InfixRule infixRule;
        
        if (Rules.TryGetValue(symbol, out var rule))
        {
            if (rule.prefixRule is NoOverwritePrefixRule) 
                return $"Prefix rule for '{symbol}' already exists";
            
            infixRule = rule.infixRule;
            
            if (LexerConfig.CanInterruptIdentifier(symbol) != canInterruptIdentifier)
                return new Result($"Prefix rule for '{symbol}' already exists with different interrupt identifier setting");
        }
        else
        {
            infixRule = new InfixNoRule();
            
            LexerConfig.AddKeyword(symbol, canInterruptIdentifier);
        }
        
        Rules[symbol] = (new PrefixUsedPrefixRule(() => ParsePrefixRule(tyAppName, precedence)), infixRule);

        return Result.Success;
    }
    
    public Result RemovePrefixRule(string symbol)
    {
        if (!Rules.TryGetValue(symbol, out var rule)) 
            return $"Prefix rule for '{symbol}' does not exist";

        if (rule.prefixRule is PrefixNoRule) 
            return $"Prefix rule for '{symbol}' does not exist";
        
        if (rule.prefixRule is not PrefixUsedPrefixRule)
            return $"Rule for '{symbol}' is not a prefix rule";
        
        if (rule.infixRule is InfixNoRule)
        {
            Rules.Remove(symbol);
            LexerConfig.RemoveKeyword(symbol);
        }
        else
        {
            Rules[symbol] = (new PrefixNoRule(), rule.infixRule);
        }

        return Result.Success;
    }
    
    private Result<InfType> ParsePrefixRule(string tyAppName, int precedence)
    {
        if (!Kernel.TypeArities.TryGetValue(tyAppName, out var arity))
            return $"Unknown type application '{tyAppName}'";
        
        if (arity != 1)
            return $"Unexpected arity for type application '{tyAppName}'. Expected 1, got {arity}";
        
        Lexer.MoveNext();
        
        if (!ParsePrecedence(precedence).Deconstruct(out var arg, out var error))
            return error;
        
        return new InfTyApp(tyAppName, new[] {arg});
    }
    
    public Result AddPostfixRule(string symbol, string tyAppName, int precedence, bool canInterruptIdentifier, bool verify = true)
    {
        if (verify)
        {
            if (!Kernel.TypeArities.TryGetValue(tyAppName, out var arity))
                return $"Type '{tyAppName}' does not exist";
            
            if (arity != 1)
                return $"Type '{tyAppName}' does not have arity 1. It has arity {arity}";
        }
        
        PrefixRule prefixRule;
        
        if (Rules.TryGetValue(symbol, out var rule))
        {
            if (rule.infixRule is NoOverwriteInfixRule) 
                return $"Postfix rule for '{symbol}' already exists";
            
            prefixRule = rule.prefixRule;
            
            if (LexerConfig.CanInterruptIdentifier(symbol) != canInterruptIdentifier)
                return new Result($"Postfix rule for '{symbol}' already exists with different interrupt identifier setting");
        }
        else
        {
            prefixRule = new PrefixNoRule();
            
            LexerConfig.AddKeyword(symbol, canInterruptIdentifier);
        }
        
        Rules[symbol] = (prefixRule, new PostfixUsedInfixRule(left => ParsePostfixRule(tyAppName, left), precedence));

        return Result.Success;
    }
    
    public Result RemovePostfixRule(string symbol)
    {
        if (!Rules.TryGetValue(symbol, out var rule)) 
            return $"Postfix rule for '{symbol}' does not exist";

        if (rule.infixRule is InfixNoRule) 
            return $"Postfix rule for '{symbol}' does not exist";
        
        if (rule.infixRule is not PostfixUsedInfixRule)
            return $"Rule for '{symbol}' is not a postfix rule";
        
        if (rule.prefixRule is PrefixNoRule)
        {
            Rules.Remove(symbol);
            LexerConfig.RemoveKeyword(symbol);
        }
        else
        {
            Rules[symbol] = (rule.prefixRule, new InfixNoRule());
        }

        return Result.Success;
    }
    
    private Result<InfType> ParsePostfixRule(string tyAppName, InfType left)
    {
        if (!Kernel.TypeArities.TryGetValue(tyAppName, out var arity))
            return $"Unknown type application '{tyAppName}'";
        
        if (arity != 1)
            return $"Unexpected arity for type application '{tyAppName}'. Expected 1, got {arity}";
        
        Lexer.MoveNext();
        
        return new InfTyApp(tyAppName, new[] {left});
    }
    
    public Result AddConstantRule(string symbol, string tyAppName, bool canInterruptIdentifier, bool verify = true)
    {
        if (verify)
        {
            if (!Kernel.TypeArities.TryGetValue(tyAppName, out var arity))
                return $"Type '{tyAppName}' does not exist";
            
            if (arity != 0)
                return $"Type '{tyAppName}' does not have arity 0. It has arity {arity}";
        }
        
        InfixRule infixRule;
        
        if (Rules.TryGetValue(symbol, out var rule))
        {
            if (rule.prefixRule is NoOverwritePrefixRule) 
                return $"Constant rule for '{symbol}' already exists";
            
            infixRule = rule.infixRule;
            
            if (LexerConfig.CanInterruptIdentifier(symbol) != canInterruptIdentifier)
                return new Result($"Constant rule for '{symbol}' already exists with different interrupt identifier setting");
        }
        else
        {
            infixRule = new InfixNoRule();
            
            LexerConfig.AddKeyword(symbol, canInterruptIdentifier);
        }
        
        Rules[symbol] = (new ConstantUsedPrefixRule(() => ParseConstantRule(tyAppName)), infixRule);
        
        return Result.Success;
    }
    
    public Result RemoveConstantRule(string symbol)
    {
        if (!Rules.TryGetValue(symbol, out var rule)) 
            return $"Constant rule for '{symbol}' does not exist";

        if (rule.prefixRule is PrefixNoRule) 
            return $"Constant rule for '{symbol}' does not exist";
        
        if (rule.prefixRule is not ConstantUsedPrefixRule)
            return $"Rule for '{symbol}' is not a constant rule";
        
        if (rule.infixRule is InfixNoRule)
        {
            Rules.Remove(symbol);
            LexerConfig.RemoveKeyword(symbol);
        }
        else
        {
            Rules[symbol] = (new PrefixNoRule(), rule.infixRule);
        }

        return Result.Success;
    }
    
    private Result<InfType> ParseConstantRule(string tyAppName)
    {
        if (!Kernel.TypeArities.TryGetValue(tyAppName, out var arity))
            return $"Unknown type application '{tyAppName}'";
        
        if (arity != 0)
            return $"Unexpected arity for type application '{tyAppName}'. Expected 0, got {arity}";
        
        Lexer.MoveNext();
        
        return new InfTyApp(tyAppName, Array.Empty<InfType>());
    }

    public Result AddMacroRule(string symbol, Type type, bool canInterruptIdentifier)
    {
        
        InfixRule infixRule;
        
        if (Rules.TryGetValue(symbol, out var rule))
        {
            if (rule.prefixRule is NoOverwritePrefixRule) 
                return $"Macro rule for '{symbol}' already exists";
            
            infixRule = rule.infixRule;
            
            if (LexerConfig.CanInterruptIdentifier(symbol) != canInterruptIdentifier)
                return new Result($"Macro rule for '{symbol}' already exists with different interrupt identifier setting");
        }
        else
        {
            infixRule = new InfixNoRule();
            
            LexerConfig.AddKeyword(symbol, canInterruptIdentifier);
        }
        
        Rules[symbol] = (new MacroUsedPrefixRule(() => ParseMacroRule(type)), infixRule);
        
        return Result.Success;
    }
    
    public Result RemoveMacroRule(string symbol)
    {
        if (!Rules.TryGetValue(symbol, out var rule)) 
            return $"Macro rule for '{symbol}' does not exist";

        if (rule.prefixRule is PrefixNoRule) 
            return $"Macro rule for '{symbol}' does not exist";
        
        if (rule.prefixRule is not MacroUsedPrefixRule)
            return $"Rule for '{symbol}' is not a macro rule";
        
        if (rule.infixRule is InfixNoRule)
        {
            Rules.Remove(symbol);
            LexerConfig.RemoveKeyword(symbol);
        }
        else
        {
            Rules[symbol] = (new PrefixNoRule(), rule.infixRule);
        }

        return Result.Success;
    }
    
    private Result<InfType> ParseMacroRule(Type type)
    {
        Lexer.MoveNext();

        return new InfTyFixed(type);
    }

    private sealed record InfixUsedInfixRule(Func<InfType, Result<InfType>> Rule, int Precedence) : SetInfixRule(Rule, Precedence);
    private sealed record PrefixUsedPrefixRule(Func<Result<InfType>> Rule) : SetPrefixRule(Rule);
    private sealed record PostfixUsedInfixRule(Func<InfType, Result<InfType>> Rule, int Precedence) : SetInfixRule(Rule, Precedence);
    private sealed record ConstantUsedPrefixRule(Func<Result<InfType>> Rule) : SetPrefixRule(Rule);
    private sealed record MacroUsedPrefixRule(Func<Result<InfType>> Rule) : SetPrefixRule(Rule);
}