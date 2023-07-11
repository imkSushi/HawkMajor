using System.Diagnostics.CodeAnalysis;
using System.Text;
using HawkMajor2.Engine;
using HawkMajor2.Engine.Displays;
using HawkMajor2.Engine.StrategyInstructions;
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
    private ScriptLexer _lexer;
    private Kernel _kernel;
    public readonly Workspace Workspace;
    private LexerConfig _baseLexerConfig;
    private DisplayManager _displayManager;

    public ScriptParser(Kernel kernel)
    {
        _kernel = kernel;
        _lexer = new ScriptLexer("");
        
        Workspace = new Workspace();
        
        _printer = new TheoremPrinter();
        
        var thmLexer = new Lexer("");
        
        _theoremParser = new TheoremParser(new TheoremContext(thmLexer, kernel), new TheoremTypeInference(kernel), thmLexer);
        _termParser = new TermParser(kernel, thmLexer, _theoremParser.Context.TermContext);
        _typeParser = new TypeParser(kernel, thmLexer, _theoremParser.Context.TypeContext);
        _displayManager = new DisplayManager(_termParser, _typeParser, _printer);
        
        var config = new LexerConfig {RegisterNewLines = true};
        config.AddKeywords(new []{";", "=", "}", "{", "\""});
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
        _lexer.SkipNewLines();

        if (_lexer.Current is EndOfExpressionToken)
            return true;

        if (!_lexer.ExpectIdentifier().Deconstruct(out var identifier, out var error))
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
        if (!_lexer.ExpectIdentifier().Deconstruct(out var fileIdentifier, out var error))
            return (false, error);

        fileName.Append(fileIdentifier);
        while (_lexer.Current is IdentifierToken {Value: var identifierValue})
        {
            fileName.Append($" {identifierValue}");
            _lexer.MoveNext();
        }

        if (!_lexer.ExpectEndOfLine(out error))
            return (false, error);
        
        return (true, fileName.ToString());
    }

    private Result ParseDisplay()
    {
        if (!_lexer.ExpectIdentifiers(out var termOrType, out var variant, out var error))
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

    [ParseDisplay(true, true)]
    private partial Result ParseTermInfixDisplay();

    [ParseDisplay(false, true)]
    private partial Result ParseTermPostfixDisplay();

    [ParseDisplay(false, true)]
    private partial Result ParseTermLambdaDisplay();

    [ParseDisplay(false, false)]
    private partial Result ParseTermConstantDisplay();

    [ParseDisplay(false, true)]
    private partial Result ParseTypePrefixDisplay();

    [ParseDisplay(true, true)]
    private partial Result ParseTypeInfixDisplay();

    [ParseDisplay(false, true)]
    private partial Result ParseTypePostfixDisplay();

    [ParseDisplay(false, false)]
    private partial Result ParseTypeConstantDisplay();
    
    private Result ParseTypeDefinition()
    {
        if (!_lexer.ExpectIdentifiers(out var name, out var constructorName, out var destructorName, out var error))
            return error;
        
        if (_lexer.ExpectKeyword("=").IsError(out error))
            return error;
        
        if (!ParseTerm().Deconstruct(out var term, out error))
            return error;
        
        if (_lexer.ExpectEndOfLine().IsError(out error))
            return error;
        
        if (!_kernel.NewBasicTypeDefinition(name, constructorName, destructorName, term).Deconstruct(out var constructorThm, out var destructorThm, out error))
            return error;
        
        Workspace.AddGlobalTheorem(constructorThm);
        Workspace.AddGlobalTheorem(destructorThm);
        
        return Result.Success;
    }

    private Result ParseConstant()
    {
        if (!_lexer.ExpectIdentifier(out var name, out var error))
            return error;
        
        if (_lexer.ExpectKeyword("=").IsError(out error))
            return error;
        
        if (!ParseTerm().Deconstruct(out var term, out error))
            return error;
        
        if (_lexer.ExpectEndOfLine().IsError(out error))
            return error;
        
        if (!_kernel.NewBasicDefinition(name, term).Deconstruct(out var thm, out error))
            return error;
        
        Workspace.AddGlobalTheorem(thm);
        
        return Result.Success;
    }

    private Result ParseStrategy(VisibilityModifier? modifier = null)
    {
        if (!_lexer.ExpectIdentifier(out var name, out var error))
            return error;
        
        if (!ParseConjecture().Deconstruct(out var conjecture, out error))
            return error;

        var previousTerms = new List<Term> { conjecture.Conclusion };
        previousTerms.AddRange(conjecture.Premises);

        var freeTypes = conjecture.FreeTypesIn().ToDictionary(x => x.Name, x => (ShadowTyMeta) new ShadowTyUnfixed(x.Name));
        var frees = conjecture.FreesIn().ToDictionary(x => x.Name, x => (ShadowVar)new ShadowUnfixed(x.Name, ShadowType.ToShadowType(x.Type, freeTypes)));
        
        var shadowTheorem = ShadowTheorem.FromConjecture(conjecture, frees, freeTypes);
        
        if (!StrategyParser.ParseStrategy(frees, freeTypes, previousTerms, _lexer, this, name, shadowTheorem).Deconstruct(out var strategy, out error))
            return error;

        return RegisterStrategy(modifier, strategy);
    }

    private Result RegisterStrategy(VisibilityModifier? modifier, Strategy strategy)
    {
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

    internal Result<Type> ParseType()
    {
        _lexer.SkipNewLines();
        
        string? error;
        
        if (_lexer.Current is not KeywordToken { Value: "\"" })
        {
            if (!_lexer.ExpectIdentifier().Deconstruct(out var typeString, out error))
                return error;
            
            return _typeParser.Parse(typeString);
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

    internal Result<Term> ParseTerm(List<Term>? previousTerms = null)
    {
        previousTerms ??= new List<Term>();
        
        _lexer.SkipNewLines();
        
        string? error;
        string? termString;

        if (_lexer.Current is not KeywordToken { Value: "\"" })
        {
            if (!_lexer.ExpectIdentifier(out termString, out error))
                return error;
        }
        else
        {
            if (!_lexer.ParseQuotedString(out termString, out error))
                return error;
        }

        return _termParser.Parse(termString, previousTerms);
    }

    internal Result<Conjecture> ParseConjecture(List<Term>? previousTerms = null)
    {
        previousTerms ??= new List<Term>();
        
        if (!_lexer.ParseQuotedString(out var conjectureString, out var error))
            return error;
        
        return _theoremParser.Parse(conjectureString, previousTerms);
    }

    private Result ParseProof(VisibilityModifier? modifier = null)
    {
        if (!_lexer.ExpectIdentifier().Deconstruct(out var name, out var error))
            return error;
        
        if (!ParseConjecture().Deconstruct(out var conjecture, out error))
            return error;
        
        if (!_lexer.ExpectKeyword("{").Deconstruct(out error))
            return error;
        
        Workspace.NewScope();

        Theorem? lastTheorem = null;
        
        _lexer.SkipNewLines();

        while (_lexer.Current is not KeywordToken { Value: "}" })
        {
            if (!ParseProofItem().Deconstruct(out var thm, out error))
                return error;

            if (thm is not null)
            {
                
                Workspace.AddLocalTheorem(thm);
                lastTheorem = thm;
            }
        
            _lexer.SkipNewLines();
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
            _lexer.SkipNewLines();
            if (!_lexer.ExpectIdentifier().Deconstruct(out var name, out error))
                return error;
            
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
        if (!_lexer.ExpectIdentifier(out var identifier, out var error))
            return error;

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