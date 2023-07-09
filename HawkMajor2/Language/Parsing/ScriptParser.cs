using System.Text;
using HawkMajor2;
using HawkMajor2.Engine;
using HawkMajor2.Engine.Displays;
using HawkMajor2.Engine.Displays.Terms;
using HawkMajor2.Engine.Displays.Types;
using HawkMajor2.Language.Contexts;
using HawkMajor2.Language.Inference;
using HawkMajor2.Language.Lexing;
using HawkMajor2.Language.Lexing.Tokens;
using HawkMajor2.Printers;
using HawkMajor2.Shadows;
using HawkMajor2.Shadows.ShadowTerms;
using HawkMajor2.Shadows.ShadowTypes;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Parsing;

public partial class ScriptParser
{
    private TypeParser _typeParser;
    private TermParser _termParser;
    private TheoremParser _theoremParser;
    private TheoremPrinter _printer;
    private Lexer _lexer;
    private Kernel _kernel;
    public readonly Workspace Workspace;
    private LexerConfig _baseLexerConfig;
    private DisplayManager _displayManager;

    public ScriptParser(Kernel kernel)
    {
        _kernel = kernel;
        _lexer = new Lexer("");
        
        Workspace = new Workspace();
        
        _printer = new TheoremPrinter();
        
        var thmLexer = new Lexer("");
        
        _theoremParser = new TheoremParser(new TheoremContext(thmLexer, kernel), new TheoremTypeInference(kernel), thmLexer);
        _termParser = new TermParser(kernel, thmLexer, _theoremParser.Context.TermContext);
        _typeParser = new TypeParser(kernel, thmLexer, _theoremParser.Context.TypeContext);
        _displayManager = new DisplayManager(_termParser, _typeParser, _printer);
        
        var config = new LexerConfig {RegisterNewLines = true};
        config.AddKeyword(";");
        config.AddKeyword("=");
        config.AddKeyword("}");
        config.AddKeyword("{");
        config.AddKeyword("\"");
        _baseLexerConfig = config;
    }

    public Result<HashSet<Theorem>, HashSet<Strategy>, DisplayManager> Run(string input)
    {
        if (Parse(input).IsError(out var error))
            return error;

        return (Workspace.GlobalTheorems, Workspace.GlobalStrategies, _displayManager);
    }
    
    public Result Parse(string input)
    {
        _printer.Activate();
        _lexer.Context = _baseLexerConfig;
        _lexer.Reset(input);
        _lexer.MoveNext();

        var atEnd = false;
        
        while (!atEnd)
        {
            if (!ParseScriptItem().Deconstruct(out atEnd, out var error))
                return error;
        }
        
        return Result.Success;
    }

    private Result<bool> ParseScriptItem()
    {
        SkipNewLines();

        if (_lexer.Current is EndOfExpressionToken)
            return true;

        if (!ExpectIdentifierName().Deconstruct(out var identifier, out var error))
            return error;

        return (identifier switch
        {
            "load"     => ParseLoad(),
            "strat"    => ParseStrategy(),
            "proof"    => ParseProof(),
            "global"   => ParseModifier(VisibilityModifier.Global),
            "local"    => ParseModifier(VisibilityModifier.Local),
            "explicit" => ParseModifier(VisibilityModifier.Explicit),
            "file"     => ParseModifier(VisibilityModifier.File),
            "const"    => ParseConstant(),
            "type"     => ParseTypeDefinition(),
            "display"  => ParseDisplay(),
            _          => _lexer.GenerateError($"Unrecognised script item: {identifier}")
        }).ErrorOr(false);
    }

    private Result ParseLoad()
    {
        if (!ParseLoadFileName().Deconstruct(out var fileName, out var error))
            return error;

        return RunFile(fileName);
    }

    private StringResult ParseLoadFileName()
    {
        var fileName = new StringBuilder();
        if (!ExpectIdentifier().Deconstruct(out var fileIdentifier, out var error))
            return (false, error);

        fileName.Append(fileIdentifier.Value);
        while (_lexer.Current is IdentifierToken {Value: var identifierValue})
        {
            fileName.Append($" {identifierValue}");
            _lexer.MoveNext();
        }

        if (ExpectEndOfLine().IsError(out error))
            return (false, error);
        
        return (true, fileName.ToString());
    }

    private Result ParseDisplay()
    {
        if (!ExpectIdentifierName().Deconstruct(out var termOrType, out var error))
            return error;

        if (!ExpectIdentifierName().Deconstruct(out var variant, out error))
            return error;

        return (termOrType, variant) switch
        {
            ("term", "prefix")  => ParseTermPrefixDisplay(),
            ("term", "infix")   => ParseTermInfixDisplay(),
            ("term", "postfix") => ParseTermPostfixDisplay(),
            ("term", "lambda")  => ParseTermLambdaDisplay(),
            ("term", "const")   => ParseTermConstantDisplay(),
            ("type", "prefix")  => ParseTypePrefixDisplay(),
            ("type", "infix")   => ParseTypeInfixDisplay(),
            ("type", "postfix") => ParseTypePostfixDisplay(),
            ("type", "const")   => ParseTypeConstantDisplay(),
            _                   => _lexer.GenerateError($"Unrecognised display variant: {variant}")
        };
    }
    
    
    [ParseDisplay(false, true)]
    private partial Result ParseTermPrefixDisplay();
    
