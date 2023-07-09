namespace Generators;


public struct DisplayValues
{
    public bool HasAssociativity;
    public bool HasPrecedence;
    
    public DisplayValues(bool hasAssociativity, bool hasPrecedence)
    {
        HasAssociativity = hasAssociativity;
        HasPrecedence = hasPrecedence;
    }
}