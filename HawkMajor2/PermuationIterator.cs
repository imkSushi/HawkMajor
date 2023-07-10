using System.Collections;

namespace HawkMajor2;

public class PermuationIterator : IEnumerator<int[]>
{
    public PermuationIterator(int length, int? maxValue = null)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), length, "Must be non-negative.");
        
        if (maxValue != null & maxValue < length - 1)
            throw new ArgumentOutOfRangeException(nameof(maxValue), maxValue, "Must be greater than length.");
        
        Length = length;
        Current = new int[length];
        MaxValue = maxValue ?? length - 1;
        for (var i = 0; i < Length; i++)
        {
            Current[i] = -1;
        }
    }
    public int Length { get; }
    public int MaxValue { get; }

    public int[] Current { get; }

    public int Cursor { get; private set; } = -1;

    public bool IncrementCursor()
    {
        if (Cursor == Length - 1)
            return false;
        
        Cursor++;
        do
        {
            Current[Cursor]++;
        } while (MustIncreaseCursorValue());
        
        return true;
    }
    
    public bool MoveNext()
    {
        if (Cursor == -1)
            return false;
        
        do
        {
            Current[Cursor]++;
        } while (MustIncreaseCursorValue());
        
        if (Current[Cursor] == MaxValue)
        {
            if (Cursor == 0)
                return false;
            
            Current[Cursor] = -1;
            Cursor--;
            return MoveNext();
        }
        
        return true;
    }

    private bool MustIncreaseCursorValue()
    {
        var indexValue = Current[Cursor];
        for (var i = 0; i < Cursor; i++)
        {
            if (Current[i] == indexValue)
                return true;
        }
        
        return false;
    }

    public void Reset()
    {
        Cursor = -1;
        for (var i = 0; i < Length; i++)
        {
            Current[i] = -1;
        }
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        
    }

    public override string ToString()
    {
        return $"Cursor: {Cursor}, Current: [{string.Join(", ", Current)}]";
    }
}