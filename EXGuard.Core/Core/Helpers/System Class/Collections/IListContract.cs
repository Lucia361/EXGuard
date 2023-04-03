namespace EXGuard.Core.Helpers.System.Collections
{
    using global::System;
    using global::System.Collections;
    using System.Diagnostics.Contracts;

    [ContractClassFor(typeof(IList))]
    internal abstract class IListContract : IList
    {
        int IList.Add(Object value)
        {
            //Contract.Ensures(((IList)this).Count == Contract.OldValue(((IList)this).Count) + 1);  // Not threadsafe
            // This method should return the index in which an item was inserted, but we have
            // some internal collections that don't always insert items into the list, as well
            // as an MSDN sample code showing us returning -1.  Allow -1 to mean "did not insert".
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < ((IList)this).Count);
            return default(int);
        }

        Object IList.this[int index]
        {
            get
            {
                //Contract.Requires(index >= 0);
                //Contract.Requires(index < ((IList)this).Count);
                return default(int);
            }
            set
            {
                //Contract.Requires(index >= 0);
                //Contract.Requires(index < ((IList)this).Count);
            }
        }

        bool IList.IsFixedSize
        {
            get { return default(bool); }
        }

        bool IList.IsReadOnly
        {
            get { return default(bool); }
        }

        bool ICollection.IsSynchronized
        {
            get { return default(bool); }
        }

        void IList.Clear()
        {
            //Contract.Ensures(((IList)this).Count == 0  || ((IList)this).IsFixedSize);  // not threadsafe
        }

        bool IList.Contains(Object value)
        {
            return default(bool);
        }

        void ICollection.CopyTo(Array array, int startIndex)
        {
            //Contract.Requires(array != null);
            //Contract.Requires(startIndex >= 0);
            //Contract.Requires(startIndex + ((IList)this).Count <= array.Length);
        }

        int ICollection.Count
        {
            get
            {
                return default(int);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return default(IEnumerator);
        }

        [Pure]
        int IList.IndexOf(Object value)
        {
            Contract.Ensures(Contract.Result<int>() >= -1);
            Contract.Ensures(Contract.Result<int>() < ((IList)this).Count);
            return default(int);
        }

        void IList.Insert(int index, Object value)
        {
            //Contract.Requires(index >= 0);
            //Contract.Requires(index <= ((IList)this).Count);  // For inserting immediately after the end.
            //Contract.Ensures(((IList)this).Count == Contract.OldValue(((IList)this).Count) + 1);  // Not threadsafe
        }

        void IList.Remove(Object value)
        {
            // No information if removal fails.
        }

        void IList.RemoveAt(int index)
        {
            //Contract.Requires(index >= 0);
            //Contract.Requires(index < ((IList)this).Count);
            //Contract.Ensures(((IList)this).Count == Contract.OldValue(((IList)this).Count) - 1);  // Not threadsafe
        }

        Object ICollection.SyncRoot
        {
            get
            {
                return default(Object);
            }
        }
    }
}