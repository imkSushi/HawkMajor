using HawkMajor2.Language.Inference;
using HawkMajor2.Shadows;
using Printing;
using Valiant;

namespace HawkMajor2.Printers;

public class TheoremPrinter : TermPrinter
{
    public override string Print(IPrintable printable)
    {
        if (printable is Conjecture conjecture)
            return PrintConjecture(conjecture);
        
        if (printable is Theorem theorem)
            return PrintTheorem(theorem);
        
        if (printable is InfTheorem infTheorem)
            return PrintInfTheorem(infTheorem);
        
        if (printable is ShadowTheorem shadow)
            return PrintShadowTheorem(shadow);
        
        return base.Print(printable);
    }
    
    private string PrintConjecture(Conjecture conjecture)
    {
        return conjecture.Premises.Any() ? $"{string.Join(", ", conjecture.Premises.Select(x => x.ToString()))} ⊢ {conjecture.Conclusion}" : $"⊢ {conjecture.Conclusion}";
    }
    
    private string PrintTheorem(Theorem theorem)
    {
        return theorem.Premises.Any() ? $"{string.Join(", ", theorem.Premises.Select(x => x.ToString()))} ⊢ {theorem.Conclusion}" : $"⊢ {theorem.Conclusion}";
    }
    
    private string PrintInfTheorem(InfTheorem infTheorem)
    {
        return infTheorem.Premises.Any() ? $"{string.Join(", ", infTheorem.Premises.Select(x => x.ToString()))} ⊢ {infTheorem.Conclusion}" : $"⊢ {infTheorem.Conclusion}";
    }
    
    private string PrintShadowTheorem(ShadowTheorem shadow)
    {
        return shadow.Premises.Any() ? $"{string.Join(", ", shadow.Premises.Select(x => x.ToString()))} ⊢ {shadow.Conclusion}" : $"⊢ {shadow.Conclusion}";
    }
}