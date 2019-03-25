using Aprismatic.Cache;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class LRUCacheTests
{
    private static readonly int OpTimeMs = 100;

    public int slowOp(string arg)
    {
        return Convert.ToInt32(arg);
    }

    [Fact(DisplayName = "Single Thread - Simple")]
    public void TestCacheSingleThread()
    {
        // setup
        const int size = 5;
        var cache = new LRUCache<string, int>(size, slowOp);

        // warm the cache up and check
        for (var i = 0; i < size; i++)
        {
            var res = cache.Get(i.ToString(), out var hit);
            Assert.Equal(res, i);
            Assert.False(hit);
        }

        Thread.Sleep(5); // let processing thread finish up

        // now should be all hits
        for (var i = 0; i < size; i++)
        {
            var res = cache.Get(i.ToString(), out var hit);
            Assert.Equal(res, i);
            Assert.True(hit);
        }

        Thread.Sleep(5); // let processing thread finish up

        { // insert new one, oldest one should get evicted
            var res = cache.Get(size.ToString(), out var hit); // value 'size' was not yet inserted
            Assert.Equal(size, res);
            Assert.False(hit);
        }

        Thread.Sleep(5); // let processing thread finish up

        {
            var res = cache.Get(size.ToString(), out var hit); // value 'size' was already inserted
            Assert.Equal(size, res);
            Assert.True(hit);
        }

        Thread.Sleep(5); // let processing thread finish up

        {
            var res = cache.Get("1", out var hit); // value '1' should still be in the cache
            Assert.Equal(1, res);
            Assert.True(hit);
        }

        Thread.Sleep(5); // let processing thread finish up

        {
            var res = cache.Get("0", out var hit); // value '0' should be evicted by now
            Assert.Equal(0, res);
            Assert.False(hit);
        }
    }

    [Fact(DisplayName = "Single Thread")]
    public void StressTestCacheSingleThread()
    {
        {
            var cache = new LRUCache<string, int>(500, slowOp);
            Request(cache, 100000, 1, 500); // fill

            var hitrate = Request(cache, 1000, 1, 500);

            Assert.True(Math.Abs(hitrate - 1) < 0.0001d);
        }

        {
            var cache = new LRUCache<string, int>(500, slowOp);

            Request(cache, 100000, 1, 500); // fill

            var missalot = Request(cache, 100, 501, int.MaxValue - 1);

            Assert.True(missalot < 0.0001d);
        }

        {
            var cache = new LRUCache<string, int>(500, slowOp);

            Request(cache, 100000, 1, 500); // fill

            var fifty = Request(cache, 100, 250, 750);

            Assert.True(Math.Abs(fifty - 0.5) < 0.1d);
        }
    }

    [Fact(DisplayName = "Multiple Threads")]
    public void StressTestCacheMultipleThreads()
    {
        var left = 1;
        var right = 500;
        var size = right;

        {
            var cache = new LRUCache<string, int>(size, slowOp);
            Request(cache, 10000, left, right); // warmup

            var tasks = new List<Task<double>>();
            const int threadCount = 50;
            for (var i = 0; i < threadCount; i++)
            {
                var t = new Task<double>(() => Request(cache, 100000, left, right));
                t.Start();
                tasks.Add(t);
            }

            double accumulator = 0;
            foreach (var t in tasks)
            {
                t.Wait();
                accumulator += t.Result;
            }

            accumulator /= threadCount;

            Assert.True(Math.Abs(accumulator - 1) < 0.0001d);
        }

        {
            var cache = new LRUCache<string, int>(size, slowOp);
            Request(cache, 10000, left, right); // warmup

            var tasks = new List<Task<double>>();
            const int threadCount = 50;
            for (var i = 0; i < threadCount; i++)
            {
                var t = new Task<double>(() => Request(cache, 100000, left, right));
                t.Start();
                tasks.Add(t);
            }

            var missalot = Request(cache, 50000, right+1, int.MaxValue-1);

            foreach (var t in tasks) t.Wait();

            Assert.True(missalot < 0.0001d);
        }

        {
            var cache = new LRUCache<string, int>(size, slowOp);
            Request(cache, 10000, left, right); // warmup

            var tasks = new List<Task<double>>();
            const int threadCount = 50;
            for (var i = 0; i < threadCount; i++)
            {
                var t = new Task<double>(() => Request(cache, 100000, left, right));
                t.Start();
                tasks.Add(t);
            }

            var fifty = Request(cache, 50000, right - (right - left + 1) / 2, right + (right - left + 1) / 2);

            foreach (var t in tasks) t.Wait();

            Assert.True(Math.Abs(fifty - 0.5) < 0.1d);  // hit rate ends up ~4% higher than 50%, not sure why
        }
    }

    private double Request(LRUCache<string, int> cache, int amount, int min, int max)
    {
        var hits = 0;
        bool hit;
        var rnd = GetNewRandom();

        for (var i = 0; i < amount; i++)
        {
            cache.Get(rnd.Next(min, max + 1).ToString(), out hit);
            hits += hit ? 1 : 0;
        }

        return ((double)hits) / ((double) amount);
    }

    private static Random GetNewRandom()
    {
        var seed = Guid.NewGuid().GetHashCode();
        var res = new Random(seed);
        return res;
    }
}
