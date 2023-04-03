﻿using System;
using EXGuard.Runtime.Dynamic;
using EXGuard.Runtime.Execution;

namespace EXGuard.Runtime.OpCodes {
	internal class LindByte : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_LIND_BYTE; }
		}

		public unsafe void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var adrSlot = ctx.Stack[sp];

			VMSlot valSlot;
			if (adrSlot.O is IReference) {
				valSlot = ((IReference)adrSlot.O).GetValue(ctx, PointerType.BYTE);
			}
			else {
				var ptr = (byte*)adrSlot.U8;
				valSlot = new VMSlot { U1 = *ptr };
			}
			ctx.Stack[sp] = valSlot;

			state = ExecutionState.Next;
		}
	}

	internal class LindWord : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_LIND_WORD; }
		}

		public unsafe void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var adrSlot = ctx.Stack[sp];

			VMSlot valSlot;
			if (adrSlot.O is IReference) {
				valSlot = ((IReference)adrSlot.O).GetValue(ctx, PointerType.WORD);
			}
			else {
				var ptr = (ushort*)adrSlot.U8;
				valSlot = new VMSlot { U2 = *ptr };
			}
			ctx.Stack[sp] = valSlot;

			state = ExecutionState.Next;
		}
	}

	internal class LindDword : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_LIND_DWORD; }
		}

		public unsafe void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var adrSlot = ctx.Stack[sp];

			VMSlot valSlot;
			if (adrSlot.O is IReference) {
				valSlot = ((IReference)adrSlot.O).GetValue(ctx, PointerType.DWORD);
			}
			else {
				var ptr = (uint*)adrSlot.U8;
				valSlot = new VMSlot { U4 = *ptr };
			}
			ctx.Stack[sp] = valSlot;

			state = ExecutionState.Next;
		}
	}

	internal class LindQword : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_LIND_QWORD; }
		}

		public unsafe void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var adrSlot = ctx.Stack[sp];

			VMSlot valSlot;
			if (adrSlot.O is IReference) {
				valSlot = ((IReference)adrSlot.O).GetValue(ctx, PointerType.QWORD);
			}
			else {
				var ptr = (ulong*)adrSlot.U8;
				valSlot = new VMSlot { U8 = *ptr };
			}
			ctx.Stack[sp] = valSlot;

			state = ExecutionState.Next;
		}
	}

	internal class LindObject : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_LIND_OBJECT; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var adrSlot = ctx.Stack[sp];

			VMSlot valSlot;
			if (adrSlot.O is IReference) {
				valSlot = ((IReference)adrSlot.O).GetValue(ctx, PointerType.OBJECT);
			}
			else {
				throw new ExecutionEngineException();
			}
			ctx.Stack[sp] = valSlot;

			state = ExecutionState.Next;
		}
	}

	internal class LindPtr : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_LIND_PTR; }
		}

		public unsafe void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var adrSlot = ctx.Stack[sp];

			VMSlot valSlot;
			if (adrSlot.O is IReference) {
				valSlot = ((IReference)adrSlot.O).GetValue(ctx, (IntPtr.Size == 8) ? PointerType.QWORD : PointerType.DWORD);
			}
			else {
				if (IntPtr.Size == 8) {
					var ptr = (ulong*)adrSlot.U8;
					valSlot = new VMSlot { U8 = *ptr };
				}
				else {
					var ptr = (uint*)adrSlot.U8;
					valSlot = new VMSlot { U4 = *ptr };
				}
			}
			ctx.Stack[sp] = valSlot;

			state = ExecutionState.Next;
		}
	}
}