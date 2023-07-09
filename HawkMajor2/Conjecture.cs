using System.Collections.ObjectModel;
using Printing;
using Valiant;
using Valiant.Terms;
using Valiant.Types;

namespace HawkMajor2;

public class Conjecture : IPrintable
{
    internal Conjecture(ReadOnlyCollection<Term> premises, Term conclusion)
    {
        Premises = premises;
        Conclusion = conclusion;
    }

    internal Conjecture(Term conclusion)
    {
        Premises = Array.Empty<Term>().AsReadOnly();
        Conclusion = conclusion;
    }

    internal Conjecture(Term premise, Term conclusion)
    {
        Premises = new[] {premise}.AsReadOnly();
        Conclusion = conclusion;
    }
    
    internal Conjecture(HashSet<Term> premises, Term conclusion)
    {
        Premises = premises.ToList().AsReadOnly();
        Conclusion = conclusion;
    }
    
    internal Conjecture(IEnumerable<Term> premises, Term conclusion)
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

    public bool Equals(Theorem other)
    {
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

    public Theorem? CheckIfInstance(Theorem theorem, Kernel kernel)
    {
        var (premises, conclusion) = theorem;
        
        if (!TermMapGenerator.GenerateMap(Conclusion, conclusion, out var termMap, out var typeMap))
            return null;
        
        var mapStack = new Stack<(Dictionary<Free, Term> termMap, Dictionary<TyVar, Type> typeMap)>(new []{(termMap, typeMap)});
        var permutations = new PermuationIterator(theorem.Premises.Count, Premises.Count);
            
        if (!permutations.IncrementCursor())
        {
            var typed = (Theorem) kernel.Instantiate(theorem, typeMap);
            return (Theorem) kernel.Instantiate(typed, termMap);
        }

        do
        {
            while (mapStack.Count > permutations.Cursor + 1)
            {
                mapStack.Pop();
            }
            
            var newTopTypes = new Dictionary<TyVar, Type>(mapStack.Peek().typeMap);
            var newTopTerms = new Dictionary<Free, Term>(mapStack.Peek().termMap);
            
            while (TermMapGenerator.GenerateMap(Premises[permutations.Current[permutations.Cursor]], premises[permutations.Cursor], newTopTerms, newTopTypes, new Stack<(Free term, Free template)>()))
            {
                if (!permutations.IncrementCursor())
                {
                    var typed = (Theorem) kernel.Instantiate(theorem, newTopTypes);
                    return (Theorem) kernel.Instantiate(typed, newTopTerms);
                }

                mapStack.Push((newTopTerms, newTopTypes));
                newTopTypes = new Dictionary<TyVar, Type>(newTopTypes);
                newTopTerms = new Dictionary<Free, Term>(newTopTerms);
            }
            
        } while (permutations.MoveNext());
        
        return null;
    }

    public HashSet<Free> FreesIn()
    {
        var output = new HashSet<Free>();
        
        Conclusion.FreesIn(output);
        foreach (var premise in Premises)
        {
            premise.FreesIn(output);
        }
        
        return output;
    }
    
    public HashSet<TyVar> FreeTypesIn()
    {
        var output = new HashSet<TyVar>();
        
        Conclusion.FreeTypesIn(output);
        foreach (var premise in Premises)
        {
            premise.FreeTypesIn(output);
        }
        
        return output;
    }
}