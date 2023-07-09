using System.Text;
using HawkMajor2.Extensions;
using HawkMajor2.Language.Inference.Terms;
using HawkMajor2.Shadows.ShadowTerms;
using Printing;
using Valiant.Terms;

namespace HawkMajor2.Printers;

public class TermPrinter : Printer
{
    private TypePrinter _typePrinter = new();
    
    public override string Print(IPrintable printable)
    {
        return printable switch
        {
            Term term => PrintTerm(term),
            InfTerm term => PrintInfTerm(term),
            ShadowTerm term => PrintShadowTerm(term),
            _ => _typePrinter.Print(printable)
        };
    }

    private string PrintTerm(Term term)
    {
        return term switch
        {
            Abs abs      => PrintAbs(abs),
            App app      => PrintApp(app),
            Bound bound  => PrintBound(bound),
            Const @const => PrintConst(@const),
            Free free    => PrintFree(free),
            _            => base.Print(term)
        };
    }
    
    private string PrintAbs(Abs abs, string lambdaSymbol = "λ")
    {
        var (body, free) = abs.GetBody();

        var output = new StringBuilder($"{lambdaSymbol} {free.Name}");

        while (body is Abs innerAbs)
        {
            (body, free) = innerAbs.GetBody();
            
            output.Append($" {free.Name}");
        }
        
        output.Append($" . {body}");
        
        return output.ToString();
    }
    
    private string PrintApp(App app)
    {
        switch (app)
        {
            case {Application: Const {Name: var constName}, Argument: Abs abs} when _lambdaRules.TryGetValue(constName, out var lambdaDisplay):
                return PrintAbs(abs, lambdaDisplay.display);
            case {Application: App {Application: Const {Name: var infixName}, Argument: var leftArg}, Argument: var rightArg} when _infixRules.TryGetValue(infixName, out var infixDisplay):
            {
                var leftPrecedence = infixDisplay.precedence;
                var rightPrecedence = infixDisplay.precedence;

                if (infixDisplay.leftAssociative)
                {
                    rightPrecedence++;
                }
                else
                {
                    leftPrecedence++;
                }

                return (BracketForPostfix(leftArg, leftPrecedence), BracketForPrefix(rightArg, rightPrecedence)) switch
                {
                    (true, true)   => $"({Print(leftArg)}) {infixDisplay.display} ({Print(rightArg)})",
                    (true, false)  => $"({Print(leftArg)}) {infixDisplay.display} {Print(rightArg)}",
                    (false, true)  => $"{Print(leftArg)} {infixDisplay.display} ({Print(rightArg)})",
                    (false, false) => $"{Print(leftArg)} {infixDisplay.display} {Print(rightArg)}"
                };
            }
            case {Application: Const {Name: var prefixName}, Argument: var arg} when _prefixRules.TryGetValue(prefixName, out var prefixDisplay):
            {
                var precedence = prefixDisplay.precedence;
                var bracket = BracketForPrefix(arg, precedence);
                return $"{prefixDisplay.display}{(bracket ? $"({Print(arg)})" : Print(arg))}";
            }
            case {Application: var arg, Argument: Const {Name: var postfixName}} when _postfixRules.TryGetValue(postfixName, out var postfixDisplay):
            {
                var precedence = postfixDisplay.precedence;
                var bracket = BracketForPostfix(arg, precedence);
                return $"{(bracket ? $"({Print(arg)})" : Print(arg))}{postfixDisplay.display}";
            }
        }
        
        return (BracketForPostfix(app.Application, 0), BracketForPrefix(app.Argument, 0)) switch
        {
            (true, true)   => $"({Print(app.Application)}) ({Print(app.Argument)})",
            (true, false)  => $"({Print(app.Application)}) {Print(app.Argument)}",
            (false, true)  => $"{Print(app.Application)} ({Print(app.Argument)})",
            (false, false) => $"{Print(app.Application)} {Print(app.Argument)}"
        };
    }
    
