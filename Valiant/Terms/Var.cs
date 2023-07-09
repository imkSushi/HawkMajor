namespace Valiant.Terms;

public abstract record Var : Term
{
    // ReSharper disable once ConvertToPrimaryConstructor
    protected Var(Type type)
    {
        Type = type;
    }
    public Type Type { get; }
    public void Deconstruct(out Type type)
    {
        type = Type;
    }
}