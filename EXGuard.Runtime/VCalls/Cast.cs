﻿using System;
using EXGuard.Runtime.Dynamic;
using EXGuard.Runtime.Execution;

namespace EXGuard.Runtime.VCalls {
	internal class Cast : IVCall {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.VCALL_CAST; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var typeSlot = ctx.Stack[sp--];
			var valSlot = ctx.Stack[sp];

			bool castclass = (typeSlot.U4 & 0x80000000) != 0;
			var castType = (Type)ctx.Instance.Data.LookupReference(typeSlot.U4 & ~0x80000000);
			if (Type.GetTypeCode(castType) == TypeCode.String && valSlot.O == null)
				valSlot.O = ctx.Instance.Data.LookupString(valSlot.U4);
			else if (valSlot.O == null)
				valSlot.O = null;
			else if (!castType.IsInstanceOfType(valSlot.O)) {
				valSlot.O = null;
				if (castclass)
					throw new InvalidCastException();
			}
			ctx.Stack[sp] = valSlot;

			ctx.Stack.SetTopPosition(sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;
			state = ExecutionState.Next;
		}
	}
}