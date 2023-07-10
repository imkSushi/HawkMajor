using HawkMajor2.Shadows.ShadowTerms.MatchData;
using HawkMajor2.Shadows.ShadowTypes;
using Printing;
using Results;
using Valiant;
using Valiant.Terms;
using Valiant.Types;

namespace HawkMajor2.Shadows.ShadowTerms;

public abstract record ShadowTerm : IPrintable
{
    public abstract string DefaultPrint();

    public sealed override string ToString()
    {
        return Printer.UniversalPrinter.Print(this);
    }

    public abstract bool ContainsUnfixed();
    public abstract bool Match(Term term, MatchTermData maps);
    public abstract bool Match(ShadowTerm term, MatchShadowTermData maps);

    public abstract bool ContainsUnboundBound(int depth = 0);
    public abstract ShadowTerm FreeBoundVariable(string name, int depth);

    public abstract Result<ShadowTerm> RemoveFixedTerms(Dictionary<ShadowFixed, ShadowTerm> termMap,
        Dictionary<ShadowTyFixed, ShadowType> typeMap);
    public abstract Result<Term> ConvertToTerm(Kernel kernel);


    public abstract Result<Term> ConvertToTerm(Dictionary<ShadowFixed, Term> termMap,
        Dictionary<ShadowTyFixed, Type> typeMap,
        Kernel kernel);

    public static ShadowTerm ToShadowTerm(Term term,
        Dictionary<string, ShadowVar> fixedTerms,
        Dictionary<string, ShadowTyMeta> fixedTypes)
    {
        return term switch
        {
            Abs abs        => ShadowAbs.FromAbs(abs, fixedTerms, fixedTypes),
            App app        => ShadowApp.FromApp(app, fixedTerms, fixedTypes),
            Const constant => ShadowConst.FromConst(constant, fixedTerms, fixedTypes),
            Free free      => ShadowVar.FromFree(free, fixedTerms, fixedTypes),
            _              => throw new ArgumentException()
        };
    }
    
    internal abstract ShadowTerm BindVariable(ShadowFree free, int depth);

    public abstract ShadowTerm FixTerms(HashSet<string> terms, HashSet<string> types);
    public abstract ShadowTerm FixMeta();
}