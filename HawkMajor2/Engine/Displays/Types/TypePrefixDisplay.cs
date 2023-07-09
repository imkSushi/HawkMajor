using HawkMajor2.Language.Parsing;
using Results;

namespace HawkMajor2.Engine.Displays.Types;

public record TypePrefixDisplay(string Name, string Symbol, string Display, int Precedence, bool CanInterruptIdentifier = true, bool Verify = true) : TypeDisplay(Name, Symbol, Display, CanInterruptIdentifier, Verify);