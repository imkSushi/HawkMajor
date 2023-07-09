using HawkMajor2.NameGenerators;
using Valiant.Terms;

namespace HawkMajor2.Extensions;

public static class AbsExtension
{
    public static (Term body, Free boundVariable) GetBody(this Abs abs)
    {
        var usedFrees = new HashSet<Free>();
        abs.FreesIn(usedFrees);

        var uniqueName = NameGenerator.GetUniqueWord(usedFrees.Select(free => free.Name).ToHashSet());

        var body = abs.GetBody(uniqueName, out var boundVariable);
        
        return (body, boundVariable);
    }
}