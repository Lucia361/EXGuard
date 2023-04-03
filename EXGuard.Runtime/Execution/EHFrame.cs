﻿using System;

namespace EXGuard.Runtime.Execution {
	internal struct EHFrame {
		public byte EHType;
		public ulong FilterAddr;
		public ulong HandlerAddr;
		public Type CatchType;

		public VMSlot BP;
		public VMSlot SP;
	}
}