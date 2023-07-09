using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generators.Results;

public class InlineResultReceiver : ISyntaxContextReceiver
{
    public readonly HashSet<(MethodDeclarationSyntax syntax, IMethodSymbol symbol)> Methods = new();
    
    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclarationSyntax || methodDeclarationSyntax.AttributeLists.Count == 0)
            return;
            
        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;
        if (!methodSymbol!.GetAttributes()
                .Any(ad => ad.AttributeClass!.ToDisplayString() == "HawkMajor2.InlineResultAttribute")) 
            return;
        
        Methods.Add((methodDeclarationSyntax, methodSymbol));
    }
}