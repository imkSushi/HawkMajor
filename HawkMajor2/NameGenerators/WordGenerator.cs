using System.Collections;

namespace HawkMajor2.NameGenerators;

public class WordGenerator : IEnumerator<string>
{
    public LetterConfig FirstLetterConfig { get; init; } = new();
    public LetterConfig LetterConfig { get; init; } = new();
    private List<LetterGenerator> _generators = new();

    public WordGenerator()
    {
        if (FirstLetterConfig.Empty)
        {
            throw new ArgumentException("FirstLetterConfig must not be empty.");
        }
        
        if (LetterConfig.Empty)
        {
            throw new ArgumentException("LetterConfig must not be empty.");
        }
    }
    
    public bool MoveNext()
    {
        if (_generators.Count == 0)
        {
            var firstGenerator = new LetterGenerator {Config = FirstLetterConfig};
            firstGenerator.MoveNext();
            _generators.Add(firstGenerator);
            Current = firstGenerator.Current.ToString();
            return true;
        }
        
        for (var i = _generators.Count - 1; i >= 0; i--)
        {
            var generator = _generators[i];
            if (generator.MoveNext())
            {
                Current = new string(_generators.Select(g => g.Current).ToArray());
                return true;
            }
            
            generator.Reset();
            generator.MoveNext();
        }
        
        var newGenerator = new LetterGenerator {Config = LetterConfig};
        newGenerator.MoveNext();
        _generators.Add(newGenerator);
        Current = new string(_generators.Select(g => g.Current).ToArray());
        return true;
    }

    public void Reset()
    {
        _generators.Clear();
    }

    public string Current { get; private set; } = "";

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        
    }
}