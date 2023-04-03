// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Interface:  IReadOnlyCollection<T>
** 
** <OWNER>matell</OWNER>
**
** Purpose: Base interface for read-only generic lists.
** 
===========================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using EXGuard.Core.Helpers.System.Diagnostics.Contracts;

namespace EXGuard.Core.Helpers.System.Collections.Generic
{
    [ContractClassFor(typeof(IReadOnlyCollection<>))]
    internal abstract class IReadOnlyCollectionContract<T> : IReadOnlyCollection<T>
    {
        int IReadOnlyCollection<T>.Count {
            get {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return default(int);
            }
        }
 
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return default(IEnumerator<T>);
        }
 
        IEnumerator IEnumerable.GetEnumerator()
        {
            return default(IEnumerator);
        }
    }
}