    /*private Result ParseTermPrefixDisplay()
    {
        if (!ExpectIdentifierName().Deconstruct(out var name, out var error))
            return error;

        if (!ExpectIdentifierName().Deconstruct(out var symbol, out error))
            return error;
        
        if (!ExpectIdentifierName().Deconstruct(out var displayName, out error))
            return error;
        
        if (!ExpectIdentifierName().Deconstruct(out var precedenceString, out error))
            return error;
        
        if (!int.TryParse(precedenceString, out var precedence))
            return _lexer.GenerateError($"Expected integer precedence, got {precedenceString}");

        var interrupt = true;
        var verify = true;
        
        if (_lexer.Current is IdentifierToken interruptToken)
        {
            switch (interruptToken.Value)
            {
                case "interrupt":
                    interrupt = true;
                    _lexer.MoveNext();
                    break;
                case "noInterrupt":
                    interrupt = false;
                    _lexer.MoveNext();
                    break;
            }
        }
        
        if (_lexer.Current is IdentifierToken verifyToken)
        {
            switch (verifyToken.Value)
            {
                case "verify":
                    verify = true;
                    _lexer.MoveNext();
                    break;
                case "noVerify":
                    verify = false;
                    _lexer.MoveNext();
                    break;
            }
        }

        if (ExpectEndOfLine().IsError(out error))
            return error;

        _displayManager.ApplyDisplay(new TermPrefixDisplay(name, symbol, displayName, precedence, interrupt, verify));
        
        return Result.Success;
    }*/
    
    private Result ParseTermInfixDisplay()
    {
        if (!ExpectIdentifier().Deconstruct(out var identifierToken, out var error))
            return error;
        
        var name = identifierToken.Value;

        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var symbol = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var displayName = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        bool associativity;
        
        switch (identifierToken.Value)
        {
            case "left":
                associativity = true;
                break;
            case "right":
                associativity = false;
                break;
            default:
                return _lexer.GenerateError($"Expected associativity, got {identifierToken.Value}");
        }
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        if (!int.TryParse(identifierToken.Value, out var precedence))
            return _lexer.GenerateError($"Expected integer precedence, got {identifierToken.Value}");

        var interrupt = true;
        var verify = true;
        
        if (_lexer.Current is IdentifierToken interruptToken)
        {
            switch (interruptToken.Value)
            {
                case "interrupt":
                    interrupt = true;
                    _lexer.MoveNext();
                    break;
                case "noInterrupt":
                    interrupt = false;
                    _lexer.MoveNext();
                    break;
            }
        }
        
        if (_lexer.Current is IdentifierToken verifyToken)
        {
            switch (verifyToken.Value)
            {
                case "verify":
                    verify = true;
                    _lexer.MoveNext();
                    break;
                case "noVerify":
                    verify = false;
                    _lexer.MoveNext();
                    break;
            }
        }

        if (ExpectEndOfLine().IsError(out error))
            return error;

        _displayManager.ApplyDisplay(new TermInfixDisplay(name, symbol, displayName, associativity, precedence, interrupt, verify));
        
        return Result.Success;
    }
    
    private Result ParseTermPostfixDisplay()
    {
        if (!ExpectIdentifier().Deconstruct(out var identifierToken, out var error))
            return error;
        
        var name = identifierToken.Value;

        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var symbol = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var displayName = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        if (!int.TryParse(identifierToken.Value, out var precedence))
            return _lexer.GenerateError($"Expected integer precedence, got {identifierToken.Value}");

        var interrupt = true;
        var verify = true;
        
        if (_lexer.Current is IdentifierToken interruptToken)
        {
            switch (interruptToken.Value)
            {
                case "interrupt":
                    interrupt = true;
                    _lexer.MoveNext();
                    break;
                case "noInterrupt":
                    interrupt = false;
                    _lexer.MoveNext();
                    break;
            }
        }
        
        if (_lexer.Current is IdentifierToken verifyToken)
        {
            switch (verifyToken.Value)
            {
                case "verify":
                    verify = true;
                    _lexer.MoveNext();
                    break;
                case "noVerify":
                    verify = false;
                    _lexer.MoveNext();
                    break;
            }
        }

        if (ExpectEndOfLine().IsError(out error))
            return error;

        _displayManager.ApplyDisplay(new TermPostfixDisplay(name, symbol, displayName, precedence, interrupt, verify));
        
        return Result.Success;
    }

