#if DEBUG

using AsyncKeyedLock;
using BlogAngular.Application.Blog.Common;
using BlogAngular.Application.Blog.Tags.Queries.Listing;
using BlogAngular.Test.Data;
using BlogAngular.Web.Features;
using FluentAssertions;
using MyTested.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BlogAngular.Test.SlowTest
{

    public class AsyncKeyedLockTest
    {
        private readonly ITestOutputHelper _output;
        private readonly StripedAsyncKeyedLocker<string> _keyedLocker = new();

        public AsyncKeyedLockTest(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public class Async : AsyncKeyedLockTest
        {
            public Async(ITestOutputHelper output) : base(output) { }

            [Theory]
            [InlineData(100, 100, 100, 10, 100)]
            [InlineData(102, 100, 10, 2, 10)]
            [InlineData(103, 100, 50, 5, 50)]
            [InlineData(101, 100, 1, 1, 1)]
            public async Task ShouldApplyParallelismCorrectly(int id, int numberOfThreads, int numberOfKeys, int minParallelism,
                int maxParallelism)
            {
                var runningTasksIndex = new ConcurrentDictionary<int, int>();
                var parallelismLock = new object();
                var currentParallelism = 0;
                var peakParallelism = 0;

                var threads = Enumerable.Range(0, numberOfThreads)
                    .Select(i =>
                        Task.Run(async () => await OccupyTheLockALittleBit(i % numberOfKeys)))
                    .ToList();

                await Task.WhenAll(threads);

                peakParallelism.Should().BeLessOrEqualTo(maxParallelism);
                peakParallelism.Should().BeGreaterOrEqualTo(minParallelism);

                _output.WriteLine("Peak parallelism was " + peakParallelism);

                async Task OccupyTheLockALittleBit(int key)
                {
                    using (await _keyedLocker.LockAsync(key.ToString()))
                    {
                        var incrementedCurrentParallelism = Interlocked.Increment(ref currentParallelism);

                        lock (parallelismLock)
                        {
                            peakParallelism = Math.Max(incrementedCurrentParallelism, peakParallelism);
                        }

                        var currentTaskId = Task.CurrentId ?? -1;

                        if (!runningTasksIndex.TryAdd(key, currentTaskId))
                        {
                            throw new InvalidOperationException(
                                $"Task #{currentTaskId} acquired a lock using key ${key} but another thread is also still running using this key!");
                        }

                        MyMvc
                        .Pipeline()
                        .ShouldMap(request => request
                           .WithHeaders(new Dictionary<string, string>
                           {
                               ["X-Real-IP"] = $"8.8.{id}.{key}",
                               ["X-Real-LIMIT"] = $"{id}"
                           })
                           .WithMethod(HttpMethod.Get)
                           .WithLocation("api/v1.0/tags")
                           .WithFormFields(new { })
                        )
                        .To<TagsController>(c => c.Tags(new TagsQuery { }))
                        .Which(controller => controller
                            .WithData(StaticTestData.GetTagsWithRateLimitMiddleware(5, "name")))
                        .ShouldReturn()
                        .ActionResult(result => result.Result(new TagsResponseEnvelope
                        {
                            Total = 5,
                            Models = Enumerable
                          .Range(1, 5)
                          .Select(i =>
                          {
                              return new TagResponseModel
                              {
                                  Id = i,
                                  Title = string.Format(CultureInfo.InvariantCulture, "{0}{1}", "name", i),
                              };
                          }).ToList(),
                        }));

                        const int delay = 10;

                        await Task.Delay(delay);

                        if (!runningTasksIndex.TryRemove(key, out var value))
                        {
                            throw new InvalidOperationException($"Task #{currentTaskId} has just finished " +
                                                                $"but the running tasks index does not contain an entry for key {key}");
                        }

                        if (value != currentTaskId)
                        {
                            var ex = new InvalidOperationException($"Task #{currentTaskId} has just finished " +
                                                                   $"but the running threads index has linked task #{value} to key {key}!");

                            throw ex;
                        }

                        Interlocked.Decrement(ref currentParallelism);
                    }
                }
            }
        }
    }
}
#endif