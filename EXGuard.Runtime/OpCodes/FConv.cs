﻿using System;
using EXGuard.Runtime.Dynamic;
using EXGuard.Runtime.Execution;

namespace EXGuard.Runtime.OpCodes {
	internal class FConvR32 : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_FCONV_R32; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var valueSlot = ctx.Stack[sp];

			valueSlot.R4 = (long)valueSlot.U8;

			ctx.Stack[sp] = valueSlot;

			state = ExecutionState.Next;
		}
	}

	internal class FConvR64 : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_FCONV_R64; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var valueSlot = ctx.Stack[sp];

			byte fl = ctx.Registers[ctx.Data.Constants.REG_FL].U1;
			if ((fl & ctx.Data.Constants.FL_UNSIGNED) != 0) {
				valueSlot.R8 = valueSlot.U8;
			}
			else {
				valueSlot.R8 = (long)valueSlot.U8;
			}
			ctx.Registers[ctx.Data.Constants.REG_FL].U1 = (byte)(fl & ~ctx.Data.Constants.FL_UNSIGNED);

			ctx.Stack[sp] = valueSlot;

			state = ExecutionState.Next;
		}
	}

	internal class FConvR32R64 : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_FCONV_R32_R64; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var valueSlot = ctx.Stack[sp];
			valueSlot.R8 = valueSlot.R4;
			ctx.Stack[sp] = valueSlot;

			state = ExecutionState.Next;
		}
	}

	internal class FConvR64R32 : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_FCONV_R64_R32; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var valueSlot = ctx.Stack[sp];
			valueSlot.R4 = (float)valueSlot.R8;
			ctx.Stack[sp] = valueSlot;

			state = ExecutionState.Next;
		}
	}
}