namespace HawkMajor2.Engine.Displays.Types;

public record TypePostfixDisplay(string Name, string Symbol, string Display, int Precedence, bool CanInterruptIdentifier = true, bool Verify = true) : TypeDisplay(Name, Symbol, Display, CanInterruptIdentifier, Verify);