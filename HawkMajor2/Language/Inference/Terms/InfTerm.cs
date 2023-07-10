using HawkMajor2.Language.Inference.Types;
using HawkMajor2.NameGenerators;
using Printing;
using Results;
using Valiant;
using Valiant.Gatherers;
using Valiant.Terms;

namespace HawkMajor2.Language.Inference.Terms;

public abstract record InfTerm : IPrintable
{
    internal abstract Result<InfTerm> FixCombTypes();
    internal abstract InfType TypeOf();
    internal abstract InfTerm ConvertUnboundTypeToFunction(string name);
    internal abstract InfTerm UniquifyUnboundTypeNames(NameGenerator generator);
    internal abstract Result GenerateMappings(Kernel kernel,
        HashSet<(InfType, InfType)> mappings,
        NameGenerator nameGenerator);

    internal abstract HashSet<InfType> GetTypesOfBoundVariable(int depth);
    internal abstract InfTerm FixAbsVariables(Stack<string?> nameStack);
    internal abstract InfTerm SubstituteType(string name, InfType type);
    internal abstract void GetUsedTyVarNames(HashSet<string> names);

    internal static void GetUsedTyVarNames(Term term, HashSet<string> names)
    {
        var output = FreeTypesInGatherer.GatherData<FreeTypesInGatherer>(term);
        
        names.UnionWith(output.Select(x => x.Name));
    }

    internal abstract InfTerm NicelyRename(NameGenerator nameGenerator, Dictionary<string, string> map);
    internal abstract Result<Term> BindTerm(Kernel kernel, Stack<string> temporaryBoundVariableNames, NameGenerator nameGenerator);

    internal Result<Term> BindTerm(Kernel kernel)
    {
        var usedVarNames = new HashSet<string>();
        GetUsedVarNames(usedVarNames);
        var nameGenerator = new NameGenerator
        {
            ToAvoid = usedVarNames
        };
        return BindTerm(kernel, new Stack<string>(), nameGenerator);
    }
    internal abstract void GetUsedVarNames(HashSet<string> names);
    

    public sealed override string ToString()
    {
        return Printer.UniversalPrinter.Print(this);
    }

    public abstract void GetFrees(HashSet<(string name, InfType type)> frees);
    
    public abstract string DefaultPrint();

    public static InfTerm FromTerm(Term term)
    {
        return term switch
        {
            Abs abs => InfAbs.FromAbs(abs),
            App app => InfApp.FromApp(app),
            Free free => InfFree.FromFree(free),
            Const constant => InfConst.FromConst(constant),
            Bound bound => InfBound.FromBound(bound),
            _ => throw new Exception("Unknown term type")
        };
    }
}