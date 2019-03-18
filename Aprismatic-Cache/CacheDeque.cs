using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

[assembly: InternalsVisibleTo("AprismaticCacheTests")]

namespace Aprismatic.Cache
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal class CacheDeque<T>
    {
        public int Count { get; private set; }
        private DequeElem<T> head, tail;

        public CacheDeque()
        {
            Count = 0;
            head = null;
            tail = null;
        }

        public void PushHead(DequeElem<T> newElem)
        {
            Count++;

            if (Count == 1) // => it was 0 before increment
            {
                head = newElem;
                tail = newElem;
                return;
            }

            newElem.next = head;
            head.prev = newElem;
            head = newElem;
        }

        public DequeElem<T> DetachTail()
        {
            if (tail == null)
                throw new ApplicationException("Can't pop from an empty deque");

            var tmp = tail;

            tail = tail.prev;
            if (tail != null)
                tail.next = null;
            else // tail == null
                head = null;

            Count--;

            tmp.next = null;
            tmp.prev = null;

            return tmp;
        }

        public void Bubble(DequeElem<T> elem)
        {
            if (elem == head)
                return;

            if (elem == tail)
            {
                DetachTail(); // changes elem
            }
            else // now guaranteed to be in the middle of the deque
            {
                elem.prev.next = elem.next;
                elem.next.prev = elem.prev;
                Count--;

                elem.prev = null;
                elem.next = null;
            }

            PushHead(elem);
        }

        private string DebuggerDisplay
        {
            get
            {
                if (tail == null)
                    return "[EMPTY DEQUE]";

                var sb = new StringBuilder("[tail] ");

                var iter = tail;
                while (iter.prev != null)
                {
                    sb.Append(iter.item);
                    sb.Append(" <-> ");
                    iter = iter.prev;
                }

                sb.Append(head.item);
                sb.Append(" [head]");

                return sb.ToString();
            }
        }
    }
}
