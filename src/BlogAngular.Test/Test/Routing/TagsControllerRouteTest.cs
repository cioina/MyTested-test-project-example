using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BlogAngular.Application.Blog.Common;
using BlogAngular.Application.Blog.Tags.Commands.Common;
using BlogAngular.Application.Blog.Tags.Queries.Listing;
using BlogAngular.Test.Data;
using BlogAngular.Web.Features;
using MyTested.AspNetCore.Mvc;
using Xunit;
using static BlogAngular.Domain.Common.Models.ModelConstants.Article;
using static BlogAngular.Domain.Common.Models.ModelConstants.Identity;
using static BlogAngular.Domain.Common.Models.ModelConstants.Tag;
using static BlogAngular.Test.Routing.ControllerExceptionMessages;
using static MyTested.AspNetCore.Mvc.Test.Setups.Test;

namespace BlogAngular.Test.Routing
{
    public class TagsControllerRouteTest
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
        #region Create Tag
        [Theory]
        [MemberData(nameof(ValidData))]
        public void CreateTagShouldReturnSuccessWithData(
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
          .WithMethod(HttpMethod.Post)
          .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
          .WithLocation("api/v1.0/tags/create")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                     $"{name}4")
          )
        )
        .To<TagsController>(c => c.Create(new()
        {
            TagJson = new()
            {
                Title = $"{name}4"
            }
        }))
        .Which(controller => controller
          .WithData(db => db
            .WithEntities(entities => StaticTestData.GetArticlesTagsUsersWithRole(
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
        .ActionResult(result => result.Result(new TagResponseEnvelope
        {
            TagJson = new()
            {
                Id = 4,
                Title = $"{name}4"
            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EXPERIMENTALCreateTagWithDeletedRoleShouldFail(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/create")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                          $"{name}4")
               )
             )
             .To<TagsController>(c => c.Create(new()
             {
                 TagJson = new()
                 {
                     Title = $"{name}4"
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetArticlesTagsUsers(3,
                      email,
                      fullName,
                      password,
                      name,
                      title,
                      slug,
                      description,
                      DateOnly.FromDateTime(DateTime.Today),
                      false)))
             .ShouldHave()
             .ActionAttributes(attrs => attrs
                  .RestrictingForHttpMethod(HttpMethod.Post)
                  .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn(), new Dictionary<string, string[]>
        {
           { "is_in_role_error", [$"Cannot find role {AdministratorRoleName}"] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EXPERIMENTALCreateTagWithDeletedUserShouldFail(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 4))
               .WithLocation("api/v1.0/tags/create")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                          $"{name}4")
               )
             )
             .To<TagsController>(c => c.Create(new()
             {
                 TagJson = new()
                 {
                     Title = $"{name}4"
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetArticlesTagsUsers(3,
                      email,
                      fullName,
                      password,
                      name,
                      title,
                      slug,
                      description,
                      DateOnly.FromDateTime(DateTime.Today),
                      false)))
             .ShouldHave()
             .ActionAttributes(attrs => attrs
                  .RestrictingForHttpMethod(HttpMethod.Post)
                  .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn(), new Dictionary<string, string[]>
        {
           { "user_error", ["Cannot find user by Id."] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void CreateTagWithSomeRoleShouldFail(
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        string fullName,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        string email,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        string password,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        string name,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        string title,
        string slug,
        string description
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
         )
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithRole(email, 1, "SomeRole", null))
               .WithLocation("api/v1.0/tags/create")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                          $"{name}4")
               )
             )
             .To<TagsController>(c => c.Create(new()
             {
                 TagJson = new()
                 {
                     Title = $"{name}4"
                 }
             })), string.Format(CultureInfo.InvariantCulture, HEADER_AUTHORIZATION_EXCEPTION.Replace(Environment.NewLine, ""), "/api/v1.0/tags/create", "Create", "TagsController"));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void CreateTagWithoutRoleShouldFail(
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        string fullName,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        string email,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        string password,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        string name,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        string title,
        string slug,
        string description
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
         )
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithRole(email, 1, "", null))
               .WithLocation("api/v1.0/tags/create")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                          $"{name}4")
               )
             )
             .To<TagsController>(c => c.Create(new()
             {
                 TagJson = new()
                 {
                     Title = $"{name}4"
                 }
             })), string.Format(CultureInfo.InvariantCulture, HEADER_AUTHORIZATION_EXCEPTION.Replace(Environment.NewLine, ""), "/api/v1.0/tags/create", "Create", "TagsController"));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void CreateTagWithOneCharShouldReturnValidationError(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/create")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                          "a")
               )
             )
             .To<TagsController>(c => c.Create(new()
             {
                 TagJson = new()
                 {
                     Title = "a"
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetArticlesTagsUsers(3,
                      email,
                      fullName,
                      password,
                      name,
                      title,
                      slug,
                      description,
                      DateOnly.FromDateTime(DateTime.Today),
                      false)))
             .ShouldHave()
             .ActionAttributes(attrs => attrs
                  .RestrictingForHttpMethod(HttpMethod.Post)
                  .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn(), new Dictionary<string, string[]>
        {
          { "TagJson.Title",  ["The length of 'Tag Json Title' must be at least 2 characters. You entered 1 characters."] } ,
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void CreateTagWithMaxCharsShouldReturnValidationError(
        string fullName,
        string email,
        string password,
        string name,
        string title,
        string slug,
        string description)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/create")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                          $"{VALID_MAX_NAME_LENGTH}ab")
               )
             )
             .To<TagsController>(c => c.Create(new()
             {
                 TagJson = new()
                 {
                     Title = $"{VALID_MAX_NAME_LENGTH}ab"
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetArticlesTagsUsers(3,
                      email,
                      fullName,
                      password,
                      name,
                      title,
                      slug,
                      description,
                      DateOnly.FromDateTime(DateTime.Today),
                      false)))
             .ShouldHave()
             .ActionAttributes(attrs => attrs
                  .RestrictingForHttpMethod(HttpMethod.Post)
                  .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn(), new Dictionary<string, string[]>
        {
           { "TagJson.Title", ["The length of 'Tag Json Title' must be 420 characters or fewer. You entered 421 characters."] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void CreateTagWithSameNameShouldFailWithValidationError(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/create")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                          $"{name}1")
               )
             )
             .To<TagsController>(c => c.Create(new()
             {
                 TagJson = new()
                 {
                     Title = $"{name}1"
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetArticlesTagsUsers(3,
                      email,
                      fullName,
                      password,
                      name,
                      title,
                      slug,
                      description,
                      DateOnly.FromDateTime(DateTime.Today),
                      false)))
             .ShouldHave()
             .ActionAttributes(attrs => attrs
                  .RestrictingForHttpMethod(HttpMethod.Post)
                  .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn(), new Dictionary<string, string[]>
        {
           { "TagJson.Title", ["'Tag Json Title' must be unique."] },
        });

        #endregion

        #region Edit Tag
        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditTagShouldReturnSuccessWithData(
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
          .WithData(StaticTestData.GetArticlesTagsUsers(3,
                 email,
                 fullName,
                 password,
                 name,
                 title,
                 slug,
                 description,
                 DateOnly.FromDateTime(DateTime.Today),
                 false)))
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
        public void EditSameTagWithSameNameShouldReturnSuccessWithData(
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
          .WithMethod(HttpMethod.Put)
          .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
          .WithLocation("api/v1.0/tags/edit/2")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                     $"{name}2")
          )
        )
        .To<TagsController>(c => c.Edit(2, new()
        {
            TagJson = new()
            {
                Title = $"{name}2"
            }
        }))
        .Which(controller => controller
          .WithData(StaticTestData.GetArticlesTagsUsers(3,
                 email,
                 fullName,
                 password,
                 name,
                 title,
                 slug,
                 description,
                 DateOnly.FromDateTime(DateTime.Today),
                 false)))
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
                Title = $"{name}2"
            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditTagWithOneCharShouldReturnValidationError(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Put)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/edit/2")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                          "a")
               )
             )
             .To<TagsController>(c => c.Edit(2, new()
             {
                 TagJson = new()
                 {
                     Title = "a"
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetArticlesTagsUsers(3,
                      email,
                      fullName,
                      password,
                      name,
                      title,
                      slug,
                      description,
                      DateOnly.FromDateTime(DateTime.Today),
                      false)))
             .ShouldHave()
             .ActionAttributes(attrs => attrs
                  .RestrictingForHttpMethod(HttpMethod.Put)
                  .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn(), new Dictionary<string, string[]>
        {
            { "TagJson.Title", ["The length of 'Tag Json Title' must be at least 2 characters. You entered 1 characters."] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditTagWithMaxCharsShouldReturnValidationError(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Put)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/edit/2")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                          $"{VALID_MAX_NAME_LENGTH}ab")
               )
             )
             .To<TagsController>(c => c.Edit(2, new()
             {
                 TagJson = new()
                 {
                     Title = $"{VALID_MAX_NAME_LENGTH}ab"
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetArticlesTagsUsers(3,
                      email,
                      fullName,
                      password,
                      name,
                      title,
                      slug,
                      description,
                      DateOnly.FromDateTime(DateTime.Today),
                      false)))
             .ShouldHave()
             .ActionAttributes(attrs => attrs
                  .RestrictingForHttpMethod(HttpMethod.Put)
                  .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn(), new Dictionary<string, string[]>
        {
            { "TagJson.Title", ["The length of 'Tag Json Title' must be 420 characters or fewer. You entered 421 characters."] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditTagWithWrongIdShouldFail(
        string fullName,
        string email,
        string password,
        string name,
        string title,
        string slug,
        string description)
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.InvocationAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Put)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/edit/5")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                          $"{name}4")
               )
             )
             .To<TagsController>(c => c.Edit(5, new()
             {
                 TagJson = new()
                 {
                     Title = $"{name}4"
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetArticlesTagsUsers(3,
                      email,
                      fullName,
                      password,
                      name,
                      title,
                      slug,
                      description,
                      DateOnly.FromDateTime(DateTime.Today),
                      false)))
             .ShouldHave()
             .ActionAttributes(attrs => attrs
                  .RestrictingForHttpMethod(HttpMethod.Put)
                  .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn(), string.Format(CultureInfo.InvariantCulture, FROM_NOT_FOUND_EXCEPTION.Replace(Environment.NewLine, ""), "Edit", "TagsController", "NotFoundException", "tag", 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditTagWithIncorrecrIdFormatShouldFail(
        string fullName,
        string email,
        string password,
        string name,
        string title,
        string slug,
        string description)
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Put)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/edit/a")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                          $"{name}4")
               )
             )
             .To<TagsController>(c => c.Edit(5, new()
             {
                 TagJson = new()
                 {
                     Title = $"{name}4"
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetArticlesTagsUsers(3,
                      email,
                      fullName,
                      password,
                      name,
                      title,
                      slug,
                      description,
                      DateOnly.FromDateTime(DateTime.Today),
                      false)))
             .ShouldHave()
             .ActionAttributes(attrs => attrs
                  .RestrictingForHttpMethod(HttpMethod.Put)
                  .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn(), string.Format(CultureInfo.InvariantCulture, DIFFERENCE_FORMAT_EXCEPTION.Replace(Environment.NewLine, ""), "/api/v1.0/tags/edit/a", "id", "a"));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditTagWithSameNameShouldFailWithValidationError(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Put)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/edit/2")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""tag"":{{""title"": ""{0}"" }}}}",
                          $"{name}1")
               )
             )
             .To<TagsController>(c => c.Edit(2, new()
             {
                 TagJson = new()
                 {
                     Title = $"{name}1"
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetArticlesTagsUsers(3,
                      email,
                      fullName,
                      password,
                      name,
                      title,
                      slug,
                      description,
                      DateOnly.FromDateTime(DateTime.Today),
                      false)))
             .ShouldHave()
             .ActionAttributes(attrs => attrs
                  .RestrictingForHttpMethod(HttpMethod.Put)
                  .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn(), new Dictionary<string, string[]>
        {
            { "TagJson.Title", ["'Tag Json Title' must be unique."] },
        });
        #endregion

        #region Delete Tag
        [Theory]
        [MemberData(nameof(ValidData))]
        public void DeleteTagShouldReturnSuccessWithTagId(
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
          .WithMethod(HttpMethod.Delete)
          .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
          .WithLocation("api/v1.0/tags/delete/2")
        )
        .To<TagsController>(c => c.Delete(new()
        {
            Id = 2,
        }))
        .Which(controller => controller
          .WithData(StaticTestData.GetArticlesTagsUsers(5,
                 email,
                 fullName,
                 password,
                 name,
                 title,
                 slug,
                 description,
                 DateOnly.FromDateTime(DateTime.Today),
                 false)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs
             .RestrictingForHttpMethod(HttpMethod.Delete)
             .RestrictingForAuthorizedRequests())
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(2))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void DeleteNonExistingTagShouldReturnSuccessWithNegativeOne(
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
          .WithMethod(HttpMethod.Delete)
          .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
          .WithLocation("api/v1.0/tags/delete/20")
        )
        .To<TagsController>(c => c.Delete(new()
        {
            Id = 20,
        }))
        .Which(controller => controller
          .WithData(StaticTestData.GetArticlesTagsUsers(5,
                 email,
                 fullName,
                 password,
                 name,
                 title,
                 slug,
                 description,
                 DateOnly.FromDateTime(DateTime.Today),
                 false)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs
             .RestrictingForHttpMethod(HttpMethod.Delete)
             .RestrictingForAuthorizedRequests())
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(-1))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        //In browser, it returns success with -1
        public void DeleteNonExistingFormatTagShouldFail(
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
         string fullName,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
         string email,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
         string password,
         string name,
         string title,
         string slug,
         string description
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        )
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Delete)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/delete/a")
             )
             .To<TagsController>(c => c.Delete(new()
             {
                 Id = 1,
             })), string.Format(CultureInfo.InvariantCulture, DIFFERENCE_EXTENDED_EXCEPTION.Replace(Environment.NewLine, ""), "/api/v1.0/tags/delete/a", "command", "TagDeleteCommand.Id", "1", "0"));
        #endregion

        #region Listing Tags
        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingTagsWithUpperCaseTitleUrlParameterShouldReturnSuccessWithTagListEmpty(
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
           .WithMethod(HttpMethod.Get)
           .WithLocation($"api/v1.0/tags?Title=T1")
           .WithFormFields(new
           {
               Title = "T1"
           })
        )
        .To<TagsController>(c => c.Tags(new TagsQuery
        {
            Title = "T1"
        }))
        .Which(controller => controller
            .WithData(StaticTestData.GetTags(5, name)))
        .ShouldReturn()
        .ActionResult(result => result.Result(new TagsResponseEnvelope
        {
            Total = 0,
            Models = [],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingTagsWithTitleUrlParameterShouldReturnSuccessWithTagListWithLimit(
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
           .WithMethod(HttpMethod.Get)
           .WithLocation($"api/v1.0/tags?Title=t1")
           .WithFormFields(new
           {
               Title = "t1"
           })
        )
        .To<TagsController>(c => c.Tags(new TagsQuery
        {
            Title = "t1"
        }))
        .Which(controller => controller
            .WithData(StaticTestData.GetTags(5, name)))
        .ShouldReturn()
        .ActionResult(result => result.Result(new TagsResponseEnvelope
        {
            Total = 1,
            Models = [.. Enumerable
                      .Range(1, 1)
                      .Select(i => new TagResponseModel
                          {
                              Id = i,
                              Title = $"{name}{i}"
                          })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingTagsWithTitleDesUrlParameterShouldReturnSuccessWithTagListWithLimit(
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
           .WithMethod(HttpMethod.Get)
           .WithLocation($"api/v1.0/tags?Limit={4}&Offset={0}&TitleDes=true")
           .WithFormFields(new
           {
               Limit = 4,
               Offset = 0,
               TitleDes = true
           })
        )
        .To<TagsController>(c => c.Tags(new TagsQuery
        {
            Limit = 4,
            Offset = 0,
            TitleDes = true
        }))
        .Which(controller => controller
            .WithData(StaticTestData.GetTags(5, name)))
        .ShouldReturn()
        .ActionResult(result => result.Result(new TagsResponseEnvelope
        {
            Total = 5,
            Models = [.. Enumerable
              .Range(1, 4)
              .Select(i => new TagResponseModel
                  {
                      Id = 6 - i,
                      Title = $"{name}{ 6 - i}"
                  })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingTagsWithLimitUrlParameterShouldReturnSuccessWithTagListWithLimit(
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
           .WithMethod(HttpMethod.Get)
           .WithLocation($"api/v1.0/tags?Limit={4}&Offset={0}")
           .WithFormFields(new
           {
               Limit = 4,
               Offset = 0
           })
        )
        .To<TagsController>(c => c.Tags(new TagsQuery
        {
            Limit = 4,
            Offset = 0
        }))
        .Which(controller => controller
            .WithData(StaticTestData.GetTags(5, name)))
        .ShouldReturn()
        .ActionResult(result => result.Result(new TagsResponseEnvelope
        {
            Total = 5,
            Models = [.. Enumerable
              .Range(1, 4)
              .Select(i => new TagResponseModel
                  {
                      Id = i,
                      Title = $"{name}{i}"
                  })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingTagsWithoutUrlParametersShouldReturnSuccessWithAllTags(
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
                  Title = $"{name}{i}"
              })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingTagsWithWrongHeaderAuthorizationShouldReturnSuccessWithAllTags(
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
           .WithMethod(HttpMethod.Get)
           .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithRole(email, 1, "", null))
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
        public void ListingTagsWithHeaderAuthorizationShouldReturnSuccessWithAllTags(
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
           .WithMethod(HttpMethod.Get)
           .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
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
        public void ListingTagsWithMaxTitleShouldReturnValidationError(
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
                 .WithMethod(HttpMethod.Get)
                 .WithLocation($"api/v1.0/tags?Limit={0}&Offset={-1}&Title={VALID_MAX_NAME_LENGTH}ab")
                 .WithFormFields(new
                 {
                     Limit = 0,
                     Offset = -1,
                     Title = $"{VALID_MAX_NAME_LENGTH}ab"
                 })
              )
              .To<TagsController>(c => c.Tags(new TagsQuery
              {
                  Limit = 0,
                  Offset = -1,
                  Title = $"{VALID_MAX_NAME_LENGTH}ab"
              }))
              .Which(controller => controller
                  .WithData(StaticTestData.GetTags(5, name)))
              .ShouldReturn(), new Dictionary<string, string[]>
         {
            { "Title", ["The length of 'Title' must be 420 characters or fewer. You entered 421 characters."] },
            { "Limit", ["'Limit' must be greater than '0'."] },
            { "Offset", ["'Offset' must be greater than '-1'."] }
         });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingTagsWithWrongLimitAndOffsetAndTittleShouldReturnValidationError(
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
                .WithMethod(HttpMethod.Get)
                .WithLocation($"api/v1.0/tags?Limit=0&Offset=-1&Title=%20t")
                .WithFormFields(new
                {
                    Limit = 0,
                    Offset = -1,
                    Title = " t"
                })
             )
             .To<TagsController>(c => c.Tags(new TagsQuery
             {
                 Limit = 0,
                 Offset = -1,
                 Title = " t"
             }))
             .Which(controller => controller
                 .WithData(StaticTestData.GetTags(5, name)))
             .ShouldReturn(), new Dictionary<string, string[]>
        {
           { "Title", ["The specified condition was not met for 'Title'."] },
           { "Limit", ["'Limit' must be greater than '0'."] },
           { "Offset",["'Offset' must be greater than '-1'."] }
        });
        #endregion
    }
}