    private Result ParseTermLambdaDisplay()
    {
        if (!ExpectIdentifier().Deconstruct(out var identifierToken, out var error))
            return error;
        
        var name = identifierToken.Value;

        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var symbol = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var displayName = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        if (!int.TryParse(identifierToken.Value, out var precedence))
            return _lexer.GenerateError($"Expected integer precedence, got {identifierToken.Value}");

        var interrupt = true;
        var verify = true;
        
        if (_lexer.Current is IdentifierToken interruptToken)
        {
            switch (interruptToken.Value)
            {
                case "interrupt":
                    interrupt = true;
                    _lexer.MoveNext();
                    break;
                case "noInterrupt":
                    interrupt = false;
                    _lexer.MoveNext();
                    break;
            }
        }
        
        if (_lexer.Current is IdentifierToken verifyToken)
        {
            switch (verifyToken.Value)
            {
                case "verify":
                    verify = true;
                    _lexer.MoveNext();
                    break;
                case "noVerify":
                    verify = false;
                    _lexer.MoveNext();
                    break;
            }
        }

        if (ExpectEndOfLine().IsError(out error))
            return error;

        _displayManager.ApplyDisplay(new TermLambdaDisplay(name, symbol, displayName, precedence, interrupt, verify));
        
        return Result.Success;
    }

    private Result ParseTermConstantDisplay()
    {
        if (!ExpectIdentifier().Deconstruct(out var identifierToken, out var error))
            return error;
        
        var name = identifierToken.Value;

        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var symbol = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var displayName = identifierToken.Value;

        var interrupt = true;
        var verify = true;
        
        if (_lexer.Current is IdentifierToken interruptToken)
        {
            switch (interruptToken.Value)
            {
                case "interrupt":
                    interrupt = true;
                    _lexer.MoveNext();
                    break;
                case "noInterrupt":
                    interrupt = false;
                    _lexer.MoveNext();
                    break;
            }
        }
        
        if (_lexer.Current is IdentifierToken verifyToken)
        {
            switch (verifyToken.Value)
            {
                case "verify":
                    verify = true;
                    _lexer.MoveNext();
                    break;
                case "noVerify":
                    verify = false;
                    _lexer.MoveNext();
                    break;
            }
        }

        if (ExpectEndOfLine().IsError(out error))
            return error;

        _displayManager.ApplyDisplay(new TermConstantDisplay(name, symbol, displayName, interrupt, verify));
        
        return Result.Success;
    }
    
    private Result ParseTypePrefixDisplay()
    {
        if (!ExpectIdentifier().Deconstruct(out var identifierToken, out var error))
            return error;
        
        var name = identifierToken.Value;

        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var symbol = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var displayName = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        if (!int.TryParse(identifierToken.Value, out var precedence))
            return _lexer.GenerateError($"Expected integer precedence, got {identifierToken.Value}");

        var interrupt = true;
        var verify = true;
        
        if (_lexer.Current is IdentifierToken interruptToken)
        {
            switch (interruptToken.Value)
            {
                case "interrupt":
                    interrupt = true;
                    _lexer.MoveNext();
                    break;
                case "noInterrupt":
                    interrupt = false;
                    _lexer.MoveNext();
                    break;
            }
        }
        
        if (_lexer.Current is IdentifierToken verifyToken)
        {
            switch (verifyToken.Value)
            {
                case "verify":
                    verify = true;
                    _lexer.MoveNext();
                    break;
                case "noVerify":
                    verify = false;
                    _lexer.MoveNext();
                    break;
            }
        }

        if (ExpectEndOfLine().IsError(out error))
            return error;

        _displayManager.ApplyDisplay(new TypePrefixDisplay(name, symbol, displayName, precedence, interrupt, verify));
        
        return Result.Success;
    }
    
    private Result ParseTypeInfixDisplay()
    {
        if (!ExpectIdentifier().Deconstruct(out var identifierToken, out var error))
            return error;
        
        var name = identifierToken.Value;

        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var symbol = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var displayName = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        bool associativity;
        
        switch (identifierToken.Value)
        {
            case "left":
                associativity = true;
                break;
            case "right":
                associativity = false;
                break;
            default:
                return _lexer.GenerateError($"Expected associativity, got {identifierToken.Value}");
        }
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        if (!int.TryParse(identifierToken.Value, out var precedence))
            return _lexer.GenerateError($"Expected integer precedence, got {identifierToken.Value}");

        var interrupt = true;
        var verify = true;
        
        if (_lexer.Current is IdentifierToken interruptToken)
        {
            switch (interruptToken.Value)
            {
                case "interrupt":
                    interrupt = true;
                    _lexer.MoveNext();
                    break;
                case "noInterrupt":
                    interrupt = false;
                    _lexer.MoveNext();
                    break;
            }
        }
        
        if (_lexer.Current is IdentifierToken verifyToken)
        {
            switch (verifyToken.Value)
            {
                case "verify":
                    verify = true;
                    _lexer.MoveNext();
                    break;
                case "noVerify":
                    verify = false;
                    _lexer.MoveNext();
                    break;
            }
        }

        if (ExpectEndOfLine().IsError(out error))
            return error;

        _displayManager.ApplyDisplay(new TypeInfixDisplay(name, symbol, displayName, associativity, precedence, interrupt, verify));
        
        return Result.Success;
    }
    
