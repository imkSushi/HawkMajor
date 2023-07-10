using HawkMajor2.Engine.StrategyInstructions;
using HawkMajor2.Shadows.ShadowTerms;
using HawkMajor2.Shadows.ShadowTypes;
using Valiant;
using Valiant.Terms;

namespace HawkMajor2.Engine;

public struct ProvingData
{
    public Conjecture Conjecture;
    public Workspace Workspace;
    public Dictionary<ShadowFixed, Term> TermMap;
    public Dictionary<ShadowTyFixed, Type> TypeMap;
    public int InstructionIndex;
    public List<Theorem> LocalTheorems;
    public List<StrategyInstruction> Instructions;
    
    public Kernel Kernel => Workspace.Kernel;

    public ProvingData(Conjecture conjecture,
        Workspace workspace,
        Dictionary<ShadowFixed, Term> termMap,
        Dictionary<ShadowTyFixed, Type> typeMap,
        int instructionIndex,
        List<Theorem> localTheorems,
        List<StrategyInstruction> instructions)
    {
        Conjecture = conjecture;
        Workspace = workspace;
        TermMap = termMap;
        TypeMap = typeMap;
        InstructionIndex = instructionIndex;
        LocalTheorems = localTheorems;
        Instructions = instructions;
    }
}