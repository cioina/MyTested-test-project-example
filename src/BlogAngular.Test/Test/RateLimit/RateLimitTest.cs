using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BlogAngular.Application.Blog.Common;
using BlogAngular.Application.Blog.Tags.Commands.Common;
using BlogAngular.Application.Blog.Tags.Queries.Listing;
using BlogAngular.Application.Identity.Commands.Common;
using BlogAngular.Application.Identity.Commands.Login;
using BlogAngular.Application.Identity.Commands.Update;
using BlogAngular.Test.Data;
using BlogAngular.Web.Features;
using MyTested.AspNetCore.Mvc;
using Xunit;
using static BlogAngular.Domain.Common.Models.ModelConstants.Article;
using static BlogAngular.Domain.Common.Models.ModelConstants.Identity;
using static BlogAngular.Domain.Common.Models.ModelConstants.Tag;
using static MyTested.AspNetCore.Mvc.Test.Setups.Test;

namespace BlogAngular.Test.RateLimit
{
    public class RateLimitRouteTest
    {
        private static readonly string VALID_MIN_USER_NAME_LENGTH = new('t', MinUserNameLength - 1);
        private static readonly string VALID_MAX_USER_NAME_LENGTH = new('t', MaxUserNameLength - 1);

        private static readonly string VALID_MIN_EMAIL_LENGTH = new('t', MinEmailLength - 1);
        private static readonly string VALID_MAX_EMAIL_LENGTH = new('t', MaxEmailLength - 8);

        private static readonly string VALID_MIN_PASSWORD_LENGTH = new('t', MinPasswordLength - 3);
        private static readonly string VALID_MAX_PASSWORD_LENGTH = new('t', MaxPasswordLength - 3);

        private static readonly string VALID_MIN_NAME_LENGTH = new('t', MinNameLength - 1);
        private static readonly string VALID_MAX_NAME_LENGTH = new('t', MaxNameLength - 1);

        private static readonly string VALID_MIN_TITLE_LENGTH = new('t', MinTitleLength - 1);
        private static readonly string VALID_MAX_TITLE_LENGTH = new('t', MaxTitleLength - 1);
        private static readonly string VALID_MIN_DESCRIPTION_LENGTH = new('t', MinDescriptionLength - 1);
        private static readonly string VALID_MAX_DESCRIPTION_LENGTH = new('t', MaxDescriptionLength - 1);

        public static IEnumerable<object[]> ValidData()
        {
            yield return new object[]
            {
            VALID_MIN_USER_NAME_LENGTH,
            //Must be valid email address
            $"{VALID_MIN_EMAIL_LENGTH}@a.bcde",
            //Password must contain Upper case, lower case, number, special symbols
            $"U!{VALID_MIN_PASSWORD_LENGTH}",

            VALID_MIN_NAME_LENGTH,

            VALID_MIN_TITLE_LENGTH,
            VALID_MIN_TITLE_LENGTH,
            VALID_MIN_DESCRIPTION_LENGTH,
            };

            yield return new object[]
            {
            VALID_MAX_USER_NAME_LENGTH,
            //Must be valid email address
            $"{VALID_MAX_EMAIL_LENGTH}@a.bcde",
            //Password must contain Upper case, lower case, number, special symbols
            $"U!{VALID_MAX_PASSWORD_LENGTH}",

            VALID_MAX_NAME_LENGTH,

            VALID_MAX_TITLE_LENGTH,
            VALID_MAX_TITLE_LENGTH,
            VALID_MAX_DESCRIPTION_LENGTH,
            };
        }

        #region Success Tests
        [Theory]
        [MemberData(nameof(ValidData))]
        public void LoginUserWithWhitelistedPublicRouteAndZeroLimitShouldReturnSuccessWithToken(
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
              string.Format(CultureInfo.InvariantCulture, @"{{""user"":{{""email"":""{0}"",""password"":""{1}""}}}}",
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
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 3, 4));