    private Result ParseTypePostfixDisplay()
    {
        if (!ExpectIdentifier().Deconstruct(out var identifierToken, out var error))
            return error;
        
        var name = identifierToken.Value;

        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var symbol = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var displayName = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        if (!int.TryParse(identifierToken.Value, out var precedence))
            return _lexer.GenerateError($"Expected integer precedence, got {identifierToken.Value}");

        var interrupt = true;
        var verify = true;
        
        if (_lexer.Current is IdentifierToken interruptToken)
        {
            switch (interruptToken.Value)
            {
                case "interrupt":
                    interrupt = true;
                    _lexer.MoveNext();
                    break;
                case "noInterrupt":
                    interrupt = false;
                    _lexer.MoveNext();
                    break;
            }
        }
        
        if (_lexer.Current is IdentifierToken verifyToken)
        {
            switch (verifyToken.Value)
            {
                case "verify":
                    verify = true;
                    _lexer.MoveNext();
                    break;
                case "noVerify":
                    verify = false;
                    _lexer.MoveNext();
                    break;
            }
        }

        if (ExpectEndOfLine().IsError(out error))
            return error;

        _displayManager.ApplyDisplay(new TypePostfixDisplay(name, symbol, displayName, precedence, interrupt, verify));
        
        return Result.Success;
    }

    private Result ParseTypeConstantDisplay()
    {
        if (!ExpectIdentifier().Deconstruct(out var identifierToken, out var error))
            return error;
        
        var name = identifierToken.Value;

        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var symbol = identifierToken.Value;
        
        if (!ExpectIdentifier().Deconstruct(out identifierToken, out error))
            return error;
        
        var displayName = identifierToken.Value;

        var interrupt = true;
        var verify = true;
        
        if (_lexer.Current is IdentifierToken interruptToken)
        {
            switch (interruptToken.Value)
            {
                case "interrupt":
                    interrupt = true;
                    _lexer.MoveNext();
                    break;
                case "noInterrupt":
                    interrupt = false;
                    _lexer.MoveNext();
                    break;
            }
        }
        
        if (_lexer.Current is IdentifierToken verifyToken)
        {
            switch (verifyToken.Value)
            {
                case "verify":
                    verify = true;
                    _lexer.MoveNext();
                    break;
                case "noVerify":
                    verify = false;
                    _lexer.MoveNext();
                    break;
            }
        }

        if (ExpectEndOfLine().IsError(out error))
            return error;

        _displayManager.ApplyDisplay(new TypeConstantDisplay(name, symbol, displayName, interrupt, verify));
        
        return Result.Success;
    }
    
    private Result ParseTypeDefinition()
    {
        if (!ExpectIdentifier().Deconstruct(out var nameToken, out var error))
            return error;
        
        var name = nameToken.Value;

        if (!ExpectIdentifier().Deconstruct(out nameToken, out error))
            return error;
        
        var constructorName = nameToken.Value;

        if (!ExpectIdentifier().Deconstruct(out nameToken, out error))
            return error;
        
        var destructorName = nameToken.Value;
        
        if (ExpectKeyword("=").IsError(out error))
            return error;
        
        if (!ParseTerm().Deconstruct(out var term, out error))
            return error;
        
        if (ExpectEndOfLine().IsError(out error))
            return error;
        
        if (!_kernel.NewBasicTypeDefinition(name, constructorName, destructorName, term).Deconstruct(out var constructorThm, out var destructorThm, out error))
            return error;
        
        Workspace.AddGlobalTheorem(constructorThm);
        Workspace.AddGlobalTheorem(destructorThm);
        
        return Result.Success;
    }

    private Result ParseConstant()
    {
        if (!ExpectIdentifier().Deconstruct(out var nameToken, out var error))
            return error;
        
        var name = nameToken.Value;
        
        if (ExpectKeyword("=").IsError(out error))
            return error;
        
        if (!ParseTerm().Deconstruct(out var term, out error))
            return error;
        
        if (ExpectEndOfLine().IsError(out error))
            return error;
        
        if (!_kernel.NewBasicDefinition(name, term).Deconstruct(out var thm, out error))
            return error;
        
        Workspace.AddGlobalTheorem(thm);
        
        return Result.Success;
    }

