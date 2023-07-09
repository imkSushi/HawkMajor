using System.Collections.ObjectModel;
using HawkMajor2.Shadows.ShadowTypes.MatchData;
using Results;
using Valiant;
using Valiant.Types;

namespace HawkMajor2.Shadows.ShadowTypes;

public sealed record ShadowTyApp : ShadowType
{
    internal ShadowTyApp(string name, ReadOnlyCollection<ShadowType> args)
    {
        Name = name;
        Args = args;
    }
    
    internal ShadowTyApp(string name, params ShadowType[] args)
    {
        Name = name;
        Args = args.AsReadOnly();
    }
    
    public string Name { get; }
    public ReadOnlyCollection<ShadowType> Args { get; }
    
    public void Deconstruct(out string name, out ReadOnlyCollection<ShadowType> args)
    {
        name = Name;
        args = Args;
    }

    public bool Equals(ShadowTyApp? other)
    {
        if (other is null)
            return false;
        
        var (name, args) = other;

        return Name == name && Args.SequenceEqual(args);
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

    public override string DefaultPrint()
    {
        return $"{Name}<{string.Join(", ", Args)}>";
    }

    public override bool ContainsUnfixed()
    {
        return Args.Any(arg => arg.ContainsUnfixed());
    }

    public override bool Match(Type type, MatchTypeData maps)
    {
        if (type is not TyApp tyApp)
            return false;
        
        if (tyApp.Name != Name)
            return false;

        if (tyApp.Args.Count != Args.Count)
            return false;

        return Args.Zip(tyApp.Args).All(pair => pair.First.Match(pair.Second, maps));
    }

    public override bool Match(ShadowType type, MatchShadowTypeData maps)
    {
        if (type is not ShadowTyApp tyApp)
            return false;
        
        if (tyApp.Name != Name)
            return false;

        if (tyApp.Args.Count != Args.Count)
            return false;

        return Args.Zip(tyApp.Args).All(pair => pair.First.Match(pair.Second, maps));
    }

    public override Result<ShadowType> RemoveFixedTerms(Dictionary<ShadowTyFixed, ShadowType> typeMap)
    {
        var args = new List<ShadowType>();
        
        foreach (var arg in Args)
        {
            if (!arg.RemoveFixedTerms(typeMap).Deconstruct(out var result, out var error))
                return error;
            
            args.Add(result);
        }
        
        return new ShadowTyApp(Name, args.AsReadOnly());
    }

    public override Result<Type> ConvertToType(Kernel kernel)
    {
        var args = new List<Type>();
        
        foreach (var arg in Args)
        {
            if (!arg.ConvertToType(kernel).Deconstruct(out var result, out var error))
                return error;
            
            args.Add(result);
        }

        return Result<Type>.Cast(kernel.MakeType(Name, args.ToArray()));
    }

    public override Result<Type> ConvertToType(Dictionary<ShadowTyFixed, Type> typeMap, Kernel kernel)
    {
        var args = new List<Type>();
        
        foreach (var arg in Args)
        {
            if (!arg.ConvertToType(typeMap, kernel).Deconstruct(out var result, out var error))
                return error;
            
            args.Add(result);
        }

        return Result<Type>.Cast(kernel.MakeType(Name, args.ToArray()));
    }

    public override ShadowType FixTypes(HashSet<string> toFix)
    {
        return new ShadowTyApp(Name, Args.Select(arg => arg.FixTypes(toFix)).ToArray());
    }

    public static ShadowTyApp FromTyApp(TyApp type, Dictionary<string, ShadowVarType> fixedTypes)
    {
        var args = type.Args.Select(arg => ToShadowType(arg, fixedTypes)).ToList();

        return new ShadowTyApp(type.Name, args.AsReadOnly());
    }
}