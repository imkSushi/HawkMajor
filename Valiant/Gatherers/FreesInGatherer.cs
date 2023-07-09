using Gatherers;
using Valiant.Terms;

namespace Valiant.Gatherers;

public class FreesInGatherer : DataGatherer<HashSet<Free>>
{
    public override void Gather(object gatherable, HashSet<Free> data)
    {
        switch (gatherable)
        {
            case Abs abs:
                Gather(abs.Body, data);
                break;
            case App(var app, var arg):
                Gather(app, data);
                Gather(arg, data);
                break;
            case Free free:
                data.Add(free);
                break;
        }
    }
}