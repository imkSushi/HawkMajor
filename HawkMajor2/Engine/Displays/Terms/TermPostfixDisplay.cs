namespace HawkMajor2.Engine.Displays.Terms;

public record TermPostfixDisplay(string Name, string Symbol, string Display, int Precedence, bool CanInterruptIdentifier = true, bool Verify = true) : TermDisplay(Name, Symbol, Display, CanInterruptIdentifier, Verify);