using System.Collections.ObjectModel;
using Results;

namespace Valiant.Types;

public sealed record TyApp : Type
{
    internal TyApp(string name, ReadOnlyCollection<Type> args)
    {
        Name = name;
        Args = args;
    }
    
    internal TyApp(string name, params Type[] args)
    {
        Name = name;
        Args = args.AsReadOnly();
    }
    
    public string Name { get; }
    public ReadOnlyCollection<Type> Args { get; }
    
    public void Deconstruct(out string name, out ReadOnlyCollection<Type> args)
    {
        name = Name;
        args = Args;
    }

    public bool Equals(TyApp? other)
    {
        if (other is null)
            return false;
        
        var (name, args) = other;

        return Name == name && Args.SequenceEqual(args);
    }

    public override TyApp Instantiate(Dictionary<TyVar, Type> map)
    {
        return new TyApp(Name, Args.Select(arg => arg.Instantiate(map)).ToArray());
    }

    public override void FreeTypesIn(HashSet<TyVar> frees)
    {
        foreach (var type in Args)
        {
            type.FreeTypesIn(frees);
        }
    }

    public override void FreeTypeNamesIn(HashSet<string> frees)
    {
        foreach (var type in Args)
        {
            type.FreeTypeNamesIn(frees);
        }
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Name);
        foreach (var type in Args)
        {
            hashCode.Add(type);
        }
        
        return hashCode.ToHashCode();
    }

    public override Result GenerateSubstitutionMapping(Type desired, Dictionary<TyVar, Type> typeMap)
    {
        if (desired is not TyApp {Name: var name, Args: var args})
            return $"Type {desired} is not a type application";
        
        if (Name != name)
            return $"Type application {Name} does not match {name}";
        
        for (var i = 0; i < Args.Count; i++)
        {
            if (Args[i].GenerateSubstitutionMapping(args[i], typeMap).IsError(out var error))
                return error;
        }
        
        return Result.Success;
    }

    public override string DefaultPrint()
    {
        return $"{Name}<{string.Join(", ", Args)}>";
    }
}