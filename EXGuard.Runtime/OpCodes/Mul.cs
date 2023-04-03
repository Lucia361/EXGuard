﻿using System;
using EXGuard.Runtime.Dynamic;
using EXGuard.Runtime.Execution;

namespace EXGuard.Runtime.OpCodes {
	internal class MulDword : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_MUL_DWORD; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var op1Slot = ctx.Stack[sp - 1];
			var op2Slot = ctx.Stack[sp];
			sp -= 1;
			ctx.Stack.SetTopPosition(sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;

			var fl = ctx.Registers[ctx.Data.Constants.REG_FL].U1;

			var slot = new VMSlot();
			ulong result = op1Slot.U4 * op2Slot.U4;
			slot.U4 = (uint)result;
			ctx.Stack[sp] = slot;

			byte mask1 = (byte)(ctx.Data.Constants.FL_ZERO | ctx.Data.Constants.FL_SIGN | ctx.Data.Constants.FL_UNSIGNED);
			byte mask2 = (byte)(ctx.Data.Constants.FL_CARRY | ctx.Data.Constants.FL_OVERFLOW);
			byte ovF = 0;
			if ((fl & ctx.Data.Constants.FL_UNSIGNED) != 0) {
				if ((result & (0xffffffff << 32)) != 0)
					ovF = mask2;
			}
			else {
				result = (ulong)((int)op1Slot.U4 * (int)op2Slot.U4);
				if ((result >> 63) != (slot.U4 >> 31)) {
					ovF = mask2;
				}
			}
			fl = (byte)((fl & ~mask2) | ovF);
			Utils.UpdateFL(ctx, op1Slot.U4, op2Slot.U4, slot.U4, slot.U4, ref fl, mask1);
			ctx.Registers[ctx.Data.Constants.REG_FL].U1 = fl;

			state = ExecutionState.Next;
		}
	}

	internal class MulQword : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_MUL_QWORD; }
		}

		static ulong Carry(ulong a, ulong b) {
			// https://stackoverflow.com/questions/1815367/multiplication-of-large-numbers-how-to-catch-overflow
			ulong lo1 = a & 0xffffffff, hi1 = a >> 32;
			ulong lo2 = b & 0xffffffff, hi2 = b >> 32;

			ulong s0, s1, s2;
			ulong x = lo1 * lo2;
			s0 = x & 0xffffffff;

			x = hi1 * lo2 + (x >> 32);
			s1 = x & 0xffffffff;
			s2 = x >> 32;

			x = s1 + lo1 * hi2;
			s1 = x & 0xffffffff;

			x = s2 + hi1 * hi2 + (x >> 32);
			return x;
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var op1Slot = ctx.Stack[sp - 1];
			var op2Slot = ctx.Stack[sp];
			sp -= 1;
			ctx.Stack.SetTopPosition(sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;

			var fl = ctx.Registers[ctx.Data.Constants.REG_FL].U1;

			var slot = new VMSlot();
			ulong result = op1Slot.U8 * op2Slot.U8;
			slot.U8 = result;
			ctx.Stack[sp] = slot;

			byte mask1 = (byte)(ctx.Data.Constants.FL_ZERO | ctx.Data.Constants.FL_SIGN | ctx.Data.Constants.FL_UNSIGNED);
			byte mask2 = (byte)(ctx.Data.Constants.FL_CARRY | ctx.Data.Constants.FL_OVERFLOW);
			byte ovF = 0;
			if ((fl & ctx.Data.Constants.FL_UNSIGNED) != 0) {
				if (Carry(op1Slot.U8, op2Slot.U8) != 0)
					ovF = mask2;
			}
			else {
				if ((result >> 63) != ((op1Slot.U8 ^ op2Slot.U8) >> 63))
					ovF = mask2;
			}
			fl = (byte)((fl & ~mask2) | ovF);
			Utils.UpdateFL(ctx, op1Slot.U4, op2Slot.U8, slot.U8, slot.U8, ref fl, mask1);
			ctx.Registers[ctx.Data.Constants.REG_FL].U1 = fl;

			state = ExecutionState.Next;
		}
	}

	internal class MulR32 : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_MUL_R32; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var op1Slot = ctx.Stack[sp - 1];
			var op2Slot = ctx.Stack[sp];
			sp -= 1;
			ctx.Stack.SetTopPosition(sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;

			var slot = new VMSlot();
			slot.R4 = op2Slot.R4 * op1Slot.R4;
			ctx.Stack[sp] = slot;

			byte mask = (byte)(ctx.Data.Constants.FL_ZERO | ctx.Data.Constants.FL_SIGN | ctx.Data.Constants.FL_UNSIGNED);
			var fl = (byte)(ctx.Registers[ctx.Data.Constants.REG_FL].U1 & ~mask);
			if (slot.R4 == 0)
				fl |= ctx.Data.Constants.FL_ZERO;
			else if (slot.R4 < 0)
				fl |= ctx.Data.Constants.FL_SIGN;
			ctx.Registers[ctx.Data.Constants.REG_FL].U1 = fl;

			state = ExecutionState.Next;
		}
	}

	internal class MulR64 : IOpCode {
		public byte Code {
			get { return VMInstance.STATIC_Instance.Data.Constants.OP_MUL_R64; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[ctx.Data.Constants.REG_SP].U4;
			var op1Slot = ctx.Stack[sp - 1];
			var op2Slot = ctx.Stack[sp];
			sp -= 1;
			ctx.Stack.SetTopPosition(sp);
			ctx.Registers[ctx.Data.Constants.REG_SP].U4 = sp;

			var slot = new VMSlot();
			slot.R8 = op2Slot.R8 * op1Slot.R8;
			ctx.Stack[sp] = slot;

			byte mask = (byte)(ctx.Data.Constants.FL_ZERO | ctx.Data.Constants.FL_SIGN | ctx.Data.Constants.FL_UNSIGNED);
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