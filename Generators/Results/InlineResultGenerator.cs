using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generators.Results;

[Generator]
public class InlineResultGenerator : ISourceGenerator
{
    private const string AttributeText = """
                                         using System;

                                         namespace HawkMajor2;

                                         [AttributeUsage(AttributeTargets.Method)]
                                         public class InlineResultAttribute : Attribute
                                         {
                                             public string[] ValueNames;
                                             
                                             public InlineResultAttribute()
                                             {
                                                ValueNames = new string[0];
                                             }
                                             
                                             public InlineResultAttribute(string name)
                                             {
                                                 ValueNames = new[] {name};
                                             }
                                             
                                             public InlineResultAttribute(string name1, string name2)
                                             {
                                                 ValueNames = new[] {name1, name2};
                                             }
                                             
                                             public InlineResultAttribute(string name1, string name2, string name3)
                                             {
                                                 ValueNames = new[] {name1, name2, name3};
                                             }
                                             
                                             public InlineResultAttribute(string name1, string name2, string name3, string name4)
                                             {
                                                 ValueNames = new[] {name1, name2, name3, name4};
                                             }
                                             
                                             public InlineResultAttribute(string name1, string name2, string name3, string name4, string name5)
                                             {
                                                 ValueNames = new[] {name1, name2, name3, name4, name5};
                                             }
                                         }
                                         """;
    
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(i => i.AddSource("ParseDisplayAttribute.g.cs", AttributeText));
        context.RegisterForSyntaxNotifications(() => new InlineResultReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var outputs = new Dictionary<(string, string), StringBuilder>();
        
        var receiver = (InlineResultReceiver) context.SyntaxContextReceiver!;
        
        foreach (var (syntax, symbol) in receiver.Methods)
        {
            GenerateMethod(symbol, outputs, syntax);
        }
        
        foreach (var tuple in outputs)
        {
            GenerateFileEnd(tuple.Value);
            context.AddSource($"{tuple.Key.Item1}-{tuple.Key.Item2}.g.cs", tuple.Value.ToString());
        }
    }

    private static void GenerateMethod(IMethodSymbol symbol, Dictionary<(string, string), StringBuilder> outputs, MethodDeclarationSyntax syntax)
    {
        var classSymbol = symbol.ContainingType;
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
        var className = classSymbol.Name;

        StringBuilder output;

        if (outputs.ContainsKey((namespaceName, className)))
        {
            output = outputs[(namespaceName, className)];
            output.AppendLine();
        }
        else
        {
            output = new StringBuilder();
            GenerateFileStart(output, namespaceName, className);
            outputs[(namespaceName, className)] = output;
        }

        output.Append($"    {syntax.Modifiers} bool {syntax.Identifier}(");
        foreach (var parameter in syntax.ParameterList.Parameters)
        {
            output.Append($"{parameter}, ");
        }

        switch (syntax.ReturnType.ToString())
        {
            case "Result":
                GenerateVoidResultMethod(symbol, output, syntax);
                break;
            case "StringResult":
                GenerateStringResultMethod(symbol, output, syntax);
                break;
            default:
                GenerateValueResultMethod(symbol, output, syntax);
                break;
        }
    }
    
    private static void GenerateVoidResultMethod(IMethodSymbol symbol, StringBuilder output, MethodDeclarationSyntax syntax)
    {

        var parameterNames = GetAttributeParameterNames(symbol).ToList();

        switch (parameterNames.Count)
        {
            case 0:
                parameterNames.Add("error");
                break;
            case 1:
                break;
            default:
                throw new Exception("Too many parameters");
        }

        output.AppendLine($"[MaybeNullWhen(true)] out string {parameterNames[0]})");

        output.AppendLine("    {");

        output.Append($"        return {syntax.Identifier}(");
        var first = true;
        foreach (var parameter in syntax.ParameterList.Parameters)
        {
            if (first)
                first = false;
            else
                output.Append(", ");

            foreach (var modifier in parameter.Modifiers)
            {
                output.Append($"{modifier} ");
            }

            output.Append($"{parameter.Identifier}");
        }

        output.AppendLine($").Deconstruct(out {parameterNames[0]});");
        output.AppendLine("    }");
    }
    
