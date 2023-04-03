﻿using System;
using EXGuard.Runtime.Dynamic;
using EXGuard.Runtime.Execution;

namespace EXGuard.Runtime.OpCodes {
	internal class PushRByte : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_PUSHR_BYTE; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			ctx.Stack.SetTopPosition(++sp);

			var regId = ctx.ReadByte();
			var slot = ctx.Registers[regId];
			ctx.Stack[sp] = new VMSlot { U1 = slot.U1 };

			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;
			state = ExecutionState.Next;
		}
	}

	internal class PushRWord : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_PUSHR_WORD; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			ctx.Stack.SetTopPosition(++sp);

			var regId = ctx.ReadByte();
			var slot = ctx.Registers[regId];
			ctx.Stack[sp] = new VMSlot { U2 = slot.U2 };

			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;
			state = ExecutionState.Next;
		}
	}

	internal class PushRDword : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_PUSHR_DWORD; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			ctx.Stack.SetTopPosition(++sp);

			var regId = ctx.ReadByte();
			var slot = ctx.Registers[regId];
			if (regId == ctx.Data.Constants.REG_SP || regId == ctx.Data.Constants.REG_BP)
				ctx.Stack[sp] = new VMSlot { O = new StackRef(slot.U4) };
			else
				ctx.Stack[sp] = new VMSlot { U4 = slot.U4 };

			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;
			state = ExecutionState.Next;
		}
	}

	internal class PushRQword : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_PUSHR_QWORD; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			ctx.Stack.SetTopPosition(++sp);

			var regId = ctx.ReadByte();
			var slot = ctx.Registers[regId];
			ctx.Stack[sp] = new VMSlot { U8 = slot.U8 };

			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;
			state = ExecutionState.Next;
		}
	}

	internal class PushRObject : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_PUSHR_OBJECT; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			ctx.Stack.SetTopPosition(++sp);

			var regId = ctx.ReadByte();
			var slot = ctx.Registers[regId];
			ctx.Stack[sp] = slot;

			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;
			state = ExecutionState.Next;
		}
	}

	internal class PushIDword : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_PUSHI_DWORD; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			ctx.Stack.SetTopPosition(++sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;

			ulong imm = ctx.ReadByte();
			imm |= ((ulong)ctx.ReadByte() << 8);
			imm |= ((ulong)ctx.ReadByte() << 16);
			imm |= ((ulong)ctx.ReadByte() << 24);
			var sx = (imm & 0x80000000) != 0 ? (0xffffffffUL << 32) : 0;
			ctx.Stack[sp] = new VMSlot { U8 = sx | imm };
			state = ExecutionState.Next;
		}
	}

	internal class PushIQword : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_PUSHI_QWORD; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			ctx.Stack.SetTopPosition(++sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;

			ulong imm = ctx.ReadByte();
			imm |= ((ulong)ctx.ReadByte() << 8);
			imm |= ((ulong)ctx.ReadByte() << 16);
			imm |= ((ulong)ctx.ReadByte() << 24);
			imm |= ((ulong)ctx.ReadByte() << 32);
			imm |= ((ulong)ctx.ReadByte() << 40);
			imm |= ((ulong)ctx.ReadByte() << 48);
			imm |= ((ulong)ctx.ReadByte() << 56);
			ctx.Stack[sp] = new VMSlot { U8 = imm };
			state = ExecutionState.Next;
		}
	}
}