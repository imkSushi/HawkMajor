using System.Collections.ObjectModel;
using Printing;
using Valiant.Terms;

namespace Valiant;

public sealed record Theorem : IPrintable
{
    internal Theorem(ReadOnlyCollection<Term> premises, Term conclusion)
    {
        Premises = premises;
        Conclusion = conclusion;
    }

    internal Theorem(Term conclusion)
    {
        Premises = Array.Empty<Term>().AsReadOnly();
        Conclusion = conclusion;
    }

    internal Theorem(Term premise, Term conclusion)
    {
        Premises = new[] {premise}.AsReadOnly();
        Conclusion = conclusion;
    }
    
    internal Theorem(HashSet<Term> premises, Term conclusion)
    {
        Premises = premises.ToList().AsReadOnly();
        Conclusion = conclusion;
    }
    
    internal Theorem(IEnumerable<Term> premises, Term conclusion)
    {
        Premises = premises.Distinct().ToList().AsReadOnly();
        Conclusion = conclusion;
    }

    public ReadOnlyCollection<Term> Premises { get; }
    public Term Conclusion { get; }
    public void Deconstruct(out ReadOnlyCollection<Term> premises, out Term conclusion)
    {
        premises = Premises;
        conclusion = Conclusion;
    }

    public bool Equals(Theorem? other)
    {
        if (other is null)
            return false;
        
        var (premises, conclusion) = other;
        
        if (Conclusion != conclusion)
            return false;

        var otherHashSet = premises.ToHashSet();
        var thisHashSet = Premises.ToHashSet();
        
        return thisHashSet.SetEquals(otherHashSet);
    }

    public override int GetHashCode()
    {
        var premisesHashCode = Premises.Distinct().Aggregate(0, (current, premise) => current ^ premise.GetHashCode());
        return HashCode.Combine(premisesHashCode, Conclusion);
    }

    public override string ToString()
    {
        return Printer.UniversalPrinter.Print(this);
    }

    public string DefaultPrint()
    {
        return $"{string.Join(", ", Premises)} |- {Conclusion}";
    }
}