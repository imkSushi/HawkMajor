using HawkMajor2.Engine;
using HawkMajor2.Engine.StrategyInstructions;
using HawkMajor2.Language.Lexing;
using HawkMajor2.Language.Lexing.Tokens;
using HawkMajor2.Shadows;
using HawkMajor2.Shadows.ShadowTerms;
using HawkMajor2.Shadows.ShadowTypes;
using Results;
using Valiant.Terms;

namespace HawkMajor2.Language.Parsing;

public class StrategyParser
{
    private readonly Dictionary<string, ShadowVar> _fixedTerms;
    private readonly Dictionary<string, ShadowTyMeta> _fixedTypes;
    private readonly List<Term> _previousTerms;
    private readonly ScriptLexer _lexer;
    private readonly ScriptParser _scriptParser;
    private StrategyParser(Dictionary<string, ShadowVar> fixedTerms, Dictionary<string, ShadowTyMeta> fixedTypes, List<Term> previousTerms, ScriptLexer lexer, ScriptParser scriptParser)
    {
        _fixedTerms = fixedTerms;
        _fixedTypes = fixedTypes;
        _previousTerms = previousTerms;
        _lexer = lexer;
        _scriptParser = scriptParser;
    }
    
    private void FixVariables()
    {
        foreach (var key in _fixedTerms.Keys)
        {
            var value = _fixedTerms[key];
            _fixedTerms[key] = value.FixMeta();
        }

        foreach (var key in _fixedTypes.Keys)
        {
            var value = _fixedTypes[key];
            _fixedTypes[key] = value.FixMeta();
        }
    }

    public static Result<Strategy> ParseStrategy(Dictionary<string, ShadowVar> fixedTerms,
        Dictionary<string, ShadowTyMeta> fixedTypes,
        List<Term> previousTerms,
        ScriptLexer lexer,
        ScriptParser scriptParser,
        string name,
        ShadowTheorem shadowTheorem)
    {
        var parser = new StrategyParser(fixedTerms, fixedTypes, previousTerms, lexer, scriptParser);
        if (!parser.Parse().Deconstruct(out var instructions, out var error))
            return error;
        
        var strategy = new Strategy(name, shadowTheorem, instructions);

        return strategy;
    }

    private Result<List<StrategyInstruction>> Parse()
    {
        FixVariables();
        
        if (_lexer.ExpectKeyword("{").IsError(out var error))
            return error;

        var instructions = new List<StrategyInstruction>();
        
        _lexer.SkipNewLines();

        while (_lexer.Current is not KeywordToken {Value: "}"})
        {
            if (!_lexer.ExpectIdentifier().Deconstruct(out var instructionName, out error))
                return error;

            StrategyInstruction? instruction;

            switch (instructionName)
            {
                case "match":
                    if (!ParseStratMatch().Deconstruct(out instruction, out error))
                        return error;

                    break;
                case "prove":
                    if (!ParseStratProve().Deconstruct(out instruction, out error))
                        return error;

                    break;
                case "apply":
                    if (!ParseStratApply().Deconstruct(out instruction, out error))
                        return error;

                    break;
                case "kernel":
                    if (!ParseStratKernel().Deconstruct(out instruction, out error))
                        return error;

                    break;
                default:
                    return _lexer.GenerateError($"Unrecognised strategy instruction: {instructionName}");
            }
            
            instructions.Add(instruction);
        }
        
        _lexer.MoveNext();
        
        return instructions;
    }

    private Result<StrategyInstruction> ParseStratMatch()
    {
        if (!ParseStratTheorem().Deconstruct(out var thm, out var error))
            return error;

        return new ProveLocal(thm);
    }
    
    private Result<StrategyInstruction> ParseStratProve()
    {
        if (!ParseStratTheorem().Deconstruct(out var thm, out var error))
            return error;

        return new Prove(thm);
    }

    private Result<ShadowTheorem> ParseStratTheorem(bool endLine = true)
    {
        if (!_scriptParser.ParseConjecture(_previousTerms).Deconstruct(out var conjecture, out var error))
            return error;
        
        _previousTerms.Add(conjecture.Conclusion);
        _previousTerms.AddRange(conjecture.Premises);

        if (endLine && _lexer.ExpectEndOfLine().IsError(out error)) 
            return error;

        foreach (var freeType in conjecture.FreeTypesIn())
        {
            _fixedTypes.TryAdd(freeType.Name, new ShadowTyUnfixed(freeType.Name));
        }

        foreach (var free in conjecture.FreesIn().Where(free => !_fixedTerms.ContainsKey(free.Name)))
        {
            _fixedTerms[free.Name] = new ShadowUnfixed(free.Name, ShadowType.ToShadowType(free.Type, _fixedTypes));
        }
        
        var shadowTheorem = ShadowTheorem.FromConjecture(conjecture, _fixedTerms, _fixedTypes);
        
        FixVariables();

        return shadowTheorem;
    }
    
