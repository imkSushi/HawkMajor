namespace HawkMajor2.NameGenerators;

public readonly struct LetterConfig
{
    public readonly bool LowerCase;
    public readonly bool UpperCase;
    public readonly bool Digits;
    public LetterConfig(bool lowerCase, bool upperCase, bool digits)
    {
        LowerCase = lowerCase;
        UpperCase = upperCase;
        Digits = digits;
    }

    public LetterConfig()
    {
        LowerCase = true;
        UpperCase = true;
        Digits = true;
    }
    
    public bool Empty => !LowerCase && !UpperCase && !Digits;
}