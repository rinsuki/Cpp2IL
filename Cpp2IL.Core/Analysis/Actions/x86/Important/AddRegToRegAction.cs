﻿using System.Collections.Generic;
using Cpp2IL.Core.Analysis.Actions.Base;
using Cpp2IL.Core.Analysis.ResultModels;
using Mono.Cecil.Cil;
using Instruction = Iced.Intel.Instruction;

namespace Cpp2IL.Core.Analysis.Actions.x86.Important
{
    public class AddRegToRegAction : BaseAction<Instruction>
    {
        private LocalDefinition? _firstOp;
        private IAnalysedOperand? _secondOp;

        public AddRegToRegAction(MethodAnalysis<Instruction> context, Instruction instruction) : base(context, instruction)
        {
            var firstReg = Utils.Utils.GetRegisterNameNew(instruction.Op0Register);
            var secondReg = Utils.Utils.GetRegisterNameNew(instruction.Op1Register);

            _firstOp = context.GetLocalInReg(firstReg);
            _secondOp = context.GetOperandInRegister(secondReg);
            
            if(_firstOp != null)
                RegisterUsedLocal(_firstOp, context);
            
            if(_secondOp is LocalDefinition l)
                RegisterUsedLocal(l, context);
        }

        public override Mono.Cecil.Cil.Instruction[] ToILInstructions(MethodAnalysis<Instruction> context, ILProcessor processor)
        {
            if (_firstOp == null || _secondOp == null)
                throw new TaintedInstructionException("Missing an argument");

            if (_firstOp.Variable == null)
                throw new TaintedInstructionException($"AddR2R: First operand, {_firstOp}, has been stripped or has no variable");
            
            List<Mono.Cecil.Cil.Instruction> ret = new List<Mono.Cecil.Cil.Instruction>();
            
            //Load arg one
            ret.AddRange(_firstOp.GetILToLoad(context, processor));
            
            //Load arg two
            ret.AddRange(_secondOp.GetILToLoad(context, processor));
            
            //Add
            ret.Add(processor.Create(OpCodes.Add));

            //Set local
            ret.Add(processor.Create(OpCodes.Stloc, _firstOp.Variable));

            return ret.ToArray();
        }

        public override string? ToPsuedoCode()
        {
            return $"{_firstOp?.Name} += {_secondOp?.GetPseudocodeRepresentation()}";
        }

        public override string ToTextSummary()
        {
            return $"[!] Adds {_firstOp} and {_secondOp} and stores the result in {_firstOp}";
        }
        
        public override bool IsImportant()
        {
            return true;
        }
    }
}