namespace HawkMajor2.Engine.Displays.Terms;

public record TermInfixDisplay(string Name, string Symbol, string Display, bool LeftAssociative, int Precedence, bool CanInterruptIdentifier = true, bool Verify = true) : TermDisplay(Name, Symbol, Display, CanInterruptIdentifier, Verify);