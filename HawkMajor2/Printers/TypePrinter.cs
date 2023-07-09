using HawkMajor2.Language.Inference.Types;
using HawkMajor2.Shadows.ShadowTypes;
using Printing;
using Valiant.Types;

namespace HawkMajor2.Printers;

public class TypePrinter : Printer
{
    public override string Print(IPrintable printable)
    {
        switch (printable)
        {
            case Type type:
                return PrintType(type);
            case InfType infType:
                return PrintInfType(infType);
            case ShadowType shadowType:
                return PrintShadowType(shadowType);
            default:
                return base.Print(printable);
        }
    }

    private string PrintType(Type type)
    {
        if (type is not TyApp tyApp)
            return base.Print(type);

        switch (tyApp.Args.Count)
        {
            case 0:
                if (_constantRules.TryGetValue(tyApp.Name, out var constantRule))
                    return constantRule;
                
                return tyApp.Name;
            case 1:
                if (_prefixRules.TryGetValue(tyApp.Name, out var prefixRule))
                {
                    return ShouldBracketTypeForPrefix(tyApp.Args[0], prefixRule.precedence) 
                        ? $"{prefixRule.display}({Print(tyApp.Args[0])})" 
                        : $"{prefixRule.display}{Print(tyApp.Args[0])}";
                }
                
                if (_postfixRules.TryGetValue(tyApp.Name, out var postfixRule))
                {
                    return ShouldBracketTypeForPostfix(tyApp.Args[0], postfixRule.precedence)
                        ? $"({Print(tyApp.Args[0])}){postfixRule.display}" 
                        : $"{Print(tyApp.Args[0])}{postfixRule.display}";
                }
                
                break;
            case 2:
                if (!_infixRules.TryGetValue(tyApp.Name, out var infixRule))
                    break;
                
                var leftPrecedence = infixRule.precedence;
                var rightPrecedence = infixRule.precedence;

                if (infixRule.leftAssociative)
                {
                    rightPrecedence++;
                }
                else
                {
                    leftPrecedence++;
                }

                return (ShouldBracketTypeForPostfix(tyApp.Args[0], leftPrecedence),
                    ShouldBracketTypeForPrefix(tyApp.Args[1], rightPrecedence)) switch
                    {
                        (true, true)   => $"({Print(tyApp.Args[0])}) {infixRule.display} ({Print(tyApp.Args[1])})",
                        (true, false)  => $"({Print(tyApp.Args[0])}) {infixRule.display} {Print(tyApp.Args[1])}",
                        (false, true)  => $"{Print(tyApp.Args[0])} {infixRule.display} ({Print(tyApp.Args[1])})",
                        (false, false) => $"{Print(tyApp.Args[0])} {infixRule.display} {Print(tyApp.Args[1])}",
                    };
        }
        
        return base.Print(type);
    }
    
    private string PrintInfType(InfType type)
    {
        if (type is not InfTyApp tyApp)
            return base.Print(type);

        switch (tyApp.Args.Length)
        {
            case 0:
                if (_constantRules.TryGetValue(tyApp.Name, out var constantRule))
                    return constantRule;
                
                return tyApp.Name;
            case 1:
                if (_prefixRules.TryGetValue(tyApp.Name, out var prefixRule))
                {
                    return ShouldBracketInfTypeForPrefix(tyApp.Args[0], prefixRule.precedence) 
                        ? $"{prefixRule.display}({Print(tyApp.Args[0])})" 
                        : $"{prefixRule.display}{Print(tyApp.Args[0])}";
                }
                
                if (_postfixRules.TryGetValue(tyApp.Name, out var postfixRule))
                {
                    return ShouldBracketInfTypeForPostfix(tyApp.Args[0], postfixRule.precedence)
                        ? $"({Print(tyApp.Args[0])}){postfixRule.display}" 
                        : $"{Print(tyApp.Args[0])}{postfixRule.display}";
                }
                
                break;
            case 2:
                if (!_infixRules.TryGetValue(tyApp.Name, out var infixRule))
                    break;
                
                var leftPrecedence = infixRule.precedence;
                var rightPrecedence = infixRule.precedence;

                if (infixRule.leftAssociative)
                {
                    rightPrecedence++;
                }
                else
                {
                    leftPrecedence++;
                }

                return (ShouldBracketInfTypeForPostfix(tyApp.Args[0], leftPrecedence),
                    ShouldBracketInfTypeForPrefix(tyApp.Args[1], rightPrecedence)) switch
                    {
                        (true, true)   => $"({Print(tyApp.Args[0])}) {infixRule.display} ({Print(tyApp.Args[1])})",
                        (true, false)  => $"({Print(tyApp.Args[0])}) {infixRule.display} {Print(tyApp.Args[1])}",
                        (false, true)  => $"{Print(tyApp.Args[0])} {infixRule.display} ({Print(tyApp.Args[1])})",
                        (false, false) => $"{Print(tyApp.Args[0])} {infixRule.display} {Print(tyApp.Args[1])}",
                    };
        }
        
        return base.Print(type);
    }
    
