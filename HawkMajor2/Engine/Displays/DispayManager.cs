using HawkMajor2.Engine.Displays.Terms;
using HawkMajor2.Engine.Displays.Types;
using HawkMajor2.Language.Parsing;
using HawkMajor2.Printers;
using Results;

namespace HawkMajor2.Engine.Displays;

public class DisplayManager
{
    private TermParser _termParser;
    private TypeParser _typeParser;
    private TheoremPrinter _printer;

    public HashSet<DisplaySymbol> Displays { get; } = new();

    public DisplayManager(TermParser termParser, TypeParser typeParser, TheoremPrinter printer)
    {
        _termParser = termParser;
        _typeParser = typeParser;
        _printer = printer;
    }

    public Result ApplyDisplay(DisplaySymbol display)
    {
        var output = display switch
        {
            TypePrefixDisplay typePrefixDisplay => ApplyTypePrefixDisplay(typePrefixDisplay),
            TypePostfixDisplay typePostfixDisplay => ApplyTypePostfixDisplay(typePostfixDisplay),
            TypeConstantDisplay typeConstantDisplay => ApplyTypeConstantDisplay(typeConstantDisplay),
            TypeInfixDisplay typeInfixDisplay => ApplyTypeInfixDisplay(typeInfixDisplay),
            TermPrefixDisplay termPrefixDisplay => ApplyTermPrefixDisplay(termPrefixDisplay),
            TermPostfixDisplay termPostfixDisplay => ApplyTermPostfixDisplay(termPostfixDisplay),
            TermConstantDisplay termConstantDisplay => ApplyTermConstantDisplay(termConstantDisplay),
            TermInfixDisplay termInfixDisplay => ApplyTermInfixDisplay(termInfixDisplay),
            TermLambdaDisplay termLambdaDisplay => ApplyTermLambdaDisplay(termLambdaDisplay),
            _                                   => $"Unknown display symbol {display.Name}"
        };
        
        if (output.IsSuccess())
            Displays.Add(display);
        
        return output;
    }
    
    public Result RemoveDisplay(DisplaySymbol display)
    {
        var output = display switch
        {
            TypePrefixDisplay typePrefixDisplay => RemoveTypePrefixDisplay(typePrefixDisplay),
            TypePostfixDisplay typePostfixDisplay => RemoveTypePostfixDisplay(typePostfixDisplay),
            TypeConstantDisplay typeConstantDisplay => RemoveTypeConstantDisplay(typeConstantDisplay),
            TypeInfixDisplay typeInfixDisplay => RemoveTypeInfixDisplay(typeInfixDisplay),
            TermPrefixDisplay termPrefixDisplay => RemoveTermPrefixDisplay(termPrefixDisplay),
            TermPostfixDisplay termPostfixDisplay => RemoveTermPostfixDisplay(termPostfixDisplay),
            TermConstantDisplay termConstantDisplay => RemoveTermConstantDisplay(termConstantDisplay),
            TermInfixDisplay termInfixDisplay => RemoveTermInfixDisplay(termInfixDisplay),
            TermLambdaDisplay termLambdaDisplay => RemoveTermLambdaDisplay(termLambdaDisplay),
            _                                   => $"Unknown display symbol {display.Name}"
        };
        
        if (output.IsSuccess())
            Displays.Remove(display);
        
        return output;
    }
    
    private Result ApplyTypePrefixDisplay(TypePrefixDisplay display)
    {
        if (_typeParser.Context.AddPrefixRule(display.Name, display.Symbol, display.Precedence, display.CanInterruptIdentifier, display.Verify).IsError(out var error))
            return error;
        
        _printer.AddPrefixTypeRule(display.Name, display.Display, display.Precedence);
        
        return Result.Success;
    }
    
    private Result ApplyTypePostfixDisplay(TypePostfixDisplay display)
    {
        if (_typeParser.Context.AddPostfixRule(display.Name, display.Symbol, display.Precedence, display.CanInterruptIdentifier, display.Verify).IsError(out var error))
            return error;
        
        _printer.AddPostfixTypeRule(display.Name, display.Display, display.Precedence);
        
        return Result.Success;
    }
    
    private Result ApplyTypeConstantDisplay(TypeConstantDisplay display)
    {
        if (_typeParser.Context.AddConstantRule(display.Name, display.Symbol, display.CanInterruptIdentifier, display.Verify).IsError(out var error))
            return error;
        
        _printer.AddConstantTypeRule(display.Name, display.Display);
        
        return Result.Success;
    }
    
    private Result ApplyTypeInfixDisplay(TypeInfixDisplay display)
    {
        if (_typeParser.Context.AddInfixRule(display.Name, display.Symbol, display.Precedence, display.CanInterruptIdentifier, display.Verify).IsError(out var error))
            return error;
        
        _printer.AddInfixTypeRule(display.Name, display.Display, display.LeftAssociative, display.Precedence);
        
        return Result.Success;
    }
    
