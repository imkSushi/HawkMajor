using System.Collections;

namespace HawkMajor2.NameGenerators;

public class LetterGenerator : IEnumerator<char>
{
    public LetterConfig Config { get; init; } = new();

    public LetterGenerator()
    {
        Current = (char) (FirstChar - 1);
    }
    
    public bool MoveNext()
    {
        switch (Current)
        {
            case 'z' when Config.UpperCase:
            {
                Current = 'A';
                return true;
            }
            case 'z' or 'Z' when Config.Digits:
            {
                Current = '0';
                return true;
            }
            case 'z':
            case 'Z':
            case '9':
                return false;
            default:
                Current++;
                return true;
        }
    }

    public void Reset()
    {
        Current = (char) (FirstChar - 1);
    }

    public char Current { get; private set; }
    public char FirstChar => Config.LowerCase ? 'a' : Config.UpperCase ? 'A' : Config.Digits ? '0' : throw new ArgumentException("No characters are allowed.");

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        
    }
}