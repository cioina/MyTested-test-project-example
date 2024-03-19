namespace BlogAngular.Test.Data;

using Application.Common;
using Domain.Blog.Models;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MyTested.AspNetCore.Mvc.Internal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
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
        var result = CreateJwtBearer(string.Format("{0}{1}", email, i), Guid.NewGuid().ToString(), DateTime.UtcNow.AddSeconds(1), AdministratorRoleName, "0.0.0.1");
        workerObject.RequestStop();

        return result;
    }

    public static string GetJwtBearerWithIncorrectKey(
        string email,
        int i)
    {
        return CreateJwtBearer(string.Format("{0}{1}", email, i), Guid.NewGuid().ToString(), DateTime.UtcNow.AddMinutes(1), AdministratorRoleName, "0.0.0.1");
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

        return CreateJwtBearer(string.Format("{0}{1}", email, i), secret, DateTime.UtcNow.AddMinutes(expiresInMinutes), role, ipAddress ?? ip);
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

        return string.Format("{0} {1}", JwtBearerDefaults.AuthenticationScheme, token);
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
                         string.Format("{0}{1}", email, i),
                         string.Format("{0}{1}", userName, i)
                     );

                userManager.AddPasswordAsync(user, string.Format("{0}{1}", password, i));

                return user;

            }).ToList();
    }

    public static IEnumerable<object> GetTags(
    int count,
    string name)
    => Enumerable
         .Range(1, count)
         .Select(i =>
         {
             return new Tag(
                 string.Format("{0}{1}", name, i)
             );
         }).ToList();

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
                     string.Format("{0}{1}", title, i),
                     string.Format("{0}{1}", slug, i),
                     string.Format("{0}{1}", description, i),
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
                          string.Format("{0}{1}", email, i),
                          string.Format("{0}{1}", userName, i)
                      );

                 userManager.AddPasswordAsync(user!, string.Format("{0}{1}", password, i));

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
             var user = await userManager.FindByEmailAsync(string.Format("{0}{1}", email, i));
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
                     string.Format("{0}{1}", title, i),
                     string.Format("{0}{1}", slug, i),
                     string.Format("{0}{1}", description, i),
                     date.ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
                     published

                 );
             }).ToList());
        dbContext.AddRange(GetTags(count, name));

        dbContext.SaveChanges();
    }

}