    private Result ApplyTermPrefixDisplay(TermPrefixDisplay display)
    {
        if (_termParser.Context.AddPrefixRule(display.Name, display.Symbol, display.Precedence, display.CanInterruptIdentifier, display.Verify).IsError(out var error))
            return error;
        
        _printer.AddPrefixTermRule(display.Name, display.Display, display.Precedence);
        
        return Result.Success;
    }
    
    private Result ApplyTermPostfixDisplay(TermPostfixDisplay display)
    {
        if (_termParser.Context.AddPostfixRule(display.Name, display.Symbol, display.Precedence, display.CanInterruptIdentifier, display.Verify).IsError(out var error))
            return error;
        
        _printer.AddPostfixTermRule(display.Name, display.Display, display.Precedence);
        
        return Result.Success;
    }
    
    private Result ApplyTermConstantDisplay(TermConstantDisplay display)
    {
        _printer.AddConstantTermRule(display.Name, display.Display);
        
        return Result.Success;
    }
    
    private Result ApplyTermInfixDisplay(TermInfixDisplay display)
    {
        if (_termParser.Context.AddInfixRule(display.Name, display.Symbol, display.Precedence, display.CanInterruptIdentifier, display.Verify).IsError(out var error))
            return error;
        
        _printer.AddInfixTermRule(display.Name, display.Display, display.LeftAssociative, display.Precedence);
        
        return Result.Success;
    }
    
    private Result ApplyTermLambdaDisplay(TermLambdaDisplay display)
    {
        if (_termParser.Context.AddLambdaRule(display.Name, display.Symbol, display.Precedence, display.CanInterruptIdentifier, display.Verify).IsError(out var error))
            return error;
        
        _printer.AddLambdaTermRule(display.Name, display.Display, display.Precedence);
        
        return Result.Success;
    }
    
    private Result RemoveTypePrefixDisplay(TypePrefixDisplay display)
    {
        if (_typeParser.Context.RemovePrefixRule(display.Symbol).IsError(out var error))
            return error;
        
        _printer.RemovePrefixTypeRule(display.Name);
        
        return Result.Success;
    }
    
    private Result RemoveTypePostfixDisplay(TypePostfixDisplay display)
    {
        if (_typeParser.Context.RemovePostfixRule(display.Symbol).IsError(out var error))
            return error;
        
        _printer.RemovePostfixTypeRule(display.Name);
        
        return Result.Success;
    }
    
    private Result RemoveTypeConstantDisplay(TypeConstantDisplay display)
    {
        if (_typeParser.Context.RemoveConstantRule(display.Symbol).IsError(out var error))
            return error;
        
        _printer.RemoveConstantTypeRule(display.Name);
        
        return Result.Success;
    }
    
    private Result RemoveTypeInfixDisplay(TypeInfixDisplay display)
    {
        if (_typeParser.Context.RemoveInfixRule(display.Symbol).IsError(out var error))
            return error;
        
        _printer.RemoveInfixTypeRule(display.Name);
        
        return Result.Success;
    }
    
    private Result RemoveTermPrefixDisplay(TermPrefixDisplay display)
    {
        if (_termParser.Context.RemovePrefixRule(display.Symbol).IsError(out var error))
            return error;
        
        _printer.RemovePrefixTermRule(display.Name);
        
        return Result.Success;
    }
    
    private Result RemoveTermPostfixDisplay(TermPostfixDisplay display)
    {
        if (_termParser.Context.RemovePostfixRule(display.Symbol).IsError(out var error))
            return error;
        
        _printer.RemovePostfixTermRule(display.Name);
        
        return Result.Success;
    }
    
    private Result RemoveTermConstantDisplay(TermConstantDisplay display)
    {
        _printer.RemoveConstantTermRule(display.Name);
        
        return Result.Success;
    }
    
    private Result RemoveTermInfixDisplay(TermInfixDisplay display)
    {
        if (_termParser.Context.RemoveInfixRule(display.Symbol).IsError(out var error))
            return error;
        
        _printer.RemoveInfixTermRule(display.Name);
        
        return Result.Success;
    }

    private Result RemoveTermLambdaDisplay(TermLambdaDisplay display)
    {
        if (_termParser.Context.RemoveLambdaRule(display.Symbol).IsError(out var error))
            return error;

        _printer.RemoveLambdaTermRule(display.Name);

        return Result.Success;
    }

    public Result Apply(DisplayManager manager)
    {
        return manager.Displays.Aggregate(Result.Success, (current, display) => current & ApplyDisplay(display));
    }
}