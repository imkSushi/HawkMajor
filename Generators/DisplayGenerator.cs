using System;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Generators;

[Generator]
public class DisplayGenerator : ISourceGenerator
{
    private const string attributeText = @"using System;

namespace HawkMajor2;

[AttributeUsage(AttributeTargets.Method)]
public class ParseDisplayAttribute : Attribute
{
    public bool HasAssociativity;
    public bool HasPrecedence;
    
    public ParseDisplayAttribute(bool hasAssociativity, bool hasPrecedence)
    {
        HasAssociativity = hasAssociativity;
        HasPrecedence = hasPrecedence;
    }
}";
    
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization((i) => i.AddSource("ParseDisplayAttribute.g.cs", attributeText));
        context.RegisterForSyntaxNotifications(() => new DisplayReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var receiver = (DisplayReceiver) context.SyntaxContextReceiver!;
        var output = new StringBuilder(
"""
using System.Text;
using HawkMajor2.Engine;
using HawkMajor2.Engine.Displays;
using HawkMajor2.Engine.Displays.Terms;
using HawkMajor2.Engine.Displays.Types;
using HawkMajor2.Language.Lexing;
using HawkMajor2.Language.Lexing.Tokens;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Language.Parsing;

public partial class ScriptParser
{
""");
        Console.WriteLine("DisplayGenerator");
        foreach (var keyValuePair in receiver.ParseDisplayAttributes)
        {
            var type = keyValuePair.Key;
            Console.WriteLine(type);
            var displayAttribute = keyValuePair.Value;

            output.Append($$"""
                            private partial Result Parse{{type}}()
                            {
                                if (!ExpectIdentifierName().Deconstruct(out var name, out var error))
                                    return error;

                                if (!ExpectIdentifierName().Deconstruct(out var symbol, out error))
                                    return error;
                                
                                if (!ExpectIdentifierName().Deconstruct(out var displayName, out error))
                                    return error;
                          """);

            if (displayAttribute.HasAssociativity)
            {
                output.Append("""
        
                                if (!ExpectIdentifierName().Deconstruct(out var associativityString, out error))
                                    return error;
                                
                                bool associativity;
                                
                                switch (associativityString)
                                {
                                    case "left":
                                        associativity = true;
                                        break;
                                    case "right":
                                        associativity = false;
                                        break;
                                    default:
                                        return _lexer.GenerateError($"Expected associativity, got {associativityString}");
                                }
                          """);
            }

            if (displayAttribute.HasPrecedence)
            {
                output.Append("""
        
                                if (!ExpectIdentifierName().Deconstruct(out var precedenceString, out error))
                                    return error;
                                
                                if (!int.TryParse(precedenceString, out var precedence))
                                    return _lexer.GenerateError($"Expected integer precedence, got {precedenceString}");
                          """);
            }
            
            output.Append("""
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
                          """);

            output.Append($"_displayManager.ApplyDisplay(new {type}(name, symbol, displayName,");
            if (displayAttribute.HasAssociativity)
                output.Append(" associativity,");
            if (displayAttribute.HasPrecedence)
                output.Append(" precedence,");
            output.Append(" interrupt, verify));");
            
            output.Append("""
                            return Result.Success;
                        }
                       """);
        }
        
        output.Append("}");
        
        context.AddSource("GeneratedDisplayParser", output.ToString());
    }
}