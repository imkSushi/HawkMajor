using HawkMajor2.Language.Parsing;
using Results;

namespace HawkMajor2.Engine.Displays;

public abstract record DisplaySymbol(string Name,
    string Symbol,
    string Display,
    bool CanInterruptIdentifier = true,
    bool Verify = true);