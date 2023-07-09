namespace HawkMajor2.Engine.Displays.Types;

public record TypeConstantDisplay(string Name, string Symbol, string Display, bool CanInterruptIdentifier = true, bool Verify = true) : TypeDisplay(Name, Symbol, Display, CanInterruptIdentifier, Verify);