using HawkMajor2.NameGenerators;
using Results;
using Valiant;

namespace HawkMajor2.Language.Inference.Types;

public sealed record InfTyApp(string Name, InfType[] Args) : InfType
{
    public override Result<Type> BindTypes(Kernel kernel, Dictionary<string, string> unboundTypeNames, NameGenerator nameGenerator)
    {
        var args = new Type[Args.Length];
        
        for (var i = 0; i < Args.Length; i++)
        {
            if (Args[i].BindTypes(kernel, unboundTypeNames, nameGenerator).Deconstruct(out var arg, out var err))
                args[i] = arg;
            else
                return err;
        }
        
        return Result<Type>.Cast(kernel.MakeType(Name, args));
    }

    internal override InfType ConvertUnboundTypeToFunction(string name)
    {
        var replacements = false;
        var newArgs = new InfType[Args.Length];
        for (var i = 0; i < Args.Length; i++)
        {
            var newArg = Args[i].ConvertUnboundTypeToFunction(name);
            if (newArg != Args[i])
                replacements = true;
            newArgs[i] = newArg;
        }
        
        if (replacements)
            return new InfTyApp(Name, newArgs);
        
        return this;
    }

    internal override InfType UniquifyUnboundTypeNames(NameGenerator generator, Dictionary<string, string> unboundTypeNames)
    {
        var replacements = false;
        var newArgs = new InfType[Args.Length];
        for (var i = 0; i < Args.Length; i++)
        {
            var newArg = Args[i].UniquifyUnboundTypeNames(generator, unboundTypeNames);
            if (newArg != Args[i])
                replacements = true;
            newArgs[i] = newArg;
        }
        
        if (replacements)
            return new InfTyApp(Name, newArgs);
        
        return this;
    }

    internal override InfType SubstituteType(string name, InfType type)
    {
        var replacements = false;
        var newArgs = new InfType[Args.Length];
        for (var i = 0; i < Args.Length; i++)
        {
            var newArg = Args[i].SubstituteType(name, type);
            if (newArg != Args[i])
                replacements = true;
            newArgs[i] = newArg;
        }
        
        if (replacements)
            return new InfTyApp(Name, newArgs);
        
        return this;
    }

    internal override Result CheckUnboundNameLoop(string name)
    {
        foreach (var arg in Args)
        {
            if (arg.CheckUnboundNameLoop(name).IsError(out var error))
                return error;
        }
        
        return Result.Success;
    }

    internal override void GetUsedTyVarNames(HashSet<string> names)
    {
        foreach (var arg in Args)
        {
            arg.GetUsedTyVarNames(names);
        }
    }

    internal override InfType NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map)
    {
        var replacements = false;
        var newArgs = new InfType[Args.Length];
        for (var i = 0; i < Args.Length; i++)
        {
            var newArg = Args[i].NicelyRename(nameGenerator, map);
            if (newArg != Args[i])
                replacements = true;
            newArgs[i] = newArg;
        }
        
        if (replacements)
            return new InfTyApp(Name, newArgs);
        
        return this;
    }

    internal override Result<Type> BindType(Kernel kernel)
    {
        var args = new Type[Args.Length];
        
        for (var i = 0; i < Args.Length; i++)
        {
            if (Args[i].BindType(kernel).Deconstruct(out var arg, out var err))
                args[i] = arg;
            else
                return err;
        }
        
        return Result<Type>.Cast(kernel.MakeType(Name, args));
    }

    public override string DefaultPrint()
    {
        return $"{Name}<{string.Join(", ", Args.Select(a => a.DefaultPrint()))}>";
    }

    public bool Equals(InfTyApp? other)
    {
        if (other is null)
            return false;
        
        if (other.Name != Name)
            return false;

        if (other.Args.Length != Args.Length)
            return false;

        for (var i = 0; i < Args.Length; i++)
        {
            if (other.Args[i] != Args[i])
                return false;
        }
        
        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        foreach (var arg in Args)
            hash.Add(arg);
        
        return hash.ToHashCode();
    }
}