    private bool BracketForPrefix(Term term, int precedence)
    {
        return term switch
        {
            App { Application: Const { Name: var constName }, Argument: Abs } when _lambdaRules.ContainsKey(constName)
                => false,
            App { Application: App { Application: Const { Name: var infixName } } } when
                _infixRules.TryGetValue(infixName, out var infixDisplay) => infixDisplay.precedence > precedence,
            App { Application: Const { Name: var prefixName } } when _prefixRules.ContainsKey(prefixName) => false,
            App { Argument: Const { Name: var postfixName } } when _postfixRules.TryGetValue(postfixName,
                out var postfixDisplay) => postfixDisplay.precedence > precedence,
            App => precedence > 0,
            _   => false
        };
    }
    
    private bool BracketForPostfix(Term term, int precedence)
    {
        return term switch
        {
            Abs => true,
            App { Application: Const { Name: var constName }, Argument: Abs } when _lambdaRules.ContainsKey(constName)
                => true,
            App { Application: App { Application: Const { Name: var infixName } } } when
                _infixRules.TryGetValue(infixName, out var infixDisplay) => infixDisplay.precedence > precedence,
            App { Application: Const { Name: var prefixName } } when _prefixRules.TryGetValue(prefixName,
                out var prefixDisplay) => prefixDisplay.precedence > precedence,
            App { Argument: Const { Name: var postfixName } } when _postfixRules.ContainsKey(postfixName) => false,
            App => precedence < 0,
            _   => false
        };
    }
    
    private string PrintBound(Bound bound)
    {
        return bound.Index.ToString();
    }
    
    private string PrintConst(Const @const)
    {
        if (_constantRules.TryGetValue(@const.Name, out var display))
            return display;
        return @const.Name;
    }
    
    private string PrintFree(Free free)
    {
        return free.Name;
    }
    
    private string PrintInfTerm(InfTerm term)
    {
        return term switch
        {
            InfAbs abs      => PrintInfAbs(abs),
            InfApp app      => PrintInfApp(app),
            InfBound bound  => PrintInfBound(bound),
            InfConst @const => PrintInfConst(@const),
            InfFree free    => PrintInfFree(free),
            _            => base.Print(term)
        };
    }
    
    private string PrintInfAbs(InfAbs abs, string lambdaSymbol = "λ")
    {
        var output = new StringBuilder($"{lambdaSymbol} {abs.ParameterName ?? $": {abs.ParameterType}"}");

        while (abs.Body is InfAbs innerAbs)
        {
            abs = innerAbs;
            
            output.Append($" {abs.ParameterName ?? $": {abs.ParameterType}"}");
        }
        
        output.Append($" . {abs.Body}");
        
        return output.ToString();
    }
    