    private Result<StrategyInstruction> ParseStratApply()
    {
        if (!_lexer.ExpectIdentifier().Deconstruct(out var name, out var error))
            return error;

        if (!ParseStratTheorem(false).Deconstruct(out var thm, out error))
            return error;
        
        if (!_scriptParser.Workspace.Strategies.TryGetValue(name, out var strategy))
            return _lexer.GenerateError($"Unrecognised strategy: {name}");

        return new ProveBy(strategy, thm);
    }
    
    private Result<StrategyInstruction> ParseStratKernel()
    {
        if (!_lexer.ExpectIdentifier().Deconstruct(out var name, out var error))
            return error;

        return name switch
        {
            "refl" => ParseKernelReflectivity(),
            "cong" => ParseKernelCongruence(),
            "pabs" => ParseKernelParameterAbstraction(),
            "tabs" => ParseKernelTypeAbstraction(),
            "beta" => ParseKernelBetaReduction(),
            "asm"  => ParseKernelAssume(),
            "mp"   => ParseKernelModusPonens(),
            "anti" => ParseKernelAntisymmetry(),
            _      => _lexer.GenerateError($"Unrecognised kernel strategy: {name}")
        };
    }
    
    private Result<StrategyInstruction> ParseKernelReflectivity()
    {
        if (!ParseStratTerm().Deconstruct(out var term, out var error))
            return error;
        
        if (_lexer.ExpectEndOfLine().IsError(out error))
            return error;

        return new KernelReflexivity(term);
    }
    
    private Result<StrategyInstruction> ParseKernelCongruence()
    {
        if (!ParseStratTheorem(false).Deconstruct(out var appThm, out var error))
            return error;
        
        if (!ParseStratTheorem().Deconstruct(out var argThm, out error))
            return error;

        return new KernelCongruence(appThm, argThm);
    }
    
    private Result<StrategyInstruction> ParseKernelParameterAbstraction()
    {
        if (!ParseStratTerm().Deconstruct(out var term, out var error))
            return error;
        
        if (!ParseStratTheorem().Deconstruct(out var thm, out error))
            return error;

        return new KernelParameterAbstraction(term, thm);
    }
    
    private Result<StrategyInstruction> ParseKernelTypeAbstraction()
    {
        if (!ParseStratType().Deconstruct(out var type, out var error))
            return error;
        
        if (!ParseStratTheorem().Deconstruct(out var thm, out error))
            return error;

        return new KernelTypeAbstraction(type, thm);
    }
    
    private Result<StrategyInstruction> ParseKernelBetaReduction()
    {
        if (!ParseStratTerm().Deconstruct(out var term, out var error))
            return error;
        
        if (!ParseStratTerm().Deconstruct(out var arg, out error))
            return error;
        
        if (_lexer.ExpectEndOfLine().IsError(out error))
            return error;

        return new KernelBetaReduction(term, arg);
    }
    
    private Result<StrategyInstruction> ParseKernelAssume()
    {
        if (!ParseStratTerm().Deconstruct(out var thm, out var error))
            return error;
        
        if (!_lexer.ExpectEndOfLine(out error))
            return error;

        return new KernelAssume(thm);
    }
    
    private Result<StrategyInstruction> ParseKernelModusPonens()
    {
        if (!ParseStratTheorem(false).Deconstruct(out var maj, out var error))
            return error;
        
        if (!ParseStratTheorem().Deconstruct(out var min, out error))
            return error;

        return new KernelEqModusPonens(maj, min);
    }
    
    private Result<StrategyInstruction> ParseKernelAntisymmetry()
    {
        if (!ParseStratTheorem(false).Deconstruct(out var left, out var error))
            return error;
        
        if (!ParseStratTheorem().Deconstruct(out var right, out error))
            return error;

        return new KernelAntisymmetry(left, right);
    }

    private Result<ShadowType> ParseStratType()
    {
        if (!_scriptParser.ParseType().Deconstruct(out var type, out var error))
            return error;
        
        return ShadowType.ToShadowType(type, _fixedTypes);
    }

    private Result<ShadowTerm> ParseStratTerm()
    {
        if (!_scriptParser.ParseTerm(_previousTerms).Deconstruct(out var term, out var error))
            return error;
        
        _previousTerms.Add(term);
        
        return ShadowTerm.ToShadowTerm(term, _fixedTerms, _fixedTypes);
    }
}