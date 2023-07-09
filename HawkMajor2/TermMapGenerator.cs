using System.Diagnostics.CodeAnalysis;
using HawkMajor2.Extensions;
using Valiant.Terms;
using Valiant.Types;

namespace HawkMajor2;

public static class TermMapGenerator
{
    public static bool GenerateMap(Term term,
        Term template,
        [MaybeNullWhen(false)] out Dictionary<Free, Term> termMap,
        [MaybeNullWhen(false)] out Dictionary<TyVar, Type> typeMap)
    {
        termMap = new Dictionary<Free, Term>();
        typeMap = new Dictionary<TyVar, Type>();
        return GenerateMap(term, template, termMap, typeMap, new Stack<(Free term, Free template)>());
    }
    
    public static bool GenerateMap(Term term,
        Term template,
        Dictionary<Free, Term> termMap,
        Dictionary<TyVar, Type> typeMap,
        Stack<(Free term, Free template)> boundStack)
    {
        switch (template)
        {
            case Abs templateAbs:
                if (term is not Abs termAbs)
                    return false;
                if (!GenerateMap(termAbs.ParameterType, templateAbs.ParameterType, typeMap))
                    return false;
                
                var (templateBody, templateVar) = templateAbs.GetBody();
                var (termBody, termVar) = termAbs.GetBody();
                
                boundStack.Push((termVar, templateVar));
                
                if (!GenerateMap(termBody, templateBody, termMap, typeMap, boundStack))
                    return false;
                
                boundStack.Pop();
                return true;
                
            case App(var templateApp, var templateArg):
                if (term is not App(var termApp, var termArg))
                    return false;
                return GenerateMap(termApp, templateApp, termMap, typeMap, boundStack) &&
                       GenerateMap(termArg, templateArg, termMap, typeMap, boundStack);
            
            case Const(var templateName, var templateType):
            {
                if (term is not Const(var termName, var termType))
                    return false;
                if (templateName != termName)
                    return false;
                return GenerateMap(termType, templateType, typeMap);
            }
            
            case Free templateFree:
            {
                if (term is Free termFree && boundStack.Any(bound => bound.template == templateFree && bound.term == termFree))
                    return true;
                
                if (boundStack.Any(x => x.template == templateFree || term.IsFree(x.term)))
                    return false;
                
                if (termMap.TryGetValue(templateFree, out var termValue))
                    return termValue == term;
                
                if (!GenerateMap(term.TypeOf(), templateFree.Type, typeMap))
                    return false;
                
                termMap[templateFree] = term;
                
                return true;
            }
        }
        
        throw new ArgumentOutOfRangeException();
    }
    
    public static bool GenerateMap(Type type,
        Type template,
        [MaybeNullWhen(false)] out Dictionary<TyVar, Type> typeMap)
    {
        typeMap = new Dictionary<TyVar, Type>();
        return GenerateMap(type, template, typeMap);
    }
    
    private static bool GenerateMap(Type term,
        Type template,
        Dictionary<TyVar, Type> typeMap)
    {
        switch (template)
        {
            case TyApp(var templateName, var templateArgs):
                if (term is not TyApp(var termName, var termArgs))
                    return false;
                if (templateName != termName)
                    return false;
                if (templateArgs.Count != termArgs.Count)
                    return false;


                return termArgs.Zip(templateArgs).All(arg => GenerateMap(arg.First, arg.Second, typeMap));
            case TyVar templateTyVar:
                if (typeMap.TryGetValue(templateTyVar, out var type))
                    return type == term;
                
                typeMap[templateTyVar] = term;
                return true;
        }

        throw new ArgumentOutOfRangeException();
    }
}