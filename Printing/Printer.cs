namespace Printing;

public class Printer
{
    public static Printer UniversalPrinter { get; private set; } = new();
    
    public virtual string Print(IPrintable printable)
    {
        return printable.DefaultPrint();
    }
    
    public void Activate()
    {
        UniversalPrinter = this;
    }
    
    public void Deactivate()
    {
        UniversalPrinter = new Printer();
    }
}