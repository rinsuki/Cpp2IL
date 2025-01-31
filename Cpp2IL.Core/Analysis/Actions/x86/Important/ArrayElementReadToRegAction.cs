﻿using Cpp2IL.Core.Analysis.Actions.Base;
using Cpp2IL.Core.Analysis.ResultModels;
using Mono.Cecil;
using Instruction = Iced.Intel.Instruction;

namespace Cpp2IL.Core.Analysis.Actions.x86.Important
{
    public class ArrayElementReadToRegAction : AbstractArrayOffsetReadAction<Instruction>
    {
        public ArrayElementReadToRegAction(MethodAnalysis<Instruction> context, Instruction instruction) : base(context, instruction)
        {
            var arrayReg = Utils.Utils.GetRegisterNameNew(instruction.MemoryBase);
            var offsetReg = Utils.Utils.GetRegisterNameNew(instruction.MemoryIndex);

            ArrayLocal = context.GetLocalInReg(arrayReg);
            OffsetLocal = context.GetLocalInReg(offsetReg);
            
            if(ArrayLocal?.Type?.IsArray != true)
                return;

            ArrType = (ArrayType) ArrayLocal.Type;
            
            if(ArrType == null)
                return;

            ArrayElementType = ArrType.GetElementType();

            var destReg = Utils.Utils.GetRegisterNameNew(instruction.Op0Register);

            LocalMade = context.MakeLocal(ArrType.ElementType, reg: destReg);
            
            RegisterUsedLocal(ArrayLocal, context);
            
            if(OffsetLocal != null)
                RegisterUsedLocal(OffsetLocal, context);
        }
    }
}