    private Result ParseStrategy(VisibilityModifier? modifier = null)
    {
        if (!ExpectIdentifier().Deconstruct(out var nameToken, out var error))
            return error;
        
        var name = nameToken.Value;
        
        if (!ParseConjecture().Deconstruct(out var conjecture, out error))
            return error;

        var frees = conjecture.FreesIn().ToDictionary(x => x.Name, _ => ShadowVarType.Unfixed);
        var freeTypes = conjecture.FreeTypesIn().ToDictionary(x => x.Name, _ => ShadowVarType.Unfixed);

        var shadowTheorem = ShadowTheorem.FromConjecture(conjecture, frees, freeTypes);

        FixVariables(frees, freeTypes);

        if (ExpectKeyword("{").IsError(out error))
            return error;

        var instructions = new List<StrategyInstruction>();
        
        SkipNewLines();

        while (_lexer.Current is not KeywordToken {Value: "}"})
        {
            if (!ExpectIdentifier().Deconstruct(out var instructionToken, out error))
                return error;
            
            var instruction = instructionToken.Value;

            switch (instruction)
            {
                case "match":
                {
                    if (!ParseStratMatch(frees, freeTypes).Deconstruct(out var stratMatch, out error))
                        return error;
                    
                    instructions.Add(stratMatch);
                    continue;
                }
                case "prove":
                {
                    if (!ParseStratProve(frees, freeTypes).Deconstruct(out var stratProve, out error))
                        return error;
                    
                    instructions.Add(stratProve);
                    continue;
                }
                case "apply":
                {
                    if (!ParseStratApply(frees, freeTypes).Deconstruct(out var stratApply, out error))
                        return error;
                    
                    instructions.Add(stratApply);
                    continue;
                }
                case "kernel":
                {
                    if (!ParseStratKernel(frees, freeTypes).Deconstruct(out var stratKernel, out error))
                        return error;
                    
                    instructions.Add(stratKernel);
                    continue;
                }
                default:
                {
                    return _lexer.GenerateError($"Unrecognised strategy instruction: {instruction}");
                }
            }
        }
        
        _lexer.MoveNext();
        
        var strategy = new Strategy(name, shadowTheorem, instructions);

        switch (modifier)
        {
            case VisibilityModifier.Global:
                Workspace.AddGlobalStrategy(strategy);
                break;
            case VisibilityModifier.Local:
                Workspace.AddLocalStrategy(strategy);
                break;
            case VisibilityModifier.Explicit:
                Workspace.AddExplicitStrategy(strategy);
                break;
            case null:
            case VisibilityModifier.File:
                Workspace.AddFileStrategy(strategy);
                break;
            default:
                return _lexer.GenerateError($"Unrecognised visibility modifier: {modifier}");
        }
        
        return Result.Success;
    }

    private Result<StrategyInstruction> ParseStratMatch(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ParseStratTheorem(fixedTerms, fixedTypes).Deconstruct(out var thm, out var error))
            return error;

