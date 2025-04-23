using BlogAngular.Application.Blog.Common;
using BlogAngular.Application.Blog.Tags.Commands.Common;
using BlogAngular.Application.Blog.Tags.Queries.Listing;
using BlogAngular.Application.Identity.Commands.Common;
using BlogAngular.Application.Identity.Commands.Login;
using BlogAngular.Application.Identity.Commands.Update;
using BlogAngular.Test.Data;
using BlogAngular.Web.Features;
using MyTested.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static BlogAngular.Domain.Common.Models.ModelConstants.Article;
using static BlogAngular.Domain.Common.Models.ModelConstants.Identity;
using static BlogAngular.Domain.Common.Models.ModelConstants.Tag;
using static MyTested.AspNetCore.Mvc.Test.Setups.Test;

namespace BlogAngular.Test.RateLimit
{
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
        public void Login_user_with_whitelisted_public_route_and_zero_limit_should_return_success_with_token(
         string fullName,
         string email,
         string password,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
         string name,
         string title,
         string slug,
         string description
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
            )
         => MyMvc
          .Pipeline()
          .ShouldMap(request => request
             .WithHeaders(new Dictionary<string, string>
             {
                 ["X-Real-IP"] = "9.8.8.0",
                 ["X-Real-LIMIT"] = "0"
             })
            .WithMethod(HttpMethod.Post)
            .WithLocation("api/v1.0/identity/login")
            .WithJsonBody(
                string.Format(@"{{""user"":{{""email"":""{0}"",""password"":""{1}""}}}}",
                    $"{email}1",
                    $"{password}1"
                ))
          )
          .To<IdentityController>(c => c.Login(new UserLoginCommand
          {
              UserJson = new()
              {
                  Email = $"{email}1",
                  Password = $"{password}1"
              }
          }))
          .Which(controller => controller
            .WithData(db => db
              .WithEntities(entities => StaticTestData.GetAllWithRateLimitMiddleware(
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
          .ShouldReturn()
          .ActionResult(result => result.Result(new UserResponseEnvelope
          {
              UserJson = new()
              {
                  Email = $"{email}1",
                  UserName = $"{fullName}1",
                  Token = $"Token: {email}1"
              }
          }))
          .AndAlso()
          .ShouldPassForThe<ActionAttributes>(attributes =>
          {
              Assert.Equal(4, attributes.Count());
          });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Edit_tag_with_whitelisted_client_id_and_zero_limit_on_private_route_should_return_success_with_data(
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
                 ["X-Real-IP"] = "10.8.8.0",
                 ["X-Real-LIMIT"] = "0"
             })
            .WithMethod(HttpMethod.Put)
            .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole("ClientWhitelist@email.com", 1))
            .WithLocation("api/v1.0/tags/edit/2")
            .WithJsonBody(
                   string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
                       $"{name}4")
            )
          )
          .To<TagsController>(c => c.Edit(2, new()
          {
              TagJson = new()
              {
                  Title = $"{name}4"
              }
          }))
          .Which(controller => controller
            .WithData(db => db
              .WithEntities(entities => StaticTestData.GetAllWithRateLimitMiddleware(
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
                  Title = $"{name}4"
              }
          }))
          .AndAlso()
          .ShouldPassForThe<ActionAttributes>(attributes =>
          {
              Assert.Equal(5, attributes.Count());
          });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Listing_tags_with_whitelisted_client_id_and_zero_limit_on_public_route_should_fail(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description
         )
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
            .Pipeline()
            .ShouldMap(request => request
               .WithHeaders(new Dictionary<string, string>
               {
                   ["X-Real-IP"] = "1.8.8.0",
                   ["X-Real-LIMIT"] = "0"
               })
              .WithMethod(HttpMethod.Get)
              .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole("ClientWhitelist@email.com", 1))
                 .WithLocation("api/v1.0/tags")
                 .WithFormFields(new { })
              )
            .To<TagsController>(c => c.Tags(new TagsQuery { }))
            .Which(controller => controller
              .WithData(db => db
                .WithEntities(entities => StaticTestData.GetAllWithRateLimitMiddleware(
                   count: 5,

                   email: email,
                   userName: fullName,
                   password: password,

                   name: name,

                   title: title,
                   slug: slug,
                   description: description,
                   date: DateOnly.FromDateTime(DateTime.Today),
                   published: false,

                   dbContext: entities))));
        }, new Dictionary<string, string[]>
        {
            { "RateLimitMiddlewareException", new[] { "Too many requests" } },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Edit_tag_with_whitelisted_ip_address_and_zero_limit_should_return_success_with_data(
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
                       $"{name}4")
            )
          )
          .To<TagsController>(c => c.Edit(2, new()
          {
              TagJson = new()
              {
                  Title = $"{name}4"
              }
          }))
          .Which(controller => controller
            .WithData(db => db
              .WithEntities(entities => StaticTestData.GetAllWithRateLimitMiddleware(
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
                  Title = $"{name}4"
              }
          }))
          .AndAlso()
          .ShouldPassForThe<ActionAttributes>(attributes =>
          {
              Assert.Equal(5, attributes.Count());
          });

        [Theory]
        [InlineData("ValidMinUserNameLength",
         //Must be valid email address
         "SecurityTokenRefreshException@email.com",
          //Password must contain Upper case, lower case, number, special symbols
          "!ValidMinPasswordLength",

         "ValidMinNameLength",

         "ValidMinTitleLength",
         "ValidMinTitleLength",
         "ValidMinDescriptionLength")]
        public void Edit_tag_with_refresh_token_should_fail(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description
         )
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
            .Pipeline()
            .ShouldMap(request => request
               .WithHeaders(new Dictionary<string, string>
               {
                   ["X-Real-IP"] = "15.8.8.0",
                   ["X-Real-LIMIT"] = "0"
               })
              .WithMethod(HttpMethod.Put)
              .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
              .WithLocation("api/v1.0/tags/edit/2")
              .WithJsonBody(
                     string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
                         $"{name}4")
              )
            )
            .To<TagsController>(c => c.Edit(2, new()
            {
                TagJson = new()
                {
                    Title = $"{name}4"
                }
            }))
            .Which(controller => controller
              .WithData(db => db
                .WithEntities(entities => StaticTestData.GetAllWithRateLimitMiddleware(
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

                   dbContext: entities))));
        }, new Dictionary<string, string[]>
        {
            { "SecurityTokenRefreshException", new[] { "Security token must be refreshed" } },
        });


        [Theory]
        [InlineData("ValidMinUserNameLength",
         //Must be valid email address
         "SecurityTokenRefreshException@email.com",
          //Password must contain Upper case, lower case, number, special symbols
          "!ValidMinPasswordLength",

         "ValidMinNameLength",

         "ValidMinTitleLength",
         "ValidMinTitleLength",
         "ValidMinDescriptionLength")]
        public void Login_with_password_with_refresh_token_and_whitelisted_private_route_should_return_success_with_token(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description
         ) => MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithHeaders(new Dictionary<string, string>
                {
                    ["X-Real-IP"] = "14.8.8.0",
                    ["X-Real-LIMIT"] = "0"
                })
                .WithMethod(HttpMethod.Post)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/identity")
                .WithJsonBody(
                     string.Format(@"{{""user"":{{""password"":""{0}""}}}}",
                         $"{password}1"
                     )
                )
             )
             .To<IdentityController>(c => c.LoginPassword(new LoginPasswordCommand
             {
                 UserJson = new()
                 {
                     FullName = null,
                     Password = $"{password}1"
                 }
             }))
            .Which(controller => controller
              .WithData(db => db
                .WithEntities(entities => StaticTestData.GetAllWithRateLimitMiddleware(
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
                 .RestrictingForHttpMethod(HttpMethod.Post)
                 .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn()
             .ActionResult(result => result.Result(new UserResponseEnvelope
             {
                 UserJson = new()
                 {
                     Email = $"{email}1",
                     UserName = $"{fullName}1",
                     Token = $"Token: {email}1"
                 }
             }))
             .AndAlso()
             .ShouldPassForThe<ActionAttributes>(attributes =>
             {
                 Assert.Equal(4, attributes.Count());
             });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Listing_tags_with_middleware_and_zero_rate_limit_should_fail(
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
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
            .Pipeline()
            .ShouldMap(request => request
               .WithHeaders(new Dictionary<string, string>
               {
                   ["X-Real-IP"] = "11.8.8.0",
                   ["X-Real-LIMIT"] = "0"
               })
               .WithMethod(HttpMethod.Get)
               .WithLocation("api/v1.0/tags")
               .WithFormFields(new { })
            )
            .To<TagsController>(c => c.Tags(new TagsQuery { }))
            .Which(controller => controller
                .WithData(StaticTestData.GetTagsWithRateLimitMiddleware(5, name)));
        }, new Dictionary<string, string[]>
        {
            { "RateLimitMiddlewareException", new[] { "Too many requests" } },
        });


        [Theory]
        [MemberData(nameof(ValidData))]
        public void Listing_tags_without_middleware_and_zero_rate_limit_should_return_success_with_all_tags(
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
         => MyMvc
            .Pipeline()
            .ShouldMap(request => request
                .WithHeaders(new Dictionary<string, string>
                {
                    ["X-Real-IP"] = "20.8.8.0",
                    ["X-Real-LIMIT"] = "0"
                })
               .WithMethod(HttpMethod.Get)
               .WithLocation("api/v1.0/tags")
               .WithFormFields(new { })
            )
            .To<TagsController>(c => c.Tags(new TagsQuery { }))
            .Which(controller => controller
                .WithData(StaticTestData.GetTags(5, name)))
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
                      Title = $"{name}{i}",
                  };
              }).ToList(),
            }));


        [Theory]
        [MemberData(nameof(ValidData))]
        public void Listing_tags_with_special_case_ip_address_should_fail(
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
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
            .Pipeline()
            .ShouldMap(request => request
               .WithHeaders(new Dictionary<string, string>
               {
                   ["X-Real-IP"] = "0.0.0.0",
                   ["X-Real-LIMIT"] = "1"
               })
               .WithMethod(HttpMethod.Get)
               .WithLocation("api/v1.0/tags")
               .WithFormFields(new { })
            )
            .To<TagsController>(c => c.Tags(new TagsQuery { }))
            .Which(controller => controller
                .WithData(StaticTestData.GetTagsWithRateLimitMiddleware(5, name)));
        }, new Dictionary<string, string[]>
        {
            { "MatchingRulesException", new[] { "Matching Rules Exception" } },
        });

        public static IEnumerable<object[]> ValidData()
        {
            yield return new object[]
            {
            ValidMinUserNameLength,
            //Must be valid email address
            $"{ValidMinEmailLength}@a.bcde",
            //Password must contain Upper case, lower case, number, special symbols
            $"U!{ValidMinPasswordLength}",

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
            $"U!{ValidMaxPasswordLength}",

            ValidMaxxNameLength,

            ValidMaxxTitleLength,
            ValidMaxxTitleLength,
            ValidMaxxDescriptionLength,
            };
        }
    }
}