    private string PrintInfApp(InfApp app)
    {
        switch (app)
        {
            case {Application: InfConst {Name: var constName}, Argument: InfAbs abs} when _lambdaRules.TryGetValue(constName, out var lambdaDisplay):
                return PrintInfAbs(abs, lambdaDisplay.display);
            case {Application: InfApp {Application: InfConst {Name: var infixName}, Argument: var leftArg}, Argument: var rightArg} when _infixRules.TryGetValue(infixName, out var infixDisplay):
            {
                var leftPrecedence = infixDisplay.precedence;
                var rightPrecedence = infixDisplay.precedence;

                if (infixDisplay.leftAssociative)
                {
                    rightPrecedence++;
                }
                else
                {
                    leftPrecedence++;
                }

                return (InfBracketForPostfix(leftArg, leftPrecedence), InfBracketForPrefix(rightArg, rightPrecedence)) switch
                {
                    (true, true)   => $"({Print(leftArg)}) {infixDisplay.display} ({Print(rightArg)})",
                    (true, false)  => $"({Print(leftArg)}) {infixDisplay.display} {Print(rightArg)}",
                    (false, true)  => $"{Print(leftArg)} {infixDisplay.display} ({Print(rightArg)})",
                    (false, false) => $"{Print(leftArg)} {infixDisplay.display} {Print(rightArg)}"
                };
            }
            case {Application: InfConst {Name: var prefixName}, Argument: var arg} when _prefixRules.TryGetValue(prefixName, out var prefixDisplay):
            {
                var precedence = prefixDisplay.precedence;
                var bracket = InfBracketForPrefix(arg, precedence);
                return $"{prefixDisplay.display}{(bracket ? $"({Print(arg)})" : Print(arg))}";
            }
            case {Application: var arg, Argument: InfConst {Name: var postfixName}} when _postfixRules.TryGetValue(postfixName, out var postfixDisplay):
            {
                var precedence = postfixDisplay.precedence;
                var bracket = InfBracketForPostfix(arg, precedence);
                return $"{(bracket ? $"({Print(arg)})" : Print(arg))}{postfixDisplay.display}";
            }
        }
        
        return (InfBracketForPostfix(app.Application, 0), InfBracketForPrefix(app.Argument, 0)) switch
        {
            (true, true)   => $"({Print(app.Application)}) ({Print(app.Argument)})",
            (true, false)  => $"({Print(app.Application)}) {Print(app.Argument)}",
            (false, true)  => $"{Print(app.Application)} ({Print(app.Argument)})",
            (false, false) => $"{Print(app.Application)} {Print(app.Argument)}"
        };
    }
    
    private bool InfBracketForPrefix(InfTerm term, int precedence)
    {
        return term switch
        {
            InfApp { Application: InfConst { Name: var constName }, Argument: InfAbs } when _lambdaRules.ContainsKey(constName)
                => false,
            InfApp { Application: InfApp { Application: InfConst { Name: var infixName } } } when
                _infixRules.TryGetValue(infixName, out var infixDisplay) => infixDisplay.precedence > precedence,
            InfApp { Application: InfConst { Name: var prefixName } } when _prefixRules.ContainsKey(prefixName) => false,
            InfApp { Argument: InfConst { Name: var postfixName } } when _postfixRules.TryGetValue(postfixName,
                out var postfixDisplay) => postfixDisplay.precedence > precedence,
            InfApp => precedence > 0,
            _   => false
        };
    }
    
    private bool InfBracketForPostfix(InfTerm term, int precedence)
    {
        return term switch
        {
            InfAbs => true,
            InfApp { Application: InfConst { Name: var constName }, Argument: InfAbs } when _lambdaRules.ContainsKey(constName)
                => true,
            InfApp { Application: InfApp { Application: InfConst { Name: var infixName } } } when
                _infixRules.TryGetValue(infixName, out var infixDisplay) => infixDisplay.precedence > precedence,
            InfApp { Application: InfConst { Name: var prefixName } } when _prefixRules.TryGetValue(prefixName,
                out var prefixDisplay) => prefixDisplay.precedence > precedence,
            InfApp { Argument: InfConst { Name: var postfixName } } when _postfixRules.ContainsKey(postfixName) => false,
            InfApp => precedence < 0,
            _   => false
        };
    }
    
    private string PrintInfBound(InfBound bound)
    {
        return bound.Index.ToString();
    }
    
    private string PrintInfConst(InfConst @const)
    {
        if (_constantRules.TryGetValue(@const.Name, out var display))
            return display;
        return @const.Name;
    }
    
    private string PrintInfFree(InfFree free)
    {
        return free.Name;
    }
    
    private string PrintShadowTerm(ShadowTerm term)
    {
        return term switch
        {
            ShadowAbs abs      => PrintShadowAbs(abs),
            ShadowApp app      => PrintShadowApp(app),
            ShadowBound bound  => PrintShadowBound(bound),
            ShadowConst @const => PrintShadowConst(@const),
            ShadowFree free    => PrintShadowFree(free),
            _                  => base.Print(term)
        };
    }
    
