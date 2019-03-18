using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Aprismatic.Cache;
using Xunit;

public class LRUCacheTests
{
    private static Random rnd = new Random();
    private static readonly int OpTimeMs = 100;

    public int slowOp(string arg)
    {
        return Convert.ToInt32(arg);
    }

    [Fact]
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
}
