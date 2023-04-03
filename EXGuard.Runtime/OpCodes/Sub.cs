﻿using System;
using EXGuard.Runtime.Dynamic;
using EXGuard.Runtime.Execution;

namespace EXGuard.Runtime.OpCodes {
	internal class SubR32 : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_SUB_R32; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var op1Slot = ctx.Stack[sp - 1];
			var op2Slot = ctx.Stack[sp];
			sp -= 1;
			ctx.Stack.SetTopPosition(sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;

			var slot = new VMSlot();
			slot.R4 = op1Slot.R4 - op2Slot.R4;
			ctx.Stack[sp] = slot;

			byte mask = (byte)(ctx.Data.Constants.FL_ZERO | ctx.Data.Constants.FL_SIGN | ctx.Data.Constants.FL_OVERFLOW | ctx.Data.Constants.FL_CARRY);
			var fl = (byte)(ctx.Registers[ctx.Data.Constants.REG_FL].U1 & ~mask);
			if (slot.R4 == 0)
				fl |= ctx.Data.Constants.FL_ZERO;
			else if (slot.R4 < 0)
				fl |= ctx.Data.Constants.FL_SIGN;
			ctx.Registers[ctx.Data.Constants.REG_FL].U1 = fl;

			state = ExecutionState.Next;
		}
	}

	internal class SubR64 : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_SUB_R64; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var op1Slot = ctx.Stack[sp - 1];
			var op2Slot = ctx.Stack[sp];
			sp -= 1;
			ctx.Stack.SetTopPosition(sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;

			var slot = new VMSlot();
			slot.R8 = op1Slot.R8 - op2Slot.R8;
			ctx.Stack[sp] = slot;

			byte mask = (byte)(ctx.Data.Constants.FL_ZERO | ctx.Data.Constants.FL_SIGN | ctx.Data.Constants.FL_OVERFLOW | ctx.Data.Constants.FL_CARRY);
			var fl = (byte)(ctx.Registers[ctx.Data.Constants.REG_FL].U1 & ~mask);
			if (slot.R8 == 0)
				fl |= ctx.Data.Constants.FL_ZERO;
			else if (slot.R8 < 0)
				fl |= ctx.Data.Constants.FL_SIGN;
			ctx.Registers[ctx.Data.Constants.REG_FL].U1 = fl;

			state = ExecutionState.Next;
		}
	}
}