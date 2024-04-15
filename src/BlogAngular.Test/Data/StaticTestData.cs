namespace BlogAngular.Test.Data;
#if DEBUG

using Application.Common;
using AspNetCoreRateLimit;
using Domain.Blog.Models;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MyTested.AspNetCore.Mvc.Internal.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Domain.Common.Models.ModelConstants.Identity;

public static class StaticTestData
{
    class Worker
    {
        private volatile bool _shouldStop;
        public EventWaitHandle? WaitHandleExternal;

        public void DoWork()
        {
            while (!_shouldStop)
            {
                Thread.Sleep(2000);
                WaitHandleExternal!.Set();
            }
        }
        public void RequestStop()
        {
            _shouldStop = true;
        }
    }

    public static async Task<bool> InvokeIpRateLimitMiddleware()
    {
        bool result = true;

        if (TestServiceProvider.Current.GetService(typeof(IMiddlewareFactory)) is IMiddlewareFactory middlewareFactory)
        {
            var middleware = middlewareFactory.Create(typeof(IpRateLimitMiddleware));
            if (middleware != null)
            {
                var runningTasksIndex = new ConcurrentDictionary<string, string>();
                try
                {
                    await Task.Run(
                          async () =>
                     {
                         var httpContext = TestServiceProvider.Current.GetService<IHttpContextAccessor>()!.HttpContext!;
                         var ipPolicyStore = TestServiceProvider.Current.GetService<IIpPolicyStore>();

                         var ipps = await ipPolicyStore!.GetAsync("ippp").ConfigureAwait(false);
                         if (ipps == null)
                         {
                             if (runningTasksIndex.TryAdd("ippp", "ippp"))
                             {
                                 await ipPolicyStore!.SeedAsync().ConfigureAwait(false);
                             }
                         }
                         var policy = await ipPolicyStore!.GetAsync("ippp", httpContext.RequestAborted).ConfigureAwait(false);

                         var flag = true;
                         if (httpContext.Request.Headers.TryGetValue("X-Real-IP", out var ip))
                         {
                             if (httpContext.Request.Headers.TryGetValue("X-Real-LIMIT", out var limit))
                             {
                                 flag = policy.IpRules.Any((a) => a.Ip == ip);
                                 if (!flag)
                                 {
                                     if (runningTasksIndex.TryAdd(ip!, limit!))
                                     {
                                         policy.IpRules.Add(new IpRateLimitPolicy
                                         {
                                             Ip = ip,
                                             Rules = new List<RateLimitRule>(new RateLimitRule[] {
                                               new() {
                                                   Endpoint = $"*:{httpContext.Request.Path}",
                                                   Limit = int.Parse(limit!),
                                                   Period = "1m" }})
                                         });

                                     }
                                 }
                             }
                         }

                         if (!flag)
                         {
                             await ipPolicyStore!.SetAsync("ippp", policy!, cancellationToken: httpContext.RequestAborted).ConfigureAwait(false);
                         }

                         var middle = middleware.InvokeAsync(httpContext, TestServiceProvider.Current.GetService<RequestDelegate>()!);
                         if (middle.IsFaulted || middle.IsCanceled)
                         {
                             result = false;
                         }

                     });

                }
                finally
                {
                    middlewareFactory.Release(middleware);
                }
            }

        }
        return result;
    }

    public static string GetJwtBearerWithExpiredToken(
    string email,
    int i)
    {
        EventWaitHandle _waitHandle = new AutoResetEvent(false); // is signaled value change to true
                                                                 // start a thread which will after a small time set an event
        Worker workerObject = new()
        {
            WaitHandleExternal = _waitHandle
        };
        Thread workerThread = new(workerObject.DoWork);
        workerThread.Start();
        _waitHandle.WaitOne();
        var result = CreateJwtBearer(string.Format(CultureInfo.InvariantCulture, "{0}{1}", email, i), Guid.NewGuid().ToString(), DateTime.UtcNow.AddSeconds(1), AdministratorRoleName, "0.0.0.1");
        workerObject.RequestStop();

        return result;
    }

    public static string GetJwtBearerWithIncorrectKey(
        string email,
        int i)
    {
        return CreateJwtBearer(string.Format(CultureInfo.InvariantCulture, "{0}{1}", email, i), Guid.NewGuid().ToString(), DateTime.UtcNow.AddMinutes(1), AdministratorRoleName, "0.0.0.1");
    }

    public static string GetJwtBearerAdministratorRole(
        string email,
        int i)
    {
        return GetJwtBearerWithRole(email, i, AdministratorRoleName, null);
    }

    public static string GetJwtBearerWithRole(
        string email,
        int i,
        string role,
        string? ipAddress)
    {
        var configuration = TestServiceProvider.Current.GetService<IConfiguration>();

        var secret = configuration!
            .GetSection(nameof(ApplicationSettings))
            .GetValue<string>(nameof(ApplicationSettings.SecurityTokenDescriptorKey))!;
        var expiresInMinutes = configuration!
            .GetSection(nameof(ApplicationSettings))
            .GetValue<double>(nameof(ApplicationSettings.SecurityTokenDescriptorExpiresInMinutes))!;
        var ip = configuration!
            .GetSection(nameof(ApplicationSettings))
            .GetValue<string>(nameof(ApplicationSettings.ExperimentalIpAddress))!;

        return CreateJwtBearer(string.Format(CultureInfo.InvariantCulture, "{0}{1}", email, i), secret, DateTime.UtcNow.AddMinutes(expiresInMinutes), role, ipAddress ?? ip);
    }

