using HawkMajor2.Extensions;
using HawkMajor2.Shadows;
using Valiant;

namespace HawkMajor2.Engine.StrategyInstructions;

public abstract record StrategyInstruction
{
    protected abstract Theorem? Apply(ProvingData data);

    protected static Theorem? Prove(ShadowTheorem shadowTheorem, ProvingData data, bool applyNextInstruction)
    {
        if (!shadowTheorem.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var output, out _))
            return null;

        return Prove(output, data, applyNextInstruction);
    }

    protected static (Theorem first, Theorem second)? Prove(ShadowTheorem first, ShadowTheorem second, ProvingData data)
    {
        if (!first.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var firstOutput, out _))
            return null;
        
        if (!second.ConvertToConjecture(data.TermMap, data.TypeMap, data.Conjecture.Premises, data.Kernel).Deconstruct(out var secondOutput, out _))
            return null;
        
        if (Prove(firstOutput, data, false).IsNull(out var firstThm))
            return null;
        
        if (Prove(secondOutput, data, false).IsNull(out var secondThm))
            return null;
        
        return (firstThm, secondThm);
    }

    protected static Theorem? Prove(Conjecture output, ProvingData data, bool applyNextInstruction)
    {
        for (var i = data.LocalTheorems.Count - 1; i >= 0; i--)
        {
            var localThm = data.LocalTheorems[i];

            if (output.CheckIfInstance(localThm, data.Kernel).IsNull(out var thm))
                continue;
            if (!applyNextInstruction)
                return thm;

            if (TryTheorem(thm, data).IsNotNull(out var result))
                return result;
        }

        if (data.Workspace.Prove(output).IsNull(out var workspaceProved))
            return null;
        
        if (!applyNextInstruction)
            return workspaceProved;

        return TryTheorem(workspaceProved, data);
    }

    protected static Theorem? TryTheorem(Theorem theorem, ProvingData data)
    {
        if (data.Conjecture.CheckIfInstance(theorem, data.Kernel).IsNotNull(out var output))
            return output;
                
        data.LocalTheorems.Add(theorem);
        if (ApplyInstruction(data with {InstructionIndex = data.InstructionIndex + 1}).IsNotNull(out var result))
            return result;
        data.LocalTheorems.RemoveAt(data.LocalTheorems.Count - 1);
                
        return null;
    }

    public static Theorem? ApplyInstruction(ProvingData data)
    {
        if (data.InstructionIndex >= data.Instructions.Count)
            return null;
        
        var instruction = data.Instructions[data.InstructionIndex];

        return instruction.Apply(data);
    }
}