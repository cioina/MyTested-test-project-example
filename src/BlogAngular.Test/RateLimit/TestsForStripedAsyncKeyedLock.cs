namespace BlogAngular.SlowTest.StripedAsyncKeyedLocker;
#if DEBUG

//using Application.Blog.Common;
//using Application.Blog.Tags.Queries.Listing;
using AsyncKeyedLock;
//using Application.Blog.Tags.Commands.Common;
//using FluentAssertions;
//using MyTested.AspNetCore.Mvc;
////using MyTested.AspNetCore.Mvc.Test.Setups;
using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Test.Data;
//using Web.Features;
//using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Adapted from https://raw.githubusercontent.com/amoerie/keyed-semaphores/main/KeyedSemaphores.Tests/TestsForKeyedSemaphore.cs and https://raw.githubusercontent.com/amoerie/keyed-semaphores/main/KeyedSemaphores.Tests/TestsForKeyedSemaphoresCollection.cs
/// </summary>
public class TestsForStripedAsyncKeyedLock
{
    private readonly ITestOutputHelper _output;
    private readonly StripedAsyncKeyedLocker<string> _keyedLocker = new();

    public TestsForStripedAsyncKeyedLock(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    //public class Async : TestsForStripedAsyncKeyedLock
    //{
    //    public Async(ITestOutputHelper output) : base(output) { }

    //    [Theory]
    //    [InlineData(100, 100, 100, 10, 100)]
    //    [InlineData(25, 100, 10, 2, 10)]
    //    [InlineData(103, 100, 50, 5, 50)]
    //    [InlineData(101, 100, 1, 1, 1)]
    //    public async Task ShouldApplyParallelismCorrectly(int id, int numberOfThreads, int numberOfKeys, int minParallelism,
    //        int maxParallelism)
    //    {
    //        Arrange
    //       var runningTasksIndex = new ConcurrentDictionary<int, int>();
    //        var parallelismLock = new object();
    //        var currentParallelism = 0;
    //        var peakParallelism = 0;

    //        var threads = Enumerable.Range(0, numberOfThreads)
    //            .Select(i =>
    //                Task.Run(async () => await OccupyTheLockALittleBit(i % numberOfKeys)))
    //            .ToList();

    //        Act + Assert
    //        await Task.WhenAll(threads);

    //        peakParallelism.Should().BeLessOrEqualTo(maxParallelism);
    //        peakParallelism.Should().BeGreaterOrEqualTo(minParallelism);

    //        _output.WriteLine("Peak parallelism was " + peakParallelism);

    //        async Task OccupyTheLockALittleBit(int key)
    //        {
    //            using (await _keyedLocker.LockAsync(key.ToString()))
    //            {
    //                var incrementedCurrentParallelism = Interlocked.Increment(ref currentParallelism);

    //                lock (parallelismLock)
    //                {
    //                    peakParallelism = Math.Max(incrementedCurrentParallelism, peakParallelism);
    //                }

    //                var currentTaskId = Task.CurrentId ?? -1;

    //                if (!runningTasksIndex.TryAdd(key, currentTaskId))
    //                {
    //                    throw new InvalidOperationException(
    //                        $"Task #{currentTaskId} acquired a lock using key ${key} but another thread is also still running using this key!");
    //                }

    //                MyMvc
    //                .Pipeline()
    //                .ShouldMap(request => request
    //                   .WithHeaders(new Dictionary<string, string>
    //                   {
    //                       ["X-Real-IP"] = $"8.8.{id}.{key}",
    //                       ["X-Real-LIMIT"] = $"{id}"
    //                   })
    //                   .WithMethod(HttpMethod.Get)
    //                   .WithLocation("api/v1.0/tags")
    //                   .WithFormFields(new { })
    //                )
    //                .To<TagsController>(c => c.Tags(new TagsQuery { }))
    //                .Which(controller => controller
    //                    .WithData(StaticTestData.GetTagsWithRateLimitMiddleware(5, "name")))
    //                .ShouldReturn()
    //                .ActionResult(result => result.Result(new TagsResponseEnvelope
    //                {
    //                    Total = 5,
    //                    Models = Enumerable
    //                  .Range(1, 5)
    //                  .Select(i =>
    //                  {
    //                      return new TagResponseModel
    //                      {
    //                          Id = i,
    //                          Title = string.Format(CultureInfo.InvariantCulture, "{0}{1}", "name", i),
    //                      };
    //                  }).ToList(),
    //                }));

    //                const int delay = 10;

    //                await Task.Delay(delay);

    //                if (!runningTasksIndex.TryRemove(key, out var value))
    //                {
    //                    throw new InvalidOperationException($"Task #{currentTaskId} has just finished " +
    //                                                        $"but the running tasks index does not contain an entry for key {key}");
    //                }

    //                if (value != currentTaskId)
    //                {
    //                    var ex = new InvalidOperationException($"Task #{currentTaskId} has just finished " +
    //                                                           $"but the running threads index has linked task #{value} to key {key}!");

    //                    throw ex;
    //                }

    //                Interlocked.Decrement(ref currentParallelism);
    //            }
    //        }
    //    }
    //}

    //public class One : TestsForStripedAsyncKeyedLock
    //{
    //    public One(ITestOutputHelper output) : base(output) { }

    //    [Theory]
    //    [InlineData("ValidMinUserNameLength",
    //     //Must be valid email address
    //     "ValidMinEmailLength@a.bcde",
    //      //Password must contain Upper case, lower case, number, special symbols
    //      "!ValidMinPasswordLength",

    //     "ValidMinNameLength",

    //     "ValidMinTitleLength",
    //     "ValidMinTitleLength",
    //     "ValidMinDescriptionLength", 50, 35, 5, 35)]
    //    public async Task ShouldApplyParallelismCorrectly(
    //        string fullName,
    //        string email,
    //        string password,
    //        string name,
    //        string title,
    //        string slug,
    //        string description,
    //        int numberOfThreads,
    //        int numberOfKeys,
    //        int minParallelism,
    //        int maxParallelism)
    //    {
    //        // Arrange
    //        var runningTasksIndex = new ConcurrentDictionary<int, int>();
    //        var parallelismLock = new object();
    //        var currentParallelism = 0;
    //        var peakParallelism = 0;

    //        var threads = Enumerable.Range(0, numberOfThreads)
    //            .Select(i =>
    //                Task.Run(async () => await OccupyTheLockALittleBit(i % numberOfKeys)))
    //            .ToList();

    //        // Act + Assert
    //        await Task.WhenAll(threads);

    //        peakParallelism.Should().BeLessOrEqualTo(maxParallelism);
    //        peakParallelism.Should().BeGreaterOrEqualTo(minParallelism);

    //        _output.WriteLine("Peak parallelism was " + peakParallelism);

    //        async Task OccupyTheLockALittleBit(int key)
    //        {
    //            using (await _keyedLocker.LockAsync(key.ToString()))
    //            {
    //                var incrementedCurrentParallelism = Interlocked.Increment(ref currentParallelism);

    //                lock (parallelismLock)
    //                {
    //                    peakParallelism = Math.Max(incrementedCurrentParallelism, peakParallelism);
    //                }

    //                var currentTaskId = Task.CurrentId ?? -1;

    //                if (!runningTasksIndex.TryAdd(key, currentTaskId))
    //                {
    //                    throw new InvalidOperationException(
    //                        $"Task #{currentTaskId} acquired a lock using key ${key} but another thread is also still running using this key!");
    //                }

    //                //Test.AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
    //                // () =>
    //                // {
    //                     MyMvc
    //                     .Pipeline()
    //                     .ShouldMap(request => request
    //                        .WithHeaders(new Dictionary<string, string>
    //                        {
    //                            ["X-Real-IP"] = $"26.8.{key}.0",
    //                            ["X-Real-LIMIT"] = "100"
    //                        })
    //                       .WithMethod(HttpMethod.Put)
    //                       .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
    //                       .WithLocation("api/v1.0/tags/edit/2")
    //                       .WithJsonBody(
    //                              string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
    //                                  string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, 4))
    //                       )
    //                     )
    //                     .To<TagsController>(c => c.Edit(2, new()
    //                     {
    //                         TagJson = new()
    //                         {
    //                             Title = string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, 4)
    //                         }
    //                     }))
    //                     .Which(controller => controller
    //                       .WithData(db => db
    //                         .WithEntities(entities => StaticTestData.GetAllWithRateLimitMiddleware(
    //                            count: 3,

    //                            email: email,
    //                            userName: fullName,
    //                            password: password,

    //                            name: name,

    //                            title: title,
    //                            slug: slug,
    //                            description: description,
    //                            date: DateOnly.FromDateTime(DateTime.Today),
    //                            published: false,

    //                            dbContext: entities))))
    //                     .ShouldHave()
    //                     .ActionAttributes(attrs => attrs
    //                          .RestrictingForHttpMethod(HttpMethod.Put)
    //                          .RestrictingForAuthorizedRequests())
    //                     .AndAlso()
    //                     .ShouldReturn()
    //                     .ActionResult(result => result.Result(new TagResponseEnvelope
    //                     {
    //                         TagJson = new()
    //                         {
    //                             Id = 2,
    //                             Title = string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, 4)
    //                         }
    //                     }));
    //                 //}, new Dictionary<string, string[]>
    //                 //{
    //                 //       { "SecurityTokenRefreshException", new[] { "Security token must be refreshed" } },
    //                 //});

    //                const int delay = 10;

    //                await Task.Delay(delay);

    //                if (!runningTasksIndex.TryRemove(key, out var value))
    //                {
    //                    throw new InvalidOperationException($"Task #{currentTaskId} has just finished " +
    //                                                        $"but the running tasks index does not contain an entry for key {key}");
    //                }

    //                if (value != currentTaskId)
    //                {
    //                    var ex = new InvalidOperationException($"Task #{currentTaskId} has just finished " +
    //                                                           $"but the running threads index has linked task #{value} to key {key}!");

    //                    throw ex;
    //                }

    //                Interlocked.Decrement(ref currentParallelism);
    //            }
    //        }
    //    }
    //}

}
#endif