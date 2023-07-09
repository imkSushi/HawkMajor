using System.Collections.ObjectModel;
using HawkMajor2.Shadows.ShadowTerms;
using HawkMajor2.Shadows.ShadowTerms.MatchData;
using HawkMajor2.Shadows.ShadowTypes;
using Printing;
using Results;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Shadows;

public sealed record ShadowTheorem : IPrintable
{
    internal ShadowTheorem(ReadOnlyCollection<ShadowTerm> premises, ShadowTerm conclusion)
    {
        Premises = premises;
        Conclusion = conclusion;
    }

    internal ShadowTheorem(ShadowTerm conclusion)
    {
        Premises = Array.Empty<ShadowTerm>().AsReadOnly();
        Conclusion = conclusion;
    }

    internal ShadowTheorem(ShadowTerm premise, ShadowTerm conclusion)
    {
        Premises = new[] {premise}.AsReadOnly();
        Conclusion = conclusion;
    }
    
    internal ShadowTheorem(HashSet<ShadowTerm> premises, ShadowTerm conclusion)
    {
        Premises = premises.ToList().AsReadOnly();
        Conclusion = conclusion;
    }
    
    internal ShadowTheorem(IEnumerable<ShadowTerm> premises, ShadowTerm conclusion)
    {
        Premises = premises.Distinct().ToList().AsReadOnly();
        Conclusion = conclusion;
    }

    public ReadOnlyCollection<ShadowTerm> Premises { get; }
    public ShadowTerm Conclusion { get; }
    public void Deconstruct(out ReadOnlyCollection<ShadowTerm> premises, out ShadowTerm conclusion)
    {
        premises = Premises;
        conclusion = Conclusion;
    }

    public bool Equals(ShadowTheorem? other)
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

    public IEnumerable<MatchTermData> Match(Theorem theorem, MatchTermData maps, bool allowExtras, bool allowMissing)
    {
        return Match(new Conjecture(theorem.Premises, theorem.Conclusion), maps, allowExtras, allowMissing);
    }

    public IEnumerable<MatchTermData> Match(Conjecture theorem, MatchTermData maps, bool allowExtras, bool allowMissing)
    {
        if (allowExtras && allowMissing)
            throw new ArgumentException("Cannot allow extras and missing at the same time");
        
        if (!allowExtras && theorem.Premises.Count > Premises.Count)
            yield break;
        
        if (!allowMissing && theorem.Premises.Count < Premises.Count)
            yield break;
        
        var (premises, conclusion) = theorem;
        if (!Conclusion.Match(conclusion, maps))
            yield break;
        
        var mapStack = new Stack<MatchTermData>(new []{maps});
        var permutations = new PermuationIterator(allowExtras ? Premises.Count : theorem.Premises.Count, allowMissing ? Premises.Count : theorem.Premises.Count);
            
        if (!permutations.IncrementCursor())
        {
            yield return maps.Clone();
            yield break;
        }

        do
        {
            while (mapStack.Count > permutations.Cursor + 1)
            {
                mapStack.Pop();
            }
            
            if (!mapStack.Any())
                yield break;
            
            var newTop = mapStack.Peek().Clone();

            while (Premises[allowExtras ? permutations.Cursor : permutations.Current[permutations.Cursor]].Match(premises[allowExtras ? permutations.Current[permutations.Cursor] : permutations.Cursor], newTop))
            {
                if (!permutations.IncrementCursor())
                {
                    yield return newTop.Clone();
                    break;
                }
                mapStack.Push(newTop);
                newTop = newTop.Clone();
            }
            
        } while (permutations.MoveNext());
    }
    
    public IEnumerable<MatchShadowTermData> Match(ShadowTheorem theorem, MatchShadowTermData maps, bool allowExtras, bool allowMissing)
    {
        if (allowExtras && allowMissing)
            throw new ArgumentException("Cannot allow extras and missing at the same time");
        
        if (!allowExtras && theorem.Premises.Count > Premises.Count)
            yield break;
        
        if (!allowMissing && theorem.Premises.Count < Premises.Count)
            yield break;
        
        var (premises, conclusion) = theorem;
        if (!Conclusion.Match(conclusion, maps))
            yield break;
        
        var mapStack = new Stack<MatchShadowTermData>(new []{maps});
        var permutations = new PermuationIterator(allowExtras ? Premises.Count : theorem.Premises.Count, allowMissing ? Premises.Count : theorem.Premises.Count);
            
        if (!permutations.IncrementCursor())
        {
            yield return maps.Clone();
            yield break;
        }

        do
        {
            while (mapStack.Count >= permutations.Cursor + 1)
            {
                mapStack.Pop();
            }
            
            var newTop = mapStack.Peek().Clone();

            while (Premises[allowExtras ? permutations.Cursor : permutations.Current[permutations.Cursor]].Match(premises[allowExtras ? permutations.Current[permutations.Cursor] : permutations.Cursor], newTop))
            {
                if (!permutations.IncrementCursor())
                {
                    yield return newTop.Clone();
                    break;
                }
                mapStack.Push(newTop);
                newTop = newTop.Clone();
            }
            
        } while (permutations.MoveNext());
    }

    public Result<ShadowTheorem> RemoveFixedTerms(Dictionary<ShadowFixed, ShadowTerm> termMap, Dictionary<ShadowTyFixed, ShadowType> typeMap)
    {
        if (!Conclusion.RemoveFixedTerms(termMap, typeMap).Deconstruct(out var newConclusion, out var error))
            return error;
        
        var newPremises = new List<ShadowTerm>();
        
        foreach (var premise in Premises)
        {
            if (!premise.RemoveFixedTerms(termMap, typeMap).Deconstruct(out var newPremise, out error))
                return error;
            
            newPremises.Add(newPremise);
        }
        
        return new ShadowTheorem(newPremises, newConclusion);
    }
    
    public Result<Conjecture> ConvertToConjecture(Dictionary<ShadowFixed, Term> termMap, Dictionary<ShadowTyFixed, Type> typeMap, IEnumerable<Term> premises, Kernel kernel)
    {
        if (!Conclusion.ConvertToTerm(termMap, typeMap, kernel).Deconstruct(out var newConclusion, out var error))
            return error;
        
        var newPremises = premises.ToList();
        
        foreach (var premise in Premises)
        {
            if (!premise.ConvertToTerm(termMap, typeMap, kernel).Deconstruct(out var newPremise, out error))
                return error;
            
            newPremises.Add(newPremise);
        }
        
        return new Conjecture(newPremises, newConclusion);
    }
    
    public Result<Conjecture> ConvertToConjecture(Kernel kernel)
    {
        if (!Conclusion.ConvertToTerm(kernel).Deconstruct(out var newConclusion, out var error))
            return error;
        
        var newPremises = new List<Term>();
        
        foreach (var premise in Premises)
        {
            if (!premise.ConvertToTerm(kernel).Deconstruct(out var newPremise, out error))
                return error;
            
            newPremises.Add(newPremise);
        }
        
        return new Conjecture(newPremises, newConclusion);
    }
    
    public static ShadowTheorem FromConjecture(Conjecture conjecture, Dictionary<string, ShadowVarType> fixedTerms, Dictionary<string, ShadowVarType> fixedTypes)
    {
        var premises = new List<ShadowTerm>();
        
        foreach (var premise in conjecture.Premises)
        {
            premises.Add(ShadowTerm.ToShadowTerm(premise, fixedTerms, fixedTypes));
        }
        
        return new ShadowTheorem(premises, ShadowTerm.ToShadowTerm(conjecture.Conclusion, fixedTerms, fixedTypes));
    }
}