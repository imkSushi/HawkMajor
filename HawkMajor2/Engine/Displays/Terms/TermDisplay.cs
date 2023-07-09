namespace HawkMajor2.Engine.Displays.Terms;

public record TermDisplay(string Name, string Symbol, string Display, bool CanInterruptIdentifier = true, bool Verify = true) : DisplaySymbol(Name, Symbol, Display, CanInterruptIdentifier, Verify);