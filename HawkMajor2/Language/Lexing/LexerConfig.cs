namespace HawkMajor2.Language.Lexing;

public class LexerConfig
{
    private List<(string keyword, bool canInterruptIdentifier)> _keywords = new();

    private HashSet<string> _validKeywords = new();
    
    public bool RegisterNewLines { get; init; } = false;
    
    public HashSet<string> GetKeywords() => _validKeywords.ToHashSet();
    
    public IReadOnlyList<(string keyword, bool canInterruptIdentifier)> Keywords => _keywords; 

    public bool? CanInterruptIdentifier(string symbol)
    {
        if (!_validKeywords.Contains(symbol))
            return null;
        
        return _keywords.First(x => x.keyword == symbol).canInterruptIdentifier;
    }
    
    public bool TryAddKeyword(string keyword, bool canInterruptIdentifier = true)
    {
        if (_validKeywords.Contains(keyword))
            return false;
        
        _keywords.Add((keyword, canInterruptIdentifier));
        _validKeywords.Add(keyword);
        
        return true;
    }

    public void AddKeyword(string keyword, bool canInterruptIdentifier = true)
    {
        if (!TryAddKeyword(keyword, canInterruptIdentifier))
            throw new ArgumentException($"Keyword '{keyword}' already exists.");
    }
    
    public void AddKeywords(IEnumerable<string> keywords, bool canInterruptIdentifier = true)
    {
        foreach (var keyword in keywords)
            AddKeyword(keyword, canInterruptIdentifier);
    }
    
    public bool TryRemoveKeyword(string keyword)
    {
        if (!_validKeywords.Remove(keyword))
            return false;

        _keywords.Remove((keyword, true));
        _keywords.Remove((keyword, false));

        return true;
    }
    
    public void RemoveKeyword(string keyword)
    {
        if (!TryRemoveKeyword(keyword))
            throw new ArgumentException($"Keyword '{keyword}' does not exist.");
    }
}