using System;
using System.Collections;

namespace EXGuard.Core.Helpers.System.Collections
{
    public interface IStructuralComparable
    {
        Int32 CompareTo(Object other, IComparer comparer);
    }
}