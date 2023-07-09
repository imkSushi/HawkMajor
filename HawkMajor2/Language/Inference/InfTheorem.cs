using HawkMajor2.Language.Inference.Terms;
using Printing;

namespace HawkMajor2.Language.Inference;

public record InfTheorem(InfTerm Conclusion, InfTerm[] Premises) : IPrintable
{
    public void Deconstruct(out InfTerm conclusion, out InfTerm[] premises)
    {
        conclusion = Conclusion;
        premises = Premises;
    }

    public override string ToString()
    {
        return Printer.UniversalPrinter.Print(this);
    }

    public string DefaultPrint()
    {
        return Premises.Any() ? $"{string.Join(", ", Premises.Select(x => x.ToString()))} |- {Conclusion}" : $"|- {Conclusion}";
    }

    public virtual bool Equals(InfTheorem? other)
    {
        if (other is null)
            return false;

        if (other.Conclusion != Conclusion)
            return false;

        return Premises.ToHashSet().SetEquals(other.Premises);
    }

    public override int GetHashCode()
    {
        var premisesHashCode = Premises.Distinct().Aggregate(0, (current, premise) => current ^ premise.GetHashCode());
        return HashCode.Combine(premisesHashCode, Conclusion);
    }
}