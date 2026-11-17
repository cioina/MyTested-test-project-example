using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using BlogAngular.Application.Common;
using BlogAngular.Domain.Blog.Models;
using BlogAngular.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MyTested.AspNetCore.Mvc.Internal.Services;
using static BlogAngular.Domain.Common.Models.ModelConstants.Identity;

namespace BlogAngular.Test.Data
{
    public static class StaticTestData
    {
        private class MiddlewareResult
        {
            public const int NO_RESULT = 0;
            public const int GOOD_RESULT = 1;
            public const int RATE_LIMIT_MIDDLEWARE_EXCEPTION = -1;
            public const int SECURITY_TOKEN_REFRESH_EXCEPTION = -2;
            public const int MATCHING_RULES_EXCEPTION = -3;
        }
        private class Worker
        {
            private volatile bool _shouldStop;
            public EventWaitHandle? WaitHandleExternal;

            public void DoWork()
            {
                while (!_shouldStop)
                {
                    Thread.Sleep(2000);
                    _ = WaitHandleExternal!.Set();
                }
            }
            public void RequestStop() => _shouldStop = true;
        }

        public static async Task<int> InvokeIpRateLimitMiddleware()
        {
            var result = MiddlewareResult.NO_RESULT;

            if (TestServiceProvider.Current.GetService(typeof(IMiddlewareFactory)) is IMiddlewareFactory middlewareFactory)
            {
                var middleware = middlewareFactory.Create(typeof(IpRateLimitMiddleware));
                if (middleware != null)
                {
                    try
                    {
                        await Task.Run(
                              async () =>
                         {
                             var httpContext = TestServiceProvider.Current.GetService<IHttpContextAccessor>()!.HttpContext!;
                             var ipPolicyStore = TestServiceProvider.Current.GetService<IIpPolicyStore>();

                             var policy = await ipPolicyStore!.GetAsync("ippp", httpContext.RequestAborted).ConfigureAwait(false);
                             if (policy == null)
                             {
                                 await ipPolicyStore!.SeedAsync().ConfigureAwait(false);
                                 policy = await ipPolicyStore!.GetAsync("ippp", httpContext.RequestAborted).ConfigureAwait(false);
                             }

                             if (httpContext.Request.Headers.TryGetValue("X-Real-IP", out var ip))
                             {
                                 if (httpContext.Request.Headers.TryGetValue("X-Real-LIMIT", out var limit))
                                 {
                                     if (policy.IpRules.TryAdd(ip!, new IpRateLimitPolicy
                                     {
                                         Ip = ip!,
                                         Rules = [.. new RateLimitRule[] {
                                               new() {
                                                   Endpoint = $"*:{httpContext.Request.Path}",
                                                   Limit = int.Parse(limit!, CultureInfo.InvariantCulture),
                                                   Period = "5m" }}]
                                     }))
                                     {
                                         await ipPolicyStore!.SetAsync("ippp", policy!, cancellationToken: httpContext.RequestAborted).ConfigureAwait(false);
                                     }
                                 }
                             }

                             await middleware.InvokeAsync(httpContext, TestServiceProvider.Current.GetService<RequestDelegate>()!);
                             result = MiddlewareResult.GOOD_RESULT;
                         });
                    }
                    catch (Exception exception)
                    {
                        switch (exception)
                        {
                            case RateLimitMiddlewareException:
                                result = MiddlewareResult.RATE_LIMIT_MIDDLEWARE_EXCEPTION;
                                break;
                            case SecurityTokenRefreshException:
                                result = MiddlewareResult.SECURITY_TOKEN_REFRESH_EXCEPTION;
                                break;
                            case MatchingRulesException:
                                result = MiddlewareResult.MATCHING_RULES_EXCEPTION;
                                break;
                        }
                    }
                    finally
                    {
                        middlewareFactory.Release(middleware);
                    }
                }
            }

            return result;
        }

