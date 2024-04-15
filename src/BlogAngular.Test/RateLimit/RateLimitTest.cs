namespace BlogAngular.Test.RateLimit;
#if DEBUG

using Application.Blog.Common;
using Application.Blog.Tags.Commands.Common;
using Application.Blog.Tags.Queries.Listing;
using MyTested.AspNetCore.Mvc;
using MyTested.AspNetCore.Mvc.Test.Setups;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Test.Data;
using Web.Features;
using Xunit;
using static Domain.Common.Models.ModelConstants.Article;
using static Domain.Common.Models.ModelConstants.Identity;
using static Domain.Common.Models.ModelConstants.Tag;

public class RateLimitRouteTest
{
    private static readonly string ValidMinUserNameLength = new('t', MinUserNameLength);
    private static readonly string ValidMaxUserNameLength = new('t', MaxUserNameLength - 1);

    private static readonly string ValidMinEmailLength = new('t', MinEmailLength);
    private static readonly string ValidMaxEmailLength = new('t', MaxEmailLength - 8);

    private static readonly string ValidMinPasswordLength = new('t', MinPasswordLength - 3);
    private static readonly string ValidMaxPasswordLength = new('t', MaxPasswordLength - 3);

    private static readonly string ValidMinNameLength = new('t', MinNameLength);
    private static readonly string ValidMaxxNameLength = new('t', MaxNameLength - 1);

    private static readonly string ValidMinTitleLength = new('t', MinTitleLength);
    private static readonly string ValidMaxxTitleLength = new('t', MaxTitleLength - 1);
    private static readonly string ValidMinDescriptionLength = new('t', MinDescriptionLength);
    private static readonly string ValidMaxxDescriptionLength = new('t', MaxDescriptionLength - 1);

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Edit_tag_with_client_whitelist_should_return_success_with_data(
     string fullName,
     string email,
     string password,
     string name,
     string title,
     string slug,
     string description)
     => MyMvc
      .Pipeline()
      .ShouldMap(request => request
         .WithHeaders(new Dictionary<string, string>
         {
             ["X-Real-IP"] = "9.8.8.0",
             ["X-Real-LIMIT"] = "0"
         })
        .WithMethod(HttpMethod.Put)
        .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole("ClientWhitelist@email.com", 1))
        .WithLocation("api/v1.0/tags/edit/2")
        .WithJsonBody(
               string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
                   string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, 4))
        )
      )
      .To<TagsController>(c => c.Edit(2, new()
      {
          TagJson = new()
          {
              Title = string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, 4)
          }
      }))
      .Which(controller => controller
        .WithData(db => db
          .WithEntities(entities => StaticTestData.GetAllWithRoleWithRateLimitMiddleware(
             count: 3,

             email: email,
             userName: fullName,
             password: password,

             name: name,

             title: title,
             slug: slug,
             description: description,
             date: DateOnly.FromDateTime(DateTime.Today),
             published: false,

             dbContext: entities))))
      .ShouldHave()
      .ActionAttributes(attrs => attrs
           .RestrictingForHttpMethod(HttpMethod.Put)
           .RestrictingForAuthorizedRequests())
      .AndAlso()
      .ShouldReturn()
      .ActionResult(result => result.Result(new TagResponseEnvelope
      {
          TagJson = new()
          {
              Id = 2,
              Title = string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, 4)
          }
      }))
      .AndAlso()
      .ShouldPassForThe<ActionAttributes>(attributes =>
      {
          Assert.Equal(5, attributes.Count());
      });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Edit_tag_with_ip_whitelist_should_return_success_with_data(
     string fullName,
     string email,
     string password,
     string name,
     string title,
     string slug,
     string description)
     => MyMvc
      .Pipeline()
      .ShouldMap(request => request
         .WithHeaders(new Dictionary<string, string>
         {
             ["X-Real-IP"] = "192.168.0.0",
             ["X-Real-LIMIT"] = "0"
         })
        .WithMethod(HttpMethod.Put)
        .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
        .WithLocation("api/v1.0/tags/edit/2")
        .WithJsonBody(
               string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
                   string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, 4))
        )
      )
      .To<TagsController>(c => c.Edit(2, new()
      {
          TagJson = new()
          {
              Title = string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, 4)
          }
      }))
      .Which(controller => controller
        .WithData(db => db
          .WithEntities(entities => StaticTestData.GetAllWithRoleWithRateLimitMiddleware(
             count: 3,

             email: email,
             userName: fullName,
             password: password,

             name: name,

             title: title,
             slug: slug,
             description: description,
             date: DateOnly.FromDateTime(DateTime.Today),
             published: false,

             dbContext: entities))))
      .ShouldHave()
      .ActionAttributes(attrs => attrs
           .RestrictingForHttpMethod(HttpMethod.Put)
           .RestrictingForAuthorizedRequests())
      .AndAlso()
      .ShouldReturn()
      .ActionResult(result => result.Result(new TagResponseEnvelope
      {
          TagJson = new()
          {
              Id = 2,
              Title = string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, 4)
          }
      }))
      .AndAlso()
      .ShouldPassForThe<ActionAttributes>(attributes =>
      {
          Assert.Equal(5, attributes.Count());
      });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Listing_tags_with_rate_limit_zero_should_fail(
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
     string fullName,
     string email,
     string password,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
     string name,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
     string title,
     string slug,
     string description
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
     )
    => Test.AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
    () =>
    {
        MyMvc
        .Pipeline()
        .ShouldMap(request => request
           .WithHeaders(new Dictionary<string, string>
           {
               ["X-Real-IP"] = "10.8.8.0",
               ["X-Real-LIMIT"] = "0"
           })
           .WithMethod(HttpMethod.Get)
           .WithLocation("api/v1.0/tags")
           .WithFormFields(new { })
        )
        .To<TagsController>(c => c.Tags(new TagsQuery { }))
        .Which(controller => controller
            .WithData(StaticTestData.GetTagsWithRateLimitMiddleware(5, name)))
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
                  Title = string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, i),
              };
          }).ToList(),
        }));
    }, new Dictionary<string, string[]>
    {
        { "RequestBlockedBehaviorAsync", new[] { "Too many requests" } },
    });


    public static IEnumerable<object[]> ValidData()
    {
        yield return new object[]
        {
            ValidMinUserNameLength,
            //Must be valid email address
            $"{ValidMinEmailLength}@a.bcde",
            //Password must contain Upper case, lower case, number, special symbols
            string.Format(CultureInfo.InvariantCulture, "U!{0}",  ValidMinPasswordLength),

            ValidMinNameLength,

            ValidMinTitleLength,
            ValidMinTitleLength,
            ValidMinDescriptionLength,
        };

        yield return new object[]
        {
            ValidMaxUserNameLength,
            //Must be valid email address
            $"{ValidMaxEmailLength}@a.bcde",
            //Password must contain Upper case, lower case, number, special symbols
            string.Format(CultureInfo.InvariantCulture, "U!{0}", ValidMaxPasswordLength),

            ValidMaxxNameLength,

            ValidMaxxTitleLength,
            ValidMaxxTitleLength,
            ValidMaxxDescriptionLength,
        };
    }
}

#endif