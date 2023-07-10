using System.Text;
using HawkMajor2.Language.Parsing;
using Valiant;

Console.OutputEncoding = Encoding.UTF8;

var kernel = new Kernel();

var parser = new ScriptParser(kernel);

var script = """
display term infix "=" "=" "=" right 100
display type infix -> fun ⟶ left 100

global strat Assume "p |- p"
{
    kernel asm p
}

global strat Reflectivity "|- p = p"
{
    kernel refl p
}

global strat Congruence "|- f x: a = g y: a"
{
    kernel cong "|- f = g" "|- x = y"
}

global strat EqModusPonens "|- p"
{
    match "|- q = p"
    kernel mp "|- q = p" "|- q"
}

global proof Commutativity "p = q |- q = p"
{
    "p = q |- (p = p) = (q = p)"
    "p = q |- q = p"
}

global strat Elimination "|- p"
{
    match "q |- p"
    match "|- q"
    kernel anti "|- q" "q |- p"
    kernel mp "|- q = p" "|- q"
}

global strat Commutativity "|- p = q"
{
    prove "|- q = p"
    prove "q = p |- p = q"
    prove "|- p = q"
}

global proof Transitivity "p = q, q = r |- p = r"
{
    "q = r |- (p = q) = (p = r)"
    "p = q, q = r |- p = r"
}

global strat Transitivity "|- p = r"
{
    match "|- p = q"
    match "|- q = r"
    prove "p = q, q = r |- p = r"
    prove "|- p = r"
}


""";

/*const T = "((\ x : bool . x) = (\ x . x))"
display term const T T ⊤

global proof Truth "|- T"
{
    "|- ((\ x : bool . x) = (\ x . x)) = T"
    "|- T"
}*/

var result = parser.Parse(script);

if (result.IsError(out var error))
{
    Console.WriteLine(error);
}
else
{
    foreach (var theorem in parser.Workspace.GlobalTheorems)
    {
        Console.WriteLine(theorem);
    }
}