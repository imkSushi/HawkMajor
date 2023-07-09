using HawkMajor2.NameGenerators;
using Printing;
using Results;
using Valiant;
using Valiant.Types;

namespace HawkMajor2.Language.Inference.Types;

public abstract record InfType : IPrintable
{
    public abstract Result<Type> BindTypes(Kernel kernel, Dictionary<string, string> unboundTypeNames, NameGenerator nameGenerator);
    
    public static InfType FromType(Type type)
    {
        return type switch
        {
            TyVar(var name) => new InfTyVar(name),
            TyApp(var name, var args) => new InfTyApp(name,
                args.Select(FromType).ToArray()),
            _ => throw new InvalidOperationException()
        };
    }
    
    internal abstract InfType ConvertUnboundTypeToFunction(string name);
    internal abstract InfType UniquifyUnboundTypeNames(NameGenerator generator, Dictionary<string, string> unboundTypeNames);
    
    internal static InfType UnboundFromType(Type type, NameGenerator generator, Dictionary<string, string> typeMap)
    {
        switch (type)
        {
            case TyApp(var name, var args):
            {
                var newArgs = args.Select(arg => UnboundFromType(arg, generator, typeMap)).ToArray();
                
                return new InfTyApp(name, newArgs);
            }
            case TyVar(var name):
            {
                if (typeMap.TryGetValue(name, out var newName))
                    return new InfTyUnbound(newName);

                generator.MoveNext();
                newName = generator.Current;
                typeMap[name] = newName;
                
                return new InfTyUnbound(newName);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
    
    internal static InfType SingleUnboundFromType(Type type)
    {
        switch (type)
        {
            case TyApp(var name, var args):
            {
                var newArgs = args.Select(arg => (InfType) new InfTyFixed(arg)).ToArray();
                
                return new InfTyApp(name, newArgs);
            }
            case TyVar(var name):
            {
                return new InfTyVar(name);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
    internal abstract InfType SubstituteType(string name, InfType type);
    internal abstract Result CheckUnboundNameLoop(string name);
    internal abstract void GetUsedTyVarNames(HashSet<string> names);

    internal static void GetUsedTyVarNames(Type type, HashSet<string> names)
    {
        switch (type)
        {
            case TyApp { Args: var args }:
            {
                foreach (var arg in args)
                    GetUsedTyVarNames(arg, names);
                break;
            }
            case TyVar { Name: var name }:
            {
                names.Add(name);
                break;
            }
        }
    }

    internal abstract InfType NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map);
    internal abstract Result<Type> BindType(Kernel kernel);

    public sealed override string ToString()
    {
        return Printer.UniversalPrinter.Print(this);
    }

    public abstract string DefaultPrint();
}