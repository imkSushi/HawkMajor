namespace HawkMajor2.Engine.Displays.Types;

public record TypeInfixDisplay(string Name, string Symbol, string Display, bool LeftAssociative, int Precedence, bool CanInterruptIdentifier = true, bool Verify = true) : TypeDisplay(Name, Symbol, Display, CanInterruptIdentifier, Verify);