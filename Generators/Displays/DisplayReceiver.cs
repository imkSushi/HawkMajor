using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generators.Displays;

public class DisplayReceiver : ISyntaxContextReceiver
{
    public Dictionary<string, DisplayValues> ParseDisplayAttributes = new();
    
    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclarationSyntax || methodDeclarationSyntax.AttributeLists.Count == 0)
            return;
            
        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;
        if (!methodSymbol!.GetAttributes()
                .Any(ad => ad.AttributeClass!.ToDisplayString() == "HawkMajor2.ParseDisplayAttribute")) 
            return;
        
        var attribute = methodSymbol.GetAttributes()
            .First(ad => ad.AttributeClass!.ToDisplayString() == "HawkMajor2.ParseDisplayAttribute");
        var hasAssociativity = (bool)attribute.ConstructorArguments[0].Value!;
        var hasPrecedence = (bool)attribute.ConstructorArguments[1].Value!;
        
        var name = methodSymbol.Name;
        if (name[..5] != "Parse")
            return;
        
        ParseDisplayAttributes.Add(name[5..], new DisplayValues(hasAssociativity, hasPrecedence));
    }
}