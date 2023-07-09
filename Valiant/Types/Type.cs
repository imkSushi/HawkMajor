using Printing;
using Results;

namespace Valiant.Types;

public abstract record Type : IPrintable
{
    public abstract Type Instantiate(Dictionary<TyVar, Type> map);
    public abstract void FreeTypesIn(HashSet<TyVar> frees);
    public abstract void FreeTypeNamesIn(HashSet<string> frees);
    
    public abstract Result GenerateSubstitutionMapping(Type desired, Dictionary<TyVar, Type> typeMap);
    public abstract string DefaultPrint();

    public sealed override string ToString()
    {
        return Printer.UniversalPrinter.Print(this);
    }
}