using HawkMajor2.Shadows.ShadowTerms;
using HawkMajor2.Shadows.ShadowTypes.MatchData;
using Printing;
using Results;
using Valiant;
using Valiant.Types;

namespace HawkMajor2.Shadows.ShadowTypes;

public abstract record ShadowType : IPrintable
{
    public abstract string DefaultPrint();

    public sealed override string ToString()
    {
        return Printer.UniversalPrinter.Print(this);
    }

    public abstract bool ContainsUnfixed();
    public abstract bool Match(Type type, MatchTypeData maps);
    public abstract bool Match(ShadowType type, MatchShadowTypeData maps);

    public abstract Result<ShadowType> RemoveFixedTerms(Dictionary<ShadowTyFixed, ShadowType> typeMap);
    public abstract Result<Type> ConvertToType(Kernel kernel);

    public abstract Result<Type> ConvertToType(Dictionary<ShadowTyFixed, Type> typeMap, Kernel kernel);

    public static ShadowType ToShadowType(Type type, Dictionary<string, ShadowTyMeta> fixedTypes)
    {
        return type switch
        {
            TyApp tyapp => ShadowTyApp.FromTyApp(tyapp, fixedTypes),
            TyVar tyvar => ShadowTyMeta.FromTyVar(tyvar, fixedTypes),
            _           => throw new ArgumentException()
        };
    }

    public abstract ShadowType FixTypes(HashSet<string> toFix);
    public abstract ShadowType FixMeta();
}