        [Theory]
        [MemberData(nameof(ValidData))]
        //See "ClientWhitelist" config
        public void EditTagWithWhitelistedClientIdAndZeroLimitOnPrivateRouteShouldReturnSuccessWithData(
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
                 string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
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
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingTagsWithoutMiddlewareAndZeroRateLimitShouldReturnSuccessWithAllTags(
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
            Models = [.. Enumerable
          .Range(1, 5)
          .Select(i => new TagResponseModel
              {
                  Id = i,
                  Title = $"{name}{i}",
              })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditTagWithWhitelistedIpAddressAndZeroLimitShouldReturnSuccessWithData(
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
                 string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
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
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [InlineData(
         "ValidMinUserNameLength",
         //Must be valid email address
         "SecurityTokenRefreshException@email.com",
         //Password must contain Upper case, lower case, number, special symbols
         "!ValidMinPasswordLength",
         "ValidMinNameLength",
         "ValidMinTitleLength",
         "ValidMinTitleLength",
         "ValidMinDescriptionLength")]
        //This has a special email
        public void LoginWithPasswordWithRefreshTokenAndWhitelistedPrivateRouteShouldReturnSuccessWithToken(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description
         )
        => MyMvc
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
                string.Format(CultureInfo.InvariantCulture, @"{{""user"":{{""password"":""{0}""}}}}",
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
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 3, 4));
        #endregion

        [Theory]
        [InlineData(
         "ValidMinUserNameLength",
         //Must be valid email address
         "SecurityTokenRefreshException@email.com",
         //Password must contain Upper case, lower case, number, special symbols
         "!ValidMinPasswordLength",
         "ValidMinNameLength",
         "ValidMinTitleLength",
         "ValidMinTitleLength",
         "ValidMinDescriptionLength")]
        //This has a special email
        public void EditTagWithRefreshTokenShouldFail(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description
         )
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () => MyMvc
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
                     string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
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
                   dbContext: entities)))), new Dictionary<string, string[]>
        {
           { "SecurityTokenRefreshException", ["Security token must be refreshed"] },
        });

        [Theory]
        [InlineData(
         "ValidMinUserNameLength",
         //Must be valid email address
         "ValidEmail@email.com",
         //Password must contain Upper case, lower case, number, special symbols
         "!ValidMinPasswordLength",
         "ValidMinNameLength",
         "ValidMinTitleLength",
         "ValidMinTitleLength",
         "ValidMinDescriptionLength")]
        //In case of valid token and changed IP address, the token should refresh
        public void EditTagWithValidTokenAndFakeIpShouldFail(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description
         )
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () => MyMvc
            .Pipeline()
            .ShouldMap(request => request
               .WithHeaders(new Dictionary<string, string>
               {
                   ["X-Real-IP"] = "16.8.8.0",
                   ["X-Real-LIMIT"] = "0"
               })
              .WithMethod(HttpMethod.Put)
              .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithRole(email, 1, AdministratorRoleName, "0.0.0.0"))
              .WithLocation("api/v1.0/tags/edit/2")
              .WithJsonBody(
                     string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
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
                   dbContext: entities)))), new Dictionary<string, string[]>
        {
           { "SecurityTokenRefreshException", ["Security token must be refreshed"] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        //See "ClientWhitelist" config
        public void ListingTagsWithWhitelistedClientIdAndZeroLimitOnPublicRouteShouldFail(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description
         )
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () => MyMvc
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
                   dbContext: entities)))), new Dictionary<string, string[]>
        {
           { "RateLimitMiddlewareException", ["Too many requests"] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingTagsWithMiddlewareAndZeroRateLimitShouldFail(
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
        () => MyMvc
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
                .WithData(StaticTestData.GetTagsWithRateLimitMiddleware(5, name))), new Dictionary<string, string[]>
        {
            { "RateLimitMiddlewareException", ["Too many requests"] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingTagsWithSpecialCaseIpAddressShouldFail(
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
        () => MyMvc
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
                .WithData(StaticTestData.GetTagsWithRateLimitMiddleware(5, name))), new Dictionary<string, string[]>
        {
            { "MatchingRulesException", ["Matching Rules Exception"] },
        });
    }
}
