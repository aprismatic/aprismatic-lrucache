using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Aprismatic.Cache
{
    public class LRUCache<K, V> where K : class
    {
        private ConcurrentDictionary<K, DequeElem<(K, V)>> _dict;
        private CacheDeque<(K, V)> _deque;

        private int _size;
        private Func<K, V> _eval;

        private BlockingCollection<(bool,DequeElem<(K, V)>)> _processingQ;
        private Thread _processingThread;


        public LRUCache(int cacheSize, Func<K, V> evaluationFunction)
        {
            _deque = new CacheDeque<(K, V)>();
            _size = cacheSize;
            _dict = new ConcurrentDictionary<K, DequeElem<(K, V)>>();
            _eval = evaluationFunction;
            _processingQ = new BlockingCollection<(bool, DequeElem<(K, V)>)>();

            _processingThread = new Thread(Process);
            _processingThread.Start();
        }

        ~LRUCache()
        {
            _processingThread.Abort();
        }


        public V Get(K key) => Get(key, out _);

        public V Get(K key, out bool hit)
        {
            var tmpHit = true;
            var newElem = _dict.GetOrAdd(key, k =>
            {
                tmpHit = false;
                return new DequeElem<(K, V)>((k, _eval(k)));
            });

            hit = tmpHit;
            var result = newElem.item.Item2;

            _processingQ.Add((tmpHit, newElem));

            return result;
        }


        // This executes in a separate thread
        // This is the only thread that touches the links in the queue (and only the links)
        private void Process()
        {
            bool hit;
            DequeElem<(K, V)> elem;

            while (true)
            {
                (hit, elem) = _processingQ.Take(); // sleeps if internal queue is empty

                if (hit) // retrieved from cache
                {
                    _deque.Bubble(elem);
                    continue;
                }

                _deque.PushHead(elem); // new element

                if (_deque.Count > _size) // trim cache
                {
                    var tail = _deque.DetachTail();
                    _dict.TryRemove(tail.item.Item1, out _); // drop the tail item from the cache
                }
            }
        }
    }
}