        return new ProveLocal(thm);
    }
    
    private Result<StrategyInstruction> ParseStratProve(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ParseStratTheorem(fixedTerms, fixedTypes).Deconstruct(out var thm, out var error))
            return error;

        return new Prove(thm);
    }

    private Result<ShadowTheorem> ParseStratTheorem(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes, bool endLine = true)
    {
        if (!ParseConjecture().Deconstruct(out var conjecture, out var error))
            return error;

        if (endLine)
        {
            if (ExpectEndOfLine().IsError(out error))
                return error;
        }

        foreach (var free in conjecture.FreesIn())
        {
            fixedTerms.TryAdd(free.Name, ShadowVarType.Unfixed);
        }
        
        foreach (var freeType in conjecture.FreeTypesIn())
        {
            fixedTypes.TryAdd(freeType.Name, ShadowVarType.Unfixed);
        }
        
        var shadowTheorem = ShadowTheorem.FromConjecture(conjecture, fixedTerms, fixedTypes);
        
        FixVariables(fixedTerms, fixedTypes);

        return shadowTheorem;
    }
    
    private Result<StrategyInstruction> ParseStratApply(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ExpectIdentifier().Deconstruct(out var nameToken, out var error))
            return error;

        var name = nameToken.Value;

        if (!ParseStratTheorem(fixedTerms, fixedTypes, false).Deconstruct(out var thm, out error))
            return error;
        
        if (!Workspace.Strategies.TryGetValue(name, out var strategy))
            return _lexer.GenerateError($"Unrecognised strategy: {name}");

        return new ProveBy(strategy, thm);
    }
    
    private Result<StrategyInstruction> ParseStratKernel(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ExpectIdentifier().Deconstruct(out var nameToken, out var error))
            return error;

        var name = nameToken.Value;

        return name switch
        {
            "refl" => ParseKernelReflectivity(fixedTerms, fixedTypes),
            "cong" => ParseKernelCongruence(fixedTerms, fixedTypes),
            "pabs" => ParseKernelParameterAbstraction(fixedTerms, fixedTypes),
            "tabs" => ParseKernelTypeAbstraction(fixedTerms, fixedTypes),
            "beta" => ParseKernelBetaReduction(fixedTerms, fixedTypes),
            "asm" => ParseKernelAssume(fixedTerms, fixedTypes),
            "mp" => ParseKernelModusPonens(fixedTerms, fixedTypes),
            "anti" => ParseKernelAntisymmetry(fixedTerms, fixedTypes),
            _ => _lexer.GenerateError($"Unrecognised kernel strategy: {name}")
        };
    }
    
    private Result<StrategyInstruction> ParseKernelReflectivity(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ParseStratTerm(fixedTerms, fixedTypes).Deconstruct(out var term, out var error))
            return error;
        
        if (ExpectEndOfLine().IsError(out error))
            return error;

        return new KernelReflexivity(term);
    }
    
    private Result<StrategyInstruction> ParseKernelCongruence(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ParseStratTheorem(fixedTerms, fixedTypes, false).Deconstruct(out var appThm, out var error))
            return error;
        
        if (!ParseStratTheorem(fixedTerms, fixedTypes).Deconstruct(out var argThm, out error))
            return error;

        return new KernelCongruence(appThm, argThm);
    }
    
    private Result<StrategyInstruction> ParseKernelParameterAbstraction(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ParseStratTerm(fixedTerms, fixedTypes).Deconstruct(out var term, out var error))
            return error;
        
        if (!ParseStratTheorem(fixedTerms, fixedTypes).Deconstruct(out var thm, out error))
            return error;

        return new KernelParameterAbstraction(term, thm);
    }
    
    private Result<StrategyInstruction> ParseKernelTypeAbstraction(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ParseStratType(fixedTypes).Deconstruct(out var type, out var error))
            return error;
        
        if (!ParseStratTheorem(fixedTerms, fixedTypes).Deconstruct(out var thm, out error))
            return error;

        return new KernelTypeAbstraction(type, thm);
    }
    
    private Result<StrategyInstruction> ParseKernelBetaReduction(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ParseStratTerm(fixedTerms, fixedTypes).Deconstruct(out var term, out var error))
            return error;
        
        if (!ParseStratTerm(fixedTerms, fixedTypes).Deconstruct(out var arg, out error))
            return error;
        
        if (ExpectEndOfLine().IsError(out error))
            return error;

        return new KernelBetaReduction(term, arg);
    }
    
    private Result<StrategyInstruction> ParseKernelAssume(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ParseStratTerm(fixedTerms, fixedTypes).Deconstruct(out var thm, out var error))
            return error;
        
        if (ExpectEndOfLine().IsError(out error))
            return error;

        return new KernelAssume(thm);
    }
    
    private Result<StrategyInstruction> ParseKernelModusPonens(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ParseStratTheorem(fixedTerms, fixedTypes, false).Deconstruct(out var maj, out var error))
            return error;
        
        if (!ParseStratTheorem(fixedTerms, fixedTypes).Deconstruct(out var min, out error))
            return error;

        return new KernelEqModusPonens(maj, min);
    }
    
    private Result<StrategyInstruction> ParseKernelAntisymmetry(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ParseStratTheorem(fixedTerms, fixedTypes, false).Deconstruct(out var left, out var error))
            return error;
        
        if (!ParseStratTheorem(fixedTerms, fixedTypes).Deconstruct(out var right, out error))
            return error;

        return new KernelAntisymmetry(left, right);
    }

    private Result<ShadowType> ParseStratType(Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ParseType().Deconstruct(out var type, out var error))
            return error;
        
        return ShadowType.ToShadowType(type, fixedTypes);
    }

    private Result<Type> ParseType()
    {
        SkipNewLines();
        
        string? error;
        
        if (_lexer.Current is not KeywordToken { Value: "\"" })
        {
            if (!ExpectIdentifier().Deconstruct(out var typeString, out error))
                return error;
            
            return _typeParser.Parse(typeString.Value);
        }
        
        _lexer.MoveNext();

        var conjectureStringBuilder = new StringBuilder();
        while (_lexer.Current is not KeywordToken { Value: "\"" })
        {
            switch (_lexer.Current)
            {
                case IdentifierToken { Value: var identifierValue }:
                {
                    conjectureStringBuilder.Append(identifierValue);
                    break;
                }
                case KeywordToken { Value: var keywordValue }:
                {
                    conjectureStringBuilder.Append(keywordValue);
                    break;
                }
                case ErrorToken { Message: var errorMessage }:
                {
                    return _lexer.GenerateError(errorMessage);
                }
                case NewLineToken:
                {
                    conjectureStringBuilder.Append('\n');
                    break;
                }
                case EndOfExpressionToken:
                {
                    return _lexer.GenerateError("Unexpected end of expression");
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _lexer.MoveNext();
        }

        _lexer.MoveNext();

        return _typeParser.Parse(conjectureStringBuilder.ToString());
    }

    private Result<ShadowTerm> ParseStratTerm(Dictionary<string, ShadowVarType> fixedTerms,
        Dictionary<string, ShadowVarType> fixedTypes)
    {
        if (!ParseTerm().Deconstruct(out var term, out var error))
            return error;
        
        return ShadowTerm.ToShadowTerm(term, fixedTerms, fixedTypes);
    }

    private Result<Term> ParseTerm()
    {
        SkipNewLines();
        
        string? error;

        if (_lexer.Current is not KeywordToken { Value: "\"" })
        {
            if (!ExpectIdentifier().Deconstruct(out var termString, out error))
                return error;
            
            return _termParser.Parse(termString.Value);
        }
        
        _lexer.MoveNext();

        var conjectureStringBuilder = new StringBuilder();
        while (_lexer.Current is not KeywordToken { Value: "\"" })
        {
            switch (_lexer.Current)
            {
                case IdentifierToken { Value: var identifierValue }:
                {
                    conjectureStringBuilder.Append(identifierValue);
                    break;
                }
                case KeywordToken { Value: var keywordValue }:
                {
                    conjectureStringBuilder.Append(keywordValue);
                    break;
                }
                case ErrorToken { Message: var errorMessage }:
                {
                    return _lexer.GenerateError(errorMessage);
                }
                case NewLineToken:
                {
                    conjectureStringBuilder.Append('\n');
                    break;
                }
                case EndOfExpressionToken:
                {
                    return _lexer.GenerateError("Unexpected end of expression");
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _lexer.MoveNext();
        }

        _lexer.MoveNext();

        return _termParser.Parse(conjectureStringBuilder.ToString());
    }

    private static void FixVariables(Dictionary<string, ShadowVarType> frees, Dictionary<string, ShadowVarType> freeTypes)
    {
        foreach (var key in frees.Keys)
        {
            frees[key] = ShadowVarType.Fixed;
        }

        foreach (var key in freeTypes.Keys)
        {
            freeTypes[key] = ShadowVarType.Fixed;
        }
    }

    private Result<Conjecture> ParseConjecture()
    {
        string? error;
        if (ExpectKeyword("\"").IsError(out error))
            return error;

        var conjectureStringBuilder = new StringBuilder();
        while (_lexer.Current is not KeywordToken { Value: "\"" })
        {
            conjectureStringBuilder.Append(' ');
            switch (_lexer.Current)
            {
                case IdentifierToken { Value: var identifierValue }:
                {
                    conjectureStringBuilder.Append(identifierValue);
                    break;
                }
                case KeywordToken { Value: var keywordValue }:
                {
                    conjectureStringBuilder.Append(keywordValue);
                    break;
                }
                case ErrorToken { Message: var errorMessage }:
                {
                    return _lexer.GenerateError(errorMessage);
                }
                case NewLineToken:
                {
                    conjectureStringBuilder.Append('\n');
                    break;
                }
                case EndOfExpressionToken:
                {
                    return _lexer.GenerateError("Unexpected end of expression");
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _lexer.MoveNext();
        }

        _lexer.MoveNext();

        return _theoremParser.Parse(conjectureStringBuilder.ToString());
    }

    private Result ParseProof(VisibilityModifier? modifier = null)
    {
        if (!ExpectIdentifier().Deconstruct(out var identifierToken, out var error))
            return error;
        
        var name = identifierToken.Value;
        
        if (!ParseConjecture().Deconstruct(out var conjecture, out error))
            return error;
        
        if (!ExpectKeyword("{").Deconstruct(out error))
            return error;
        
        Workspace.NewScope();

        Theorem? lastTheorem = null;
        
        SkipNewLines();

        while (_lexer.Current is not KeywordToken { Value: "}" })
        {
            if (!ParseProofItem().Deconstruct(out var thm, out error))
                return error;

            if (thm is not null)
            {
                
                Workspace.AddLocalTheorem(thm);
                lastTheorem = thm;
            }
        
            SkipNewLines();
        }

        Theorem theorem;
        
        if (lastTheorem != null && conjecture.Equals(lastTheorem))
        {
            theorem = lastTheorem;
        }
        else
        {
            var provedThm = Workspace.Prove(conjecture);
            if (provedThm is null)
                return _lexer.GenerateError("Could not prove conjecture");
            
            theorem = provedThm;
        }
        
        Workspace.EndScope();
        
        _lexer.MoveNext();
        
        switch (modifier)
        {
            case VisibilityModifier.Global:
            {
                Workspace.AddGlobalTheorem(theorem);
                break;
            }
            case VisibilityModifier.Local:
            {
                Workspace.AddLocalTheorem(theorem);
                break;
            }
            case null:
            case VisibilityModifier.File:
            {
                Workspace.AddFileTheorem(theorem);
                break;
            }
            case VisibilityModifier.Explicit:
                return "Explicit visibility modifier not allowed here";
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null);
            }
        }
        
        return Result.Success;
    }

    private Result<Theorem?> ParseProofItem()
    {
        if (!ParseConjecture().Deconstruct(out var conjecture, out var error))
            return error;

        if (_lexer.Current is IdentifierToken { Value: "by" })
        {
            _lexer.MoveNext();
            SkipNewLines();
            if (!ExpectIdentifier().Deconstruct(out var identifierToken, out error))
                return error;
            
            var name = identifierToken.Value;
            
            if (!Workspace.Strategies.TryGetValue(name, out var strategy))
                return _lexer.GenerateError($"Strategy {name} not found");
            
            _lexer.MoveNext();
            
            var theorem = strategy.Apply(conjecture, Workspace);
            if (theorem is null)
                return _lexer.GenerateError($"Strategy {name} failed to apply");
            
            return theorem;
        }

        var provedThm = Workspace.Prove(conjecture);
        if (provedThm is null)
            return _lexer.GenerateError("Failed to prove conjecture");
        
        return provedThm;
    }

    private Result ParseModifier(VisibilityModifier modifier)
    {
        if (!ExpectIdentifier().Deconstruct(out var identifierToken, out var error))
            return error;
        
        var identifier = identifierToken.Value;

        switch (identifier)
        {
            case "strat":
            {
                if (ParseStrategy(modifier).IsError(out error))
                    return error;
                
                return Result.Success;
            }
            case "proof":
            {
                if (ParseProof(modifier).IsError(out error))
                    return error;
                
                return Result.Success;
            }
            default:
            {
                return _lexer.GenerateError($"Unrecognised script item: {identifier}");
            }
        }
    }

    private StringResult ExpectIdentifierName()
    {
        return !ExpectIdentifier().Deconstruct(out var identifierToken, out var error) ? (false, error) : (true, identifierToken.Value);
    }
    
    private Result<IdentifierToken> ExpectIdentifier()
    {
        SkipNewLines();

        if (_lexer.Current is KeywordToken { Value: "\"" })
        {
            var data = _lexer.GetCurrentTokenData();
            _lexer.MoveNext();
            var stringBuilder = new StringBuilder();
            var first = true;

            while (_lexer.Current is not KeywordToken { Value: "\"" })
            {
                if (!first)
                {
                    stringBuilder.Append(' ');
                }
                else
                {
                    first = false;
                }

                stringBuilder.Append(_lexer.Current.Value);
                _lexer.MoveNext();
                if (_lexer.Current is EndOfExpressionToken)
                    return _lexer.GenerateError("Unexpected end of expression");
                if (_lexer.Current is ErrorToken { Message: var errorMessage })
                    return _lexer.GenerateError(errorMessage);
            }
            
            _lexer.MoveNext();
            return new IdentifierToken(stringBuilder.ToString(), data);
        }
        
        if (_lexer.Current is not IdentifierToken identifier)
            return _lexer.GenerateError("Expected identifier");
        
        _lexer.MoveNext();
        return identifier;
    }
    
    private Result Identifier(string identifier)
    {
        SkipNewLines();
        
        if (_lexer.Current is not IdentifierToken identifierToken)
            return _lexer.GenerateError("Expected identifier");

        if (identifierToken.Value != identifier)
            return _lexer.GenerateError($"Expected identifier {identifier}");
        
        _lexer.MoveNext();
        return Result.Success;
    }

    private Result<KeywordToken> ExpectKeyword()
    {
        SkipNewLines();
        
        if (_lexer.Current is not KeywordToken keyword)
            return _lexer.GenerateError("Expected keyword");
        
        _lexer.MoveNext();
        return keyword;
    }

    private Result ExpectKeyword(string keyword)
    {
        SkipNewLines();
        
        if (_lexer.Current is not KeywordToken keywordToken)
            return _lexer.GenerateError("Expected keyword");

        if (keywordToken.Value != keyword)
            return _lexer.GenerateError($"Expected keyword {keyword}");
        
        _lexer.MoveNext();
        return Result.Success;
    }

    private void SkipNewLines()
    {
        while (_lexer.Current is NewLineToken)
            _lexer.MoveNext();
    }
    
    private Result ExpectEndOfLine()
    {
        switch (_lexer.Current)
        {
            case NewLineToken:
                _lexer.MoveNext();
                return Result.Success;
            case KeywordToken {Value: ";"}:
                _lexer.MoveNext();
                return Result.Success;
            case EndOfExpressionToken:
                return Result.Success;
            default:
                return _lexer.GenerateError("Expected end of line");
        }
    }
    
    private Result RunFile(string fileName)
    {
        var parser = new ScriptParser(_kernel);
        
        var output = parser.Run(File.ReadAllText(fileName));
        
        if (!output.Deconstruct(out var theorems, out var strategies, out var displayManager, out var error))
            return error;

        _printer.Activate();
        Workspace.AddGlobalTheorems(theorems);
        Workspace.AddGlobalStrategies(strategies);
        return _displayManager.Apply(displayManager);
    }
}

public enum VisibilityModifier
{
    Global,
    File,
    Local,
    Explicit
}