    private string PrintShadowAbs(ShadowAbs abs, string lambdaSymbol = "λ")
    {
        var (body, free) = abs.GetBody();

        var output = new StringBuilder($"{lambdaSymbol} {free}");

        while (body is ShadowAbs innerAbs)
        {
            (body, free) = innerAbs.GetBody();
            
            output.Append($" {free}");
        }
        
        output.Append($" . {body}");
        
        return output.ToString();
    }
    
    private string PrintShadowApp(ShadowApp app)
    {
        switch (app)
        {
            case {Application: ShadowConst {Name: var constName}, Argument: ShadowAbs abs} when _lambdaRules.TryGetValue(constName, out var lambdaDisplay):
                return PrintShadowAbs(abs, lambdaDisplay.display);
            case {Application: ShadowApp {Application: ShadowConst {Name: var infixName}, Argument: var leftArg}, Argument: var rightArg} when _infixRules.TryGetValue(infixName, out var infixDisplay):
            {
                var leftPrecedence = infixDisplay.precedence;
                var rightPrecedence = infixDisplay.precedence;

                if (infixDisplay.leftAssociative)
                {
                    rightPrecedence++;
                }
                else
                {
                    leftPrecedence++;
                }

                return (ShadowBracketForPostfix(leftArg, leftPrecedence), ShadowBracketForPrefix(rightArg, rightPrecedence)) switch
                {
                    (true, true)   => $"({Print(leftArg)}) {infixDisplay.display} ({Print(rightArg)})",
                    (true, false)  => $"({Print(leftArg)}) {infixDisplay.display} {Print(rightArg)}",
                    (false, true)  => $"{Print(leftArg)} {infixDisplay.display} ({Print(rightArg)})",
                    (false, false) => $"{Print(leftArg)} {infixDisplay.display} {Print(rightArg)}"
                };
            }
            case {Application: ShadowConst {Name: var prefixName}, Argument: var arg} when _prefixRules.TryGetValue(prefixName, out var prefixDisplay):
            {
                var precedence = prefixDisplay.precedence;
                var bracket = ShadowBracketForPrefix(arg, precedence);
                return $"{prefixDisplay.display}{(bracket ? $"({Print(arg)})" : Print(arg))}";
            }
            case {Application: var arg, Argument: ShadowConst {Name: var postfixName}} when _postfixRules.TryGetValue(postfixName, out var postfixDisplay):
            {
                var precedence = postfixDisplay.precedence;
                var bracket = ShadowBracketForPostfix(arg, precedence);
                return $"{(bracket ? $"({Print(arg)})" : Print(arg))}{postfixDisplay.display}";
            }
        }
        
        return (ShadowBracketForPostfix(app.Application, 0), ShadowBracketForPrefix(app.Argument, 0)) switch
        {
            (true, true)   => $"({Print(app.Application)}) ({Print(app.Argument)})",
            (true, false)  => $"({Print(app.Application)}) {Print(app.Argument)}",
            (false, true)  => $"{Print(app.Application)} ({Print(app.Argument)})",
            (false, false) => $"{Print(app.Application)} {Print(app.Argument)}"
        };
    }
    
    private bool ShadowBracketForPrefix(ShadowTerm term, int precedence)
    {
        return term switch
        {
            ShadowApp { Application: ShadowConst { Name: var constName }, Argument: ShadowAbs } when _lambdaRules.ContainsKey(constName)
                => false,
            ShadowApp { Application: ShadowApp { Application: ShadowConst { Name: var infixName } } } when
                _infixRules.TryGetValue(infixName, out var infixDisplay) => infixDisplay.precedence > precedence,
            ShadowApp { Application: ShadowConst { Name: var prefixName } } when _prefixRules.ContainsKey(prefixName) => false,
            ShadowApp { Argument: ShadowConst { Name: var postfixName } } when _postfixRules.TryGetValue(postfixName,
                out var postfixDisplay) => postfixDisplay.precedence > precedence,
            ShadowApp => precedence > 0,
            _   => false
        };
    }
    
