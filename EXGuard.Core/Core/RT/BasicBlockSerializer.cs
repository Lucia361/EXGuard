﻿using System;
using System.IO;
using dnlib.DotNet.Pdb;
using EXGuard.Core.AST.IL;
using EXGuard.Core.AST.ILAST;

namespace EXGuard.Core.RT {
	internal class BasicBlockSerializer {
		VMRuntime rt;

		public BasicBlockSerializer(VMRuntime rt) {
			this.rt = rt;
		}

		public uint ComputeLength(ILBlock block) {
			uint len = 0;
			foreach (var instr in block.Content)
				len += ComputeLength(instr);
			return len;
		}

		public uint ComputeLength(ILInstruction instr) {
			uint len = 2;
			if (instr.Operand != null) {
				if (instr.Operand is ILRegister)
					len++;
				else if (instr.Operand is ILImmediate) {
					var value = ((ILImmediate)instr.Operand).Value;
					if (value is uint || value is int || value is float)
						len += 4;
					else if (value is ulong || value is long || value is double)
						len += 8;
					else
						throw new NotSupportedException();
				}
				else if (instr.Operand is ILRelReference) {
					len += 4;
				}
				else
					throw new NotSupportedException();
			}
			return len;
		}

		public uint ComputeOffset(ILBlock block, uint offset) {
			foreach (var instr in block.Content) {
				instr.Offset = offset;
				offset += 2;
				if (instr.Operand != null) {
					if (instr.Operand is ILRegister)
						offset++;
					else if (instr.Operand is ILImmediate) {
						var value = ((ILImmediate)instr.Operand).Value;
						if (value is uint || value is int || value is float)
							offset += 4;
						else if (value is ulong || value is long || value is double)
							offset += 8;
						else
							throw new NotSupportedException();
					}
					else if (instr.Operand is ILRelReference) {
						offset += 4;
					}
					else
						throw new NotSupportedException();
				}
			}
			return offset;
		}

		static bool Equals(SequencePoint a, SequencePoint b) {
			return a.Document.Url == b.Document.Url && a.StartLine == b.StartLine;
		}

		public void WriteData(ILBlock block, BinaryWriter writer) {
			uint offset = 0;
			foreach (var instr in block.Content) {
				writer.Write(rt.Descriptor.Architecture.OpCodes[instr.OpCode]);
				// Leave a padding to let BasicBlockChunk fixup block exit key
				writer.Write(rt.Descriptor.RandomGenerator.NextByte());
				offset += 2;

				if (instr.Operand != null) {
					if (instr.Operand is ILRegister) {
						writer.Write(rt.Descriptor.Architecture.Registers[((ILRegister)instr.Operand).Register]);
						offset++;
					}
					else if (instr.Operand is ILImmediate) {
						var value = ((ILImmediate)instr.Operand).Value;
						if (value is int) {
							writer.Write((int)value);
							offset += 4;
						}
						else if (value is uint) {
							writer.Write((uint)value);
							offset += 4;
						}
						else if (value is long) {
							writer.Write((long)value);
							offset += 8;
						}
						else if (value is ulong) {
							writer.Write((ulong)value);
							offset += 8;
						}
						else if (value is float) {
							writer.Write((float)value);
							offset += 4;
						}
						else if (value is double) {
							writer.Write((double)value);
							offset += 8;
						}
					}
					else
						throw new NotSupportedException();
				}
			}
		}
	}
}