namespace HawkMajor2.Engine.Displays.Terms;

public record TermConstantDisplay(string Name, string Symbol, string Display, bool CanInterruptIdentifier = true, bool Verify = true) : TermDisplay(Name, Symbol, Display, CanInterruptIdentifier, Verify);