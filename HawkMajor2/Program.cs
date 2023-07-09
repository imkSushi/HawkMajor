using System.Text;
using HawkMajor2.Language.Parsing;
using Valiant;

Console.OutputEncoding = Encoding.UTF8;

var kernel = new Kernel();

var parser = new ScriptParser(kernel);

var script = """
display term infix "=" "=" "=" right 100
display type infix -> fun ⟶ left 100

global strat Congruence "|- f x: a = g y: a"
{
    kernel cong "|- (f :fun a b) = g" "|- x = y"
}

global strat Assume "p |- p"
{
    kernel asm p:bool
}

global strat EqModusPonens "|- p"
{
    match "|- q :bool = p"
    kernel mp "|- q :bool = p" "|- q"
}

global strat Reflectivity "|- p = p"
{
    kernel refl p
}

global proof Commutativity "p = q |- q = p"
{
    "p = q |- (p = p) = (q = p)"
    "|- p = p"
    "p = q |- q = p"
}

""";

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