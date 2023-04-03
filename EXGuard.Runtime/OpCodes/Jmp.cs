﻿using System;
using EXGuard.Runtime.Dynamic;
using EXGuard.Runtime.Execution;

namespace EXGuard.Runtime.OpCodes {
	internal class Jmp : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_JMP; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var slot = ctx.Stack[sp];
			ctx.Stack.SetTopPosition(--sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;

			ctx.Registers[ctx.Data.Constants.REG_IP].U8 = slot.U8;
			state = ExecutionState.Next;
		}
	}

	internal class Jz : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_JZ; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var adrSlot = ctx.Stack[sp];
			var valSlot = ctx.Stack[sp - 1];
			sp -= 2;
			ctx.Stack.SetTopPosition(sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;

			if (valSlot.U8 == 0)
				ctx.Registers[ctx.Data.Constants.REG_IP].U8 = adrSlot.U8;
			state = ExecutionState.Next;
		}
	}

	internal class Jnz : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_JNZ; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var adrSlot = ctx.Stack[sp];
			var valSlot = ctx.Stack[sp - 1];
			sp -= 2;
			ctx.Stack.SetTopPosition(sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;

			if (valSlot.U8 != 0)
				ctx.Registers[ctx.Data.Constants.REG_IP].U8 = adrSlot.U8;
			state = ExecutionState.Next;
		}
	}

	internal class Swt : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_SWT; }
		}

		public unsafe void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var tblSlot = ctx.Stack[sp];
			var valSlot = ctx.Stack[sp - 1];
			sp -= 2;
			ctx.Stack.SetTopPosition(sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;

			uint index = valSlot.U4;
			ushort len = *(ushort*)(tblSlot.U8 - 2);
			if (index < len)
				ctx.Registers[ctx.Data.Constants.REG_IP].U8 += (ulong)(int)((uint*)tblSlot.U8)[index];
			state = ExecutionState.Next;
		}
	}
}