    private static string CreateJwtBearer(
    string email,
    string secret,
    DateTime expires,
    string role,
    string ipAddress)
    {
        var key = Encoding.ASCII.GetBytes(secret.PadRight((256 / 8), '\0'));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, email),
            }),
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        if (!role.IsNullOrEmpty())
        {
            tokenDescriptor.Subject.AddClaim(
                new Claim(ClaimTypes.Role, role)
            );
        }

        tokenDescriptor.Subject.AddClaim(
            new Claim(ClaimTypes.UserData, ipAddress)
        );

        JsonWebTokenHandler jwtSecurityTokenHandler = new();
        var token = jwtSecurityTokenHandler.CreateToken(tokenDescriptor);

        return string.Format(CultureInfo.InvariantCulture, "{0} {1}", JwtBearerDefaults.AuthenticationScheme, token);
    }

    public static IEnumerable<object> GetUsers(
        int count,
        string email,
        string userName,
        string password)
    {
        var userManager = TestServiceProvider.Current.GetService<UserManager<User>>()!;

        return Enumerable
            .Range(1, count)
            .Select(i =>
            {
                var user = new User(
                         string.Format(CultureInfo.InvariantCulture, "{0}{1}", email, i),
                         string.Format(CultureInfo.InvariantCulture, "{0}{1}", userName, i)
                     );

                userManager.AddPasswordAsync(user, string.Format(CultureInfo.InvariantCulture, "{0}{1}", password, i));

                return user;

            }).ToList();
    }

    public static IEnumerable<object> GetTags(
    int count,
    string name)
    {
        return Enumerable
             .Range(1, count)
             .Select(i =>
             {
                 return new Tag(
                     string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, i)
                 );
             }).ToList();
    }

    public static IEnumerable<object> GetTagsWithRateLimitMiddleware(
    int count,
    string name)
    {
        var ipRateLimit = InvokeIpRateLimitMiddleware();

        if (!ipRateLimit.Result)
        {
            throw new MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException(new Dictionary<string, string[]>
                    {
                        { "RequestBlockedBehaviorAsync", new []{"To many requests" } }
                    });
        }

        return GetTags(count, name);
    }

    public static IEnumerable<object> GetArticlesTagsUsers(
    int count,

    string email,
    string userName,
    string password,

    string name,

    string title,
    string slug,
    string description,
    DateOnly date,
    bool published)
    {
        var articles =
        Enumerable
             .Range(1, count)
             .Select(i =>
             {
                 return new Article(
                     string.Format(CultureInfo.InvariantCulture, "{0}{1}", title, i),
                     string.Format(CultureInfo.InvariantCulture, "{0}{1}", slug, i),
                     string.Format(CultureInfo.InvariantCulture, "{0}{1}", description, i),
                     date.ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
                     published

                 );
             }).ToList();

        var result = new List<object>();
        result.AddRange(GetUsers(count, email, userName, password));
        result.AddRange(articles);
        result.AddRange(GetTags(count, name));

        return result;
    }

    public static IEnumerable<object> GetUsersWithRole(
    int count,
    string email,
    string userName,
    string password,
    DbContext dbContext)
    {
        var userManager = TestServiceProvider.Current.GetService<UserManager<User>>()!;

        var users = Enumerable
             .Range(1, count)
             .Select(i =>
             {
                 var user = new User(
                          string.Format(CultureInfo.InvariantCulture, "{0}{1}", email, i),
                          string.Format(CultureInfo.InvariantCulture, "{0}{1}", userName, i)
                      );

                 userManager.AddPasswordAsync(user!, string.Format(CultureInfo.InvariantCulture, "{0}{1}", password, i));

                 return user;

             }).ToList();

        dbContext.AddRange(users);
        dbContext.SaveChanges();

        var roleManager = TestServiceProvider.Current.GetService<RoleManager<IdentityRole>>()!;
        var adminRoleExists = roleManager.RoleExistsAsync(AdministratorRoleName);
        if (!adminRoleExists.Result && roleManager.SupportsRoleClaims)
        {
            var adminRole = new IdentityRole(AdministratorRoleName);
            roleManager.CreateAsync(adminRole);
        }

        return Enumerable
         .Range(1, count)
         .Select(async i =>
         {
             var user = await userManager.FindByEmailAsync(string.Format(CultureInfo.InvariantCulture, "{0}{1}", email, i));
             await userManager.AddToRoleAsync(user!, AdministratorRoleName);

             return user;
         }).ToList();
    }

    public static void GetArticlesTagsUsersWithRole(
    int count,

    string email,
    string userName,
    string password,

    string name,

    string title,
    string slug,
    string description,
    DateOnly date,
    bool published,
    DbContext dbContext)
    {
        GetUsersWithRole(count, email, userName, password, dbContext);

        dbContext.AddRange(Enumerable
             .Range(1, count)
             .Select(i =>
             {
                 return new Article(
                     string.Format(CultureInfo.InvariantCulture, "{0}{1}", title, i),
                     string.Format(CultureInfo.InvariantCulture, "{0}{1}", slug, i),
                     string.Format(CultureInfo.InvariantCulture, "{0}{1}", description, i),
                     date.ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
                     published

                 );
             }).ToList());
        dbContext.AddRange(GetTags(count, name));

        dbContext.SaveChanges();
    }

    public static void GetAllWithRoleWithRateLimitMiddleware(
    int count,
    
    string email,
    string userName,
    string password,
    
    string name,
    
    string title,
    string slug,
    string description,
    DateOnly date,
    bool published,
    DbContext dbContext)
    {
        var ipRateLimit = InvokeIpRateLimitMiddleware();

        if (!ipRateLimit.Result)
        {
            throw new MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException(new Dictionary<string, string[]>
                    {
                        { "RequestBlockedBehaviorAsync", new []{"To many requests" } }
                    });
        }

        GetArticlesTagsUsersWithRole(count, email, userName, password, name, title, slug, description, date, published, dbContext);
    }
}
#endif