    private bool ShadowBracketForPostfix(ShadowTerm term, int precedence)
    {
        return term switch
        {
            ShadowAbs => true,
            ShadowApp { Application: ShadowConst { Name: var constName }, Argument: ShadowAbs } when _lambdaRules.ContainsKey(constName)
                => true,
            ShadowApp { Application: ShadowApp { Application: ShadowConst { Name: var infixName } } } when
                _infixRules.TryGetValue(infixName, out var infixDisplay) => infixDisplay.precedence > precedence,
            ShadowApp { Application: ShadowConst { Name: var prefixName } } when _prefixRules.TryGetValue(prefixName,
                out var prefixDisplay) => prefixDisplay.precedence > precedence,
            ShadowApp { Argument: ShadowConst { Name: var postfixName } } when _postfixRules.ContainsKey(postfixName) => false,
            ShadowApp => precedence < 0,
            _   => false
        };
    }
    
    private string PrintShadowBound(ShadowBound bound)
    {
        return bound.Index.ToString();
    }
    
    private string PrintShadowConst(ShadowConst @const)
    {
        if (_constantRules.TryGetValue(@const.Name, out var display))
            return display;
        return @const.Name;
    }
    
    private string PrintShadowFree(ShadowFree free)
    {
        return free.Name;
    }
    
    public void AddConstantTypeRule(string name, string display)
    {
        _typePrinter.AddConstantRule(name, display);
    }
    
    public void AddInfixTypeRule(string name, string display, bool leftAssociative, int precedence)
    {
        _typePrinter.AddInfixRule(name, display, leftAssociative, precedence);
    }
    
    public void AddPrefixTypeRule(string name, string display, int precedence)
    {
        _typePrinter.AddPrefixRule(name, display, precedence);
    }
    
    public void AddPostfixTypeRule(string name, string display, int precedence)
    {
        _typePrinter.AddPostfixRule(name, display, precedence);
    }
    
    public void AddConstantTermRule(string name, string display)
    {
        _constantRules.Add(name, display);
    }
    
    public void AddInfixTermRule(string name, string display, bool leftAssociative, int precedence)
    {
        _infixRules.Add(name, (display, leftAssociative, precedence));
    }
    
    public void AddPrefixTermRule(string name, string display, int precedence)
    {
        _prefixRules.Add(name, (display, precedence));
    }
    
    public void AddPostfixTermRule(string name, string display, int precedence)
    {
        _postfixRules.Add(name, (display, precedence));
    }
    
    public void AddLambdaTermRule(string name, string display, int precedence)
    {
        _lambdaRules.Add(name, (display, precedence));
    }
    
    public void RemoveConstantTermRule(string name)
    {
        _constantRules.Remove(name);
    }
    
    public void RemoveInfixTermRule(string name)
    {
        _infixRules.Remove(name);
    }
    
    public void RemovePrefixTermRule(string name)
    {
        _prefixRules.Remove(name);
    }
    
    public void RemovePostfixTermRule(string name)
    {
        _postfixRules.Remove(name);
    }
    
    public void RemoveLambdaTermRule(string name)
    {
        _lambdaRules.Remove(name);
    }
    
    public void RemoveConstantTypeRule(string name)
    {
        _typePrinter.RemoveConstantRule(name);
    }
    
    public void RemoveInfixTypeRule(string name)
    {
        _typePrinter.RemoveInfixRule(name);
    }
    
    public void RemovePrefixTypeRule(string name)
    {
        _typePrinter.RemovePrefixRule(name);
    }
    
    public void RemovePostfixTypeRule(string name)
    {
        _typePrinter.RemovePostfixRule(name);
    }
    
    private Dictionary<string, (string display, bool leftAssociative, int precedence)> _infixRules = new();
    private Dictionary<string, (string display, int precedence)> _prefixRules = new();
    private Dictionary<string, (string display, int precedence)> _postfixRules = new();
    private Dictionary<string, string> _constantRules = new();
    private Dictionary<string, (string display, int precedence)> _lambdaRules = new();
}