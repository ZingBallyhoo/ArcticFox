using System;
using System.Threading;

namespace ArcticFox.Net.Event
{
    public abstract class RefCounted
    {
        private int m_refCount;
        private bool m_hasCreationRef;

        public int RefCount() => m_refCount;
        public bool HasCreationRef() => m_hasCreationRef;

        public RefCounted()
        {
            ResetRefCount();
        }
        
        public void ReleaseCreationRef()
        {
            if (!m_hasCreationRef) return;
            m_hasCreationRef = false;
            ReleaseRef();
        }
        
        public void ReleaseRef()
        {
            var count = Interlocked.Decrement(ref m_refCount);
            if (count < 0)
            {
                //Log.Error("NetEvent with RefCount less than 0!! ({RefCount})", m_refCount);
            }
            if (count == 0)
            {
                Cleanup();
            }
        }
        
        public void GetRef()
        {
            if (m_refCount == 0)
            {
                throw new Exception("Trying to revive NetEvent with 0 references");
            }
            Interlocked.Increment(ref m_refCount);
        }

        protected abstract void Cleanup();

        protected void ResetRefCount()
        {
            m_refCount = 1;
            m_hasCreationRef = true;
        }
    }
}