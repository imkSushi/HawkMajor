using HawkMajor2.Language.Inference.Types;
using HawkMajor2.NameGenerators;
using Results;
using Valiant;
using Valiant.Terms;
using Valiant.Types;

namespace HawkMajor2.Language.Inference.Terms;

public sealed record InfConst(string Name, InfType Type) : InfTerm
{
    internal override Result<InfTerm> FixCombTypes()
    {
        return this;
    }

    internal override InfType TypeOf()
    {
        return Type;
    }

    internal override InfTerm ConvertUnboundTypeToFunction(string name)
    {
        var newType = Type.ConvertUnboundTypeToFunction(name);
        if (newType == Type)
            return this;
        
        return new InfConst(Name, newType);
    }

    internal override InfTerm UniquifyUnboundTypeNames(NameGenerator generator)
    {
        var dict = new Dictionary<string, string>();
        var newType = Type.UniquifyUnboundTypeNames(generator, dict);
        
        if (dict.Count == 0)
            return this;
        
        return new InfConst(Name, newType);
    }

    internal override Result GenerateMappings(Kernel kernel,
        HashSet<(InfType, InfType)> mappings,
        NameGenerator nameGenerator)
    {
        if (!kernel.ConstantTypes.TryGetValue(Name, out var type))
            return $"Unknown constant {Name}";
        
        var unboundType = InfType.UnboundFromType(type, nameGenerator, new Dictionary<string, string>());
        
        mappings.Add((unboundType, Type));
        
        return Result.Success;
    }

    internal override HashSet<InfType> GetTypesOfBoundVariable(int depth)
    {
        return new HashSet<InfType>();
    }

    internal override InfTerm FixAbsVariables(Stack<string?> nameStack)
    {
        return this;
    }

    internal override InfTerm SubstituteType(string name, InfType type)
    {
        var newType = Type.SubstituteType(name, type);
        if (newType == Type)
            return this;
        
        return new InfConst(Name, newType);
    }

    internal override void GetUsedTyVarNames(HashSet<string> names)
    {
        Type.GetUsedTyVarNames(names);
    }

    internal override InfTerm NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map)
    {
        var newType = Type.NicelyRename(nameGenerator, map);
        if (newType == Type)
            return this;
        
        return new InfConst(Name, newType);
    }

    internal override Result<Term> BindTerm(Kernel kernel, Stack<string> temporaryBoundVariableNames, NameGenerator nameGenerator)
    {
        if (!kernel.ConstantTypes.TryGetValue(Name, out var type))
            return $"Unknown constant {Name}";
        
        if (!GenerateTypeSubstitution(kernel).Deconstruct(out var substitution, out var error))
            return error;

        return Result<Term>.Cast(kernel.MakeConst(Name, substitution));
    }
    
    public Result<Dictionary<TyVar, Type>> GenerateTypeSubstitution(Kernel kernel)
    {
        if (!kernel.ConstantTypes.TryGetValue(Name, out var type))
            return $"Unknown constant {Name}";
        
        var substitution = new Dictionary<string, InfType>();
        if (GenerateTypeSubstitution(kernel, type, Type, substitution).IsError(out var error))
            return error;
        
        var result = new Dictionary<TyVar, Type>();
        foreach (var (name, infType) in substitution)
        {
            if (!infType.BindType(kernel).Deconstruct(out type, out error))
                return error;
            
            result[kernel.MakeType(name)] = type;
        }
        
        return result;
    }

    private Result GenerateTypeSubstitution(Kernel kernel, Type from, InfType to, Dictionary<string, InfType> map)
    {
        switch (from)
        {
            case TyVar tyVar:
                if (map.TryGetValue(tyVar.Name, out var type))
                {
                    if (type != to)
                        return $"Type mismatch for type variable {tyVar.Name}";
                }
                else
                {
                    map[tyVar.Name] = to;
                }
                return Result.Success;
            case TyApp tyApp:
                if (to is not InfTyApp infTyApp || tyApp.Name != infTyApp.Name)
                    return $"Type mismatch for type application {tyApp}";
                
                if (tyApp.Args.Count != infTyApp.Args.Length)
                    return $"Type mismatch for type application {tyApp}";

                for (var i = 0; i < tyApp.Args.Count; i++)
                {
                    if (GenerateTypeSubstitution(kernel, tyApp.Args[i], infTyApp.Args[i], map).IsError(out var error))
                        return error;
                }
                return Result.Success;
            default:
                throw new ArgumentOutOfRangeException(nameof(from));
        }
    }

    internal override void GetUsedVarNames(HashSet<string> names)
    {
        
    }

    public override void GetFrees(HashSet<(string name, InfType type)> frees)
    {
        
    }

    public bool Equals(InfConst? other)
    {
        if (other is null)
            return false;
        
        return other.Name == Name && other.Type == Type;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Type);
    }

    public override string DefaultPrint()
    {
        return Name;
    }

    public static InfConst FromConst(Const constant)
    {
        return new InfConst(constant.Name, InfType.FromType(constant.Type));
    }
}