namespace HawkMajor2.Engine.Displays.Types;

public abstract record TypeDisplay(string Name, string Symbol, string Display, bool CanInterruptIdentifier = true, bool Verify = true) : DisplaySymbol(Name, Symbol, Display, CanInterruptIdentifier, Verify);