        public static void ThrowExceptionByCode(int code)
        {
            switch (code)
            {
                case MiddlewareResult.RATE_LIMIT_MIDDLEWARE_EXCEPTION:
                    throw new MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException(new Dictionary<string, string[]>
                    {
                        { "RateLimitMiddlewareException", ["Too many requests"] }
                    });

                case MiddlewareResult.SECURITY_TOKEN_REFRESH_EXCEPTION:
                    throw new MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException(new Dictionary<string, string[]>
                    {
                        { "SecurityTokenRefreshException", ["Security token must be refreshed"] }
                    });
                case MiddlewareResult.MATCHING_RULES_EXCEPTION:
                    throw new MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException(new Dictionary<string, string[]>
                    {
                        { "MatchingRulesException", ["Matching Rules Exception"] }
                    });

                default:
                    if (code != MiddlewareResult.GOOD_RESULT)
                    {
                        throw new MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException(new Dictionary<string, string[]>
                    {
                        { "NoGoodResult", ["Unknown result"] }
                    });
                    }
                    break;
            }

        }
        public static string GetJwtBearerWithExpiredToken(
        string email,
        int i)
        {
            EventWaitHandle waitHandle = new AutoResetEvent(false);
            Worker workerObject = new()
            {
                WaitHandleExternal = waitHandle
            };
            Thread workerThread = new(workerObject.DoWork);
            workerThread.Start();
            _ = waitHandle.WaitOne();
            var result = CreateJwtBearer($"{email}{i}", Guid.NewGuid().ToString(), DateTime.UtcNow.AddSeconds(1), AdministratorRoleName, "0.0.0.1");
            workerObject.RequestStop();

            return result;
        }

        public static string GetJwtBearerWithIncorrectKey(
            string email,
            int i) => CreateJwtBearer($"{email}{i}", Guid.NewGuid().ToString(), DateTime.UtcNow.AddMinutes(1), AdministratorRoleName, "0.0.0.1");

        public static string GetJwtBearerAdministratorRole(
            string email,
            int i) => GetJwtBearerWithRole(email, i, AdministratorRoleName, null);

        public static string GetJwtBearerWithRole(
            string email,
            int i,
            string role,
            string? ipAddress) => GetJwtBearerWithRoleAndExpires(email, i, role, ipAddress, null);

        public static string GetJwtBearerWithRoleAndExpires(
        string email,
        int i,
        string role,
        string? ipAddress,
        DateTime? expires)
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

            return CreateJwtBearer($"{email}{i}", secret, expires ?? DateTime.UtcNow.AddMinutes(expiresInMinutes), role, ipAddress ?? ip);
        }

        private static string CreateJwtBearer(
        string email,
        string secret,
        DateTime expires,
        string role,
        string ipAddress)
        {
            var key = Encoding.ASCII.GetBytes(secret.PadRight(256 / 8, '\0'));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, email),
                ]),
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            if (!string.IsNullOrEmpty(role))
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

            return $"{JwtBearerDefaults.AuthenticationScheme} {token}";
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
                             $"{email}{i}",
                             $"{userName}{i}"
                         );

                    _ = userManager.AddPasswordAsync(user, $"{password}{i}");

                    return user;

                }).ToList();
        }

        public static IEnumerable<object> GetTags(
        int count,
        string name) => Enumerable
                 .Range(1, count)
                 .Select(i => new Tag(
                         $"{name}{i}"
                     )).ToList();

        public static IEnumerable<object> GetTagsWithRateLimitMiddleware(
        int count,
        string name)
        {
            var ipRateLimit = InvokeIpRateLimitMiddleware();
            ThrowExceptionByCode(ipRateLimit.Result);

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
                 .Select(i => new Article(
                         $"{title}{i}",
                         $"{slug}{i}",
                         $"{description}{i}",
                         date.ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
                         published

                     )).ToList();

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
                              $"{email}{i}",
                              $"{userName}{i}"
                          );

                     _ = userManager.AddPasswordAsync(user!, $"{password}{i}");

                     return user;

                 }).ToList();

            dbContext.AddRange(users);
            _ = dbContext.SaveChanges();

            var roleManager = TestServiceProvider.Current.GetService<RoleManager<IdentityRole>>()!;
            var adminRoleExists = roleManager.RoleExistsAsync(AdministratorRoleName);
            if (!adminRoleExists.Result && roleManager.SupportsRoleClaims)
            {
                var adminRole = new IdentityRole(AdministratorRoleName);
                _ = roleManager.CreateAsync(adminRole);
            }

            return Enumerable
             .Range(1, count)
             .Select(async i =>
             {
                 var user = await userManager.FindByEmailAsync($"{email}{i}");
                 _ = await userManager.AddToRoleAsync(user!, AdministratorRoleName);

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
            _ = GetUsersWithRole(count, email, userName, password, dbContext);

            dbContext.AddRange(Enumerable
                 .Range(1, count)
                 .Select(i => new Article(
                         $"{title}{i}",
                         $"{slug}{i}",
                         $"{description}{i}",
                         date.ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
                         published

                     )).ToList());
            dbContext.AddRange(GetTags(count, name));

            _ = dbContext.SaveChanges();
        }

        public static void GetAllWithRateLimitMiddleware(
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
            ThrowExceptionByCode(ipRateLimit.Result);

            GetArticlesTagsUsersWithRole(count, email, userName, password, name, title, slug, description, date, published, dbContext);
        }
    }
}
