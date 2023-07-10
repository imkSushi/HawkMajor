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
        
        var firstThm = Prove(firstOutput, data, false);
        if (firstThm is null)
            return null;
        
        var secondThm = Prove(secondOutput, data, false);
        if (secondThm is null)
            return null;
        
        return (firstThm, secondThm);
    }

    protected static Theorem? Prove(Conjecture output, ProvingData data, bool applyNextInstruction)
    {
        for (var i = data.LocalTheorems.Count - 1; i >= 0; i--)
        {
            var localThm = data.LocalTheorems[i];
            var thm = output.CheckIfInstance(localThm, data.Kernel);

            if (thm is null)
                continue;
            if (!applyNextInstruction)
                return thm;

            var result = TryTheorem(thm, data);
            if (result is not null)
                return result;
        }

        var workspaceProved = data.Workspace.Prove(output);
        if (workspaceProved is null)
            return null;
        
        if (!applyNextInstruction)
            return workspaceProved;

        return TryTheorem(workspaceProved, data);
    }

    protected static Theorem? TryTheorem(Theorem theorem, ProvingData data)
    {
        var output = data.Conjecture.CheckIfInstance(theorem, data.Kernel);
        if (output is not null)
            return output;
                
        data.LocalTheorems.Add(theorem);
        var result = ApplyInstruction(data with {InstructionIndex = data.InstructionIndex + 1});
        if (result is not null)
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