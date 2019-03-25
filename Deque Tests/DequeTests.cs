using Aprismatic.Cache;
using System;
using System.Collections.Generic;
using Xunit;

public class DequeTests
{
    private readonly Random _rnd = new Random();
    private const int iterations = 10;

    [Fact(DisplayName = "Constructors")]
    public void Constructors()
    {
        // setup & act
        var deque = new CacheDeque<object>();

        // assert
        Assert.Equal(0, deque.Count);
    }

    [Fact(DisplayName = "Push Head")]
    public void PushHead()
    {
        // setup
        var deque = new CacheDeque<int>();
        var queue = new Queue<int>();
        
        var val = GetRandomArray(iterations);

        // act & assert
        // insert 10 in the front
        Assert.Equal(0, deque.Count);

        for (var i = 0; i < iterations; i++)
        {
            var newVal = new DequeElem<int>(val[i]);
            deque.PushHead(newVal);
            queue.Enqueue(val[i]);

            Assert.Equal(i + 1, deque.Count);
        }

        Assert.True(DequeQueueWereEqual(deque,queue));
    }

    [Fact(DisplayName = "Detach Tail")]
    public void DetachTail()
    {
        // setup
        var deque = new CacheDeque<int>();
        var queue = new Queue<int>();

        var val = GetRandomArray(iterations);

        // act
        // insert 10 in the front
        for (var i = 0; i < iterations; i++)
        {
            var newVal = new DequeElem<int>(val[i]);
            deque.PushHead(newVal);
            queue.Enqueue(val[i]);
        }

        // act & assert
        for (var i = 0; i < iterations/2; i++)
        {
            var det = deque.DetachTail().item;
            var det2 = queue.Dequeue();
            Assert.Equal(det2, det);
        }

        Assert.True(DequeQueueWereEqual(deque, queue));

        Assert.Throws<ApplicationException>(() => deque.DetachTail());
    }

    [Fact(DisplayName = "Bubble")]
    public void Bubble()
    {
        /*{ // BUBBLE THE HEAD
            // setup
            // insert 10 in the back
            var deque = new CacheDeque<object>();

            const int iterations = 10;

            var obj = new object[iterations];
            for (var i = 0; i < iterations; i++)
            {
                obj[i] = new object();
                deque.PushTail(new DequeElem<object>(obj[i]));
            }

            // act
            var iter = deque.head;
            deque.Bubble(iter);

            // assert
            Assert.Same(obj[0], deque.head.item);
            Assert.Equal(iterations, deque.Count);
        }

        { // BUBBLE THE TAIL
            // setup
            // insert 10 in the back
            var deque = new CacheDeque<object>();

            const int iterations = 10;

            var obj = new object[iterations];
            for (var i = 0; i < iterations; i++)
            {
                obj[i] = new object();
                deque.PushTail(new DequeElem<object>(obj[i]));
            }

            // act
            var iter = deque.tail;
            deque.Bubble(iter);

            // assert
            Assert.Same(obj[iterations - 1], deque.head.item);
            Assert.Same(obj[iterations - 2], deque.tail.item);
            Assert.Same(obj[0], deque.head.next.item);
            Assert.Equal(iterations, deque.Count);
        }

        { // bubble the 3rd element (head.next.next)
            // setup
            // insert 10 in the back
            var deque = new CacheDeque<object>();

            const int iterations = 10;

            var obj = new object[iterations];
            for (var i = 0; i < iterations; i++)
            {
                obj[i] = new object();
                deque.PushTail(new DequeElem<object>(obj[i]));
            }

            // act
            var iter = deque.head; // first
            iter = iter.next;      // second
            iter = iter.next;      // third
            deque.Bubble(iter);

            // assert 3
            Assert.Same(obj[2], deque.head.item);
            Assert.Same(obj[0], deque.head.next.item);
            Assert.Equal(iterations, deque.Count);
        }

        { // bubble the only element
            // setup
            // insert 1 element
            var deque = new CacheDeque<object>();

            var obj = new object();
            deque.PushTail(new DequeElem<object>(obj));

            // act
            var iter = deque.head;
            deque.Bubble(iter);

            // assert 3
            Assert.Same(obj, deque.head.item);
            Assert.Equal(1, deque.Count);
        }*/
    }

    private bool DequeQueueWereEqual<T>(CacheDeque<T> d1, Queue<T> d2) // empties the deque and queue
    {
        if (d1.Count != d2.Count)
            return false;

        var cnt = d1.Count;
        for (var i = 0; i < cnt; i++)
        {
            var e1 = d1.DetachTail().item;
            var e2 = d2.Dequeue();
            if (!e1.Equals(e2))
                return false;
        }

        return true;
    }

    private int[] GetRandomArray(int size)
    {
        var res = new int[size];
        for (var i = 0; i < size; i++)
        {
            res[i] = _rnd.Next();
        }
        return res;
    }
}
