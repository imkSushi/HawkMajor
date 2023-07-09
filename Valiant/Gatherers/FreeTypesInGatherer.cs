using Gatherers;
using Valiant.Terms;
using Valiant.Types;

namespace Valiant.Gatherers;

public class FreeTypesInGatherer : DataGatherer<HashSet<TyVar>>
{
    public override void Gather(object gatherable, HashSet<TyVar> data)
    {
        switch (gatherable)
        {
            case Abs(var parameterType, var body):
                Gather(parameterType, data);
                Gather(body, data);
                break;
            case App(var app, var arg):
                Gather(app, data);
                Gather(arg, data);
                break;
            case Const {Type: var type}:
                Gather(type, data);
                break;
            case Free {Type: var type}:
                Gather(type, data);
                break;
            case TyApp {Args: var args}:
                foreach (var arg in args)
                {
                    Gather(arg, data);
                }
                break;
            case TyVar tyVar:
                data.Add(tyVar);
                break;
        }
    }
}