    private string PrintShadowType(ShadowType type)
    {
        if (type is not ShadowTyApp tyApp)
            return base.Print(type);

        switch (tyApp.Args.Count)
        {
            case 0:
                if (_constantRules.TryGetValue(tyApp.Name, out var constantRule))
                    return constantRule;
                
                return tyApp.Name;
            case 1:
                if (_prefixRules.TryGetValue(tyApp.Name, out var prefixRule))
                {
                    return ShouldBracketShadowTypeForPrefix(tyApp.Args[0], prefixRule.precedence) 
                        ? $"{prefixRule.display}({Print(tyApp.Args[0])})" 
                        : $"{prefixRule.display}{Print(tyApp.Args[0])}";
                }
                
                if (_postfixRules.TryGetValue(tyApp.Name, out var postfixRule))
                {
                    return ShouldBracketShadowTypeForPostfix(tyApp.Args[0], postfixRule.precedence)
                        ? $"({Print(tyApp.Args[0])}){postfixRule.display}" 
                        : $"{Print(tyApp.Args[0])}{postfixRule.display}";
                }
                
                break;
            case 2:
                if (!_infixRules.TryGetValue(tyApp.Name, out var infixRule))
                    break;
                
                var leftPrecedence = infixRule.precedence;
                var rightPrecedence = infixRule.precedence;

                if (infixRule.leftAssociative)
                {
                    rightPrecedence++;
                }
                else
                {
                    leftPrecedence++;
                }

                return (ShouldBracketShadowTypeForPostfix(tyApp.Args[0], leftPrecedence),
                    ShouldBracketShadowTypeForPrefix(tyApp.Args[1], rightPrecedence)) switch
                    {
                        (true, true)   => $"({Print(tyApp.Args[0])}) {infixRule.display} ({Print(tyApp.Args[1])})",
                        (true, false)  => $"({Print(tyApp.Args[0])}) {infixRule.display} {Print(tyApp.Args[1])}",
                        (false, true)  => $"{Print(tyApp.Args[0])} {infixRule.display} ({Print(tyApp.Args[1])})",
                        (false, false) => $"{Print(tyApp.Args[0])} {infixRule.display} {Print(tyApp.Args[1])}",
                    };
        }
        
        return base.Print(type);
    }

    private bool ShouldBracketTypeForPrefix(Type type, int precedence)
    {
        if (type is not TyApp tyApp)
            return false;
        
        if (_postfixRules.TryGetValue(tyApp.Name, out var postfixRule))
        {
            return postfixRule.precedence > precedence;
        }
        
        if (_infixRules.TryGetValue(tyApp.Name, out var infixRule))
        {
            return infixRule.precedence > precedence;
        }
        
        return false;
    }
    
    private bool ShouldBracketTypeForPostfix(Type type, int precedence)
    {
        if (type is not TyApp tyApp)
            return false;
        
        if (_prefixRules.TryGetValue(tyApp.Name, out var prefixRule))
        {
            return prefixRule.precedence > precedence;
        }
        
        if (_infixRules.TryGetValue(tyApp.Name, out var infixRule))
        {
            return infixRule.precedence > precedence;
        }
        
        return false;
    }

    private bool ShouldBracketInfTypeForPrefix(InfType type, int precedence)
    {
        if (type is not InfTyApp tyApp)
            return false;
        
        if (_postfixRules.TryGetValue(tyApp.Name, out var postfixRule))
        {
            return postfixRule.precedence > precedence;
        }
        
        if (_infixRules.TryGetValue(tyApp.Name, out var infixRule))
        {
            return infixRule.precedence > precedence;
        }
        
        return false;
    }
    
    private bool ShouldBracketInfTypeForPostfix(InfType type, int precedence)
    {
        if (type is not InfTyApp tyApp)
            return false;
        
        if (_prefixRules.TryGetValue(tyApp.Name, out var prefixRule))
        {
            return prefixRule.precedence > precedence;
        }
        
        if (_infixRules.TryGetValue(tyApp.Name, out var infixRule))
        {
            return infixRule.precedence > precedence;
        }
        
        return false;
    }

    private bool ShouldBracketShadowTypeForPrefix(ShadowType type, int precedence)
    {
        if (type is not ShadowTyApp tyApp)
            return false;
        
        if (_postfixRules.TryGetValue(tyApp.Name, out var postfixRule))
        {
            return postfixRule.precedence > precedence;
        }
        
        if (_infixRules.TryGetValue(tyApp.Name, out var infixRule))
        {
            return infixRule.precedence > precedence;
        }
        
        return false;
    }
    
    private bool ShouldBracketShadowTypeForPostfix(ShadowType type, int precedence)
    {
        if (type is not ShadowTyApp tyApp)
            return false;
        
        if (_prefixRules.TryGetValue(tyApp.Name, out var prefixRule))
        {
            return prefixRule.precedence > precedence;
        }
        
        if (_infixRules.TryGetValue(tyApp.Name, out var infixRule))
        {
            return infixRule.precedence > precedence;
        }
        
        return false;
    }

    private Dictionary<string, (string display, bool leftAssociative, int precedence)> _infixRules = new();
    private Dictionary<string, (string display, int precedence)> _prefixRules = new();
    private Dictionary<string, (string display, int precedence)> _postfixRules = new();
    private Dictionary<string, string> _constantRules = new();
    
    public void AddConstantRule(string name, string display)
    {
        _constantRules[name] = display;
    }
    
    public void AddInfixRule(string name, string display, bool leftAssociative, int precedence)
    {
        _infixRules[name] = (display, leftAssociative, precedence);
    }
    
    public void AddPrefixRule(string name, string display, int precedence)
    {
        _prefixRules[name] = (display, precedence);
    }
    
    public void AddPostfixRule(string name, string display, int precedence)
    {
        _postfixRules[name] = (display, precedence);
    }
    
    public void RemoveConstantRule(string name)
    {
        _constantRules.Remove(name);
    }
    
    public void RemoveInfixRule(string name)
    {
        _infixRules.Remove(name);
    }
    
    public void RemovePrefixRule(string name)
    {
        _prefixRules.Remove(name);
    }
    
    public void RemovePostfixRule(string name)
    {
        _postfixRules.Remove(name);
    }
}