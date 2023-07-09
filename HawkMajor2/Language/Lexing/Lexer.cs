using System.Collections;
using HawkMajor2.Language.Lexing.Tokens;

namespace HawkMajor2.Language.Lexing;

public sealed class Lexer : IEnumerator<Token>
{
    private string _expression;

    private int _index;
    private int _column;
    private int _line;

    private Token? _cachedNextToken;

    private LexerConfig _context = new();
    
    public IReadOnlyList<(string keyword, bool canInterruptIdentifier)> Keywords => _context.Keywords;

    public LexerConfig Context
    {
        get => _context;
        set
        {
            if (_context == value)
                return;
            
            _context = value;
            _cachedNextToken = null;

            SetTo(Current);
        }
    }

    public Lexer(string expression)
    {
        _expression = expression;
        _index = 0;
        _column = 1;
        _line = 1;
        _cachedNextToken = null;
    }
    
    public TokenData GetCurrentTokenData()
    {
        if (_cachedNextToken == null)
            return new TokenData(_index, _line, _column);
        
        var offset = _cachedNextToken.Value.Length;
        return new TokenData(_index - offset, _line, _column - offset);
    }

    public void AdjustLineNumber(int newValue)
    {
        _line = newValue;
    }
    
    public void AdjustColumnNumber(int newValue)
    {
        _column = newValue;
    }

    object IEnumerator.Current => Current;
    
    public void SetTo(TokenData? data)
    {
        _index = data?.Index ?? 0;
        _column = data?.Column ?? 1;
        _line = data?.Line ?? 1;
        _cachedNextToken = null;
        Current = null!;
        MoveNext();
    }

    public void SetTo(Token? token)
    {
        SetTo(token?.Data);
    }

    private bool NotAtEnd()
    {
        return _index < _expression.Length;
    }

    private bool AtEnd()
    {
        return _index >= _expression.Length;
    }

    public bool MoveNext()
    {
        if (Current is ErrorToken or EndOfExpressionToken)
            return false;
        
        Current = NextToken();
        return true;
    }

    void IEnumerator.Reset()
    {
        _index = 0;
        _cachedNextToken = null;
        _column = 1;
        _line = 1;
        Current = null!;
    }

    public void Reset(string expression)
    {
        _expression = expression;
        _index = 0;
        _cachedNextToken = null;
        _column = 1;
        _line = 1;
        Current = null!;
    }

    public Token Current { get; private set; } = null!;

    private Token NextToken()
    {
        if (_cachedNextToken != null)
        {
            var output = _cachedNextToken;
            _cachedNextToken = null;
            return output;
        }
        
        if (SkipWhitespace() && Context.RegisterNewLines)
            return new NewLineToken(new TokenData(_index, _line, _column));

        if (AtEnd())
            return new EndOfExpressionToken(new TokenData(_index, _line, _column));

        var start = _index;
        var atStart = true;

        var str = "";
        var validKeywordParts = new HashSet<(int keyword, int indexStart)>();
        (int keyword, int indexStart)? latestValidKeyword = null;

        while (NotAtEnd())
        {
            var c = CurrentChar;
            if (c is ' ' or '\t' or '\r' or '\n')
                break;
                
            if (IsInvalidChar(c))
                return new ErrorToken($"Invalid character '{c}'", new TokenData(_index, _line, _column));

            var index1 = _index;
            validKeywordParts.RemoveWhere(tuple => c != Keywords[tuple.keyword].keyword[index1 - tuple.indexStart]);

            if (latestValidKeyword == null)
            {
                for (var i = 0; i < Keywords.Count; i++)
                {
                    var (keyword, canInterruptIdentifier) = Keywords[i];
                    if (!atStart && !canInterruptIdentifier)
                        continue;
                    
                    if (keyword[0] == c) 
                        validKeywordParts.Add((i, _index));
                }
            }
            else if (validKeywordParts.Count == 0)
                break;
                
            var validKeywords = ValidFullKeywords(validKeywordParts);

            if (validKeywords.Any())
            {
                var (newKeyword, newIndexStart) = validKeywords.MinBy(tuple => tuple.indexStart);
                validKeywordParts.RemoveWhere(tuple => tuple.indexStart > newIndexStart);
                validKeywordParts.Remove((newKeyword, newIndexStart));
                latestValidKeyword = (newKeyword, newIndexStart);
                        
                if (validKeywordParts.Count == 0)
                    break;
            }
                
            if (latestValidKeyword == null)
                str += c;
                
            Advance();
            atStart = false;
        }

        if (latestValidKeyword == null) 
            return new IdentifierToken(str, new TokenData(start, _line, _column - _index + start));
            
        var (outputKeywordListIndex, outputStartIndex) = latestValidKeyword.Value;
        var outputKeyword = Keywords[outputKeywordListIndex].keyword;

        _column += outputStartIndex + outputKeyword.Length - _index;
        _index = outputStartIndex + outputKeyword.Length;
        
        if (outputStartIndex == start)
            return new KeywordToken(outputKeyword, new TokenData(start, _line, _column - _index + start));

        _cachedNextToken = new KeywordToken(outputKeyword, new TokenData(outputStartIndex, _line, _column - _index + outputStartIndex));

        return new IdentifierToken(str[..(outputStartIndex - start)], new TokenData(start, _line, _column - _index + start));
    }

    private List<(int keyword, int indexStart)> ValidFullKeywords(HashSet<(int keyword, int indexStart)> validKeywordParts)
    {
        return validKeywordParts.Where(tuple => Keywords[tuple.keyword].keyword.Length == _index - tuple.indexStart + 1).ToList();
    }

    private char CurrentChar => _expression[_index];

    private bool IsInvalidChar(char c)
    {
        /*return c is < '!' or > '~';*/
        return false;
    }

    private bool IsValidChar(char c)
    {
        /*return c is >= '!' and <= '~';*/
        return true;
    }

    private bool SkipWhitespace()
    {
        while (NotAtEnd() && CurrentChar is ' ' or '\t' or '\n' or '\r')
        {
            if (CurrentChar is '\n')
            {
                Advance();
                return true;
            }
            Advance();
        }
        
        return false;
    }

    public void Dispose()
    {
        
    }

    private void Advance()
    {
        if (_expression[_index++] == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }
    }

    public string GenerateError(string message)
    {
        if (Current is ErrorToken err)
            return err.ToString();
        
        return $"{GetCurrentTokenData()} Error: {message}";
    }
}