    private static void GenerateStringResultMethod(IMethodSymbol symbol, StringBuilder output, MethodDeclarationSyntax syntax)
    {
        var parameterNames = GetAttributeParameterNames(symbol).ToList();

        switch (parameterNames.Count)
        {
            case 0:
                parameterNames.Add("value");
                parameterNames.Add("error");
                break;
            case 1:
                parameterNames.Add("error");
                break;
            default:
                throw new Exception("Too many parameters");
        }

        output.AppendLine($"[MaybeNullWhen(false)] out string {parameterNames[0]}, [MaybeNullWhen(true)] out string {parameterNames[1]})");
        
        output.AppendLine("    {");
        
        output.Append($"        return {syntax.Identifier}(");
        var first = true;
        foreach (var parameter in syntax.ParameterList.Parameters)
        {
            if (first)
                first = false;
            else
                output.Append(", ");

            foreach (var modifier in parameter.Modifiers)
            {
                output.Append($"{modifier} ");
            }

            output.Append($"{parameter.Identifier}");
        }
        
        output.AppendLine($").Deconstruct(out {parameterNames[0]}, out {parameterNames[1]});");
        output.AppendLine("    }");
    }
    
    private static void GenerateValueResultMethod(IMethodSymbol symbol, StringBuilder output, MethodDeclarationSyntax syntax)
    {
        var parameterNames = GetAttributeParameterNames(symbol).ToList();
        var types = ((GenericNameSyntax) syntax.ReturnType).TypeArgumentList.Arguments.Select(x => x.ToString()).ToList();

        while (parameterNames.Count < types.Count)
        {
            parameterNames.Add($"value{parameterNames.Count}");
        }

        if (parameterNames.Count == types.Count)
        {
            parameterNames.Add("error");
        }
        
        if (parameterNames.Count > types.Count + 1)
        {
            throw new Exception("Too many parameters");
        }
        
        for (var i = 0; i < types.Count; i++)
        {
            output.Append($"[MaybeNullWhen(false)] out {types[i]} {parameterNames[i]}, ");
        }

        output.AppendLine($"[MaybeNullWhen(true)] out string {parameterNames[^1]})");
        
        output.AppendLine("    {");
        
        output.Append($"        return {syntax.Identifier}(");
        var first = true;
        foreach (var parameter in syntax.ParameterList.Parameters)
        {
            if (first)
                first = false;
            else
                output.Append(", ");

            foreach (var modifier in parameter.Modifiers)
            {
                output.Append($"{modifier} ");
            }

            output.Append($"{parameter.Identifier}");
        }
        
        output.Append(").Deconstruct(");
        
        for (var i = 0; i < types.Count; i++)
        {
            output.Append($"out {parameterNames[i]}, ");
        }
        
        output.AppendLine($"out {parameterNames[^1]});");
        
        output.AppendLine("    }");
    }

    private static void GenerateFileStart(StringBuilder output, string namespaceName, string className)
    {
        output.AppendLine($$"""
                    using System.Diagnostics.CodeAnalysis;
                    using Results;

                    namespace {{namespaceName}};
                    
                    public partial class {{className}}
                    {
                    """);
    }
    
    private static void GenerateFileEnd(StringBuilder output)
    {
        output.AppendLine("}");
    }
    
    private static IEnumerable<string> GetAttributeParameterNames(IMethodSymbol symbol)
    {
        var attribute = symbol.GetAttributes()
            .First(ad => ad.AttributeClass!.ToDisplayString() == "HawkMajor2.InlineResultAttribute");

        return attribute.ConstructorArguments
            .Select(a => (string)a.Value!);
    }
}