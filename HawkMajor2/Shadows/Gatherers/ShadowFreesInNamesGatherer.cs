using Gatherers;
using HawkMajor2.Shadows.ShadowTerms;
using Valiant.Gatherers;
using Valiant.Terms;

namespace HawkMajor2.Shadows.Gatherers;

public class ShadowFreesInNamesGatherer : DataGatherer<HashSet<string>>
{
    private FreesInGatherer _freesInGatherer = new();

    public override void Gather(object gatherable, HashSet<string> data)
    {
        if (gatherable is Term)
        {
            foreach (var (name, _) in _freesInGatherer.Gather(gatherable))
            {
                data.Add(name);
            }

            return;
        }

        switch (gatherable)
        {
            case ShadowAbs {Body: var body}:
                Gather(body, data);
                break;
            case ShadowApp {Application: var app, Argument: var arg}:
                Gather(app, data);
                Gather(arg, data);
                break;
            case ShadowFree {Name: var name}:
                data.Add(name);
                break;
        }
    }
}