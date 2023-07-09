using System.Collections;

namespace HawkMajor2.NameGenerators;

public class NameGenerator : IEnumerator<string>
{
    public LetterConfig FirstLetterConfig { get; init; } = new();
    public LetterConfig LetterConfig { get; init; } = new();
    public string Prefix { get; init; } = "";
    private WordGenerator _generator = new();
    public HashSet<string> ToAvoid { get; init; } = new();
    public bool AddToAvoid { get; init; } = true;

    public NameGenerator()
    {
        Current = Prefix;
    }
    
    public bool MoveNext()
    {
        do
        {
            _generator.MoveNext();
            Current = Prefix + _generator.Current;
        } while (ToAvoid.Contains(Current));
        
        if (AddToAvoid) 
            ToAvoid.Add(Current);
        
        return true;
    }

    public void Reset()
    {
        _generator.Reset();
    }

    public string Current { get; private set; }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        _generator.Dispose();
    }
    
    public static string GetUniqueWord(HashSet<string> toAvoid, string prefix = "")
    {
        var wordGen = new NameGenerator { ToAvoid = toAvoid, Prefix = prefix };
        wordGen.MoveNext();
        return wordGen.Current;
    }
}