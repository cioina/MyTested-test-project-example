#if DEBUG
using BlogAngular.Application.Blog.Common;
using BlogAngular.Application.Blog.Tags.Commands.Common;
using BlogAngular.Application.Blog.Tags.Queries.Listing;
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
using static BlogAngular.Test.Routing.ControllerExceptionMessages;
using static MyTested.AspNetCore.Mvc.Test.Setups.Test;

namespace BlogAngular.Test.Routing
{
    public class TagsControllerRouteTest
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
        public void Create_tag_should_return_success_with_data(
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
                   string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
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
          .ShouldPassForThe<ActionAttributes>(attributes =>
          {
              Assert.Equal(5, attributes.Count());
          });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EXPERIMENTAL_Create_tag_with_deleted_role_should_fail(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/create")
               .WithJsonBody(
                      string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
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
             .ShouldReturn();
        }, new Dictionary<string, string[]>
         {
         { "is_in_role_error", new[] { "Cannot find role Administrator" } },
         });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EXPERIMENTAL_Create_tag_with_deleted_user_should_fail(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
         => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
         () =>
         {
             MyMvc
                  .Pipeline()
                  .ShouldMap(request => request
                    .WithMethod(HttpMethod.Post)
                    .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 4))
                    .WithLocation("api/v1.0/tags/create")
                    .WithJsonBody(
                           string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
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
                  .ShouldReturn();
         }, new Dictionary<string, string[]>
         {
             { "user_error", new[] { "Cannot find user by Id." } },
         });

        //    [Theory]
        //    [MemberData(nameof(ValidData))]
        //    public void EXPERIMENTAL_Create_tag_with_wrong_ip_should_fail1(
        //#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        //    string fullName,
        //#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        //    string email,
        //#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        //    string password,
        //#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        //    string name,
        //#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        //    string title,
        //    string slug,
        //    string description
        //#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        //     )
        //     => Test.AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        //     () =>
        //     {
        //         MyMvc
        //          .Pipeline()
        //          .ShouldMap(request => request
        //            .WithMethod(HttpMethod.Post)
        //            .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithRole(email, 1, AdministratorRoleName, "0.0.0.0"))
        //            .WithLocation("api/v1.0/tags/create")
        //            .WithJsonBody(
        //                   string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
        //                       $"{name}4")
        //            )
        //          )
        //          .To<TagsController>(c => c.Create(new()
        //          {
        //              TagJson = new()
        //              {
        //                  Title = $"{name}4"
        //              }
        //          }));

        //     }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/tags/create", "Create", "TagsController"));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Create_tag_with_some_role_should_fail(
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
         () =>
         {
             MyMvc
              .Pipeline()
              .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithRole(email, 1, "SomeRole", null))
                .WithLocation("api/v1.0/tags/create")
                .WithJsonBody(
                       string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
                           $"{name}4")
                )
              )
              .To<TagsController>(c => c.Create(new()
              {
                  TagJson = new()
                  {
                      Title = $"{name}4"
                  }
              }));

         }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/tags/create", "Create", "TagsController"));


        [Theory]
        [MemberData(nameof(ValidData))]
        public void Create_tag_without_role_should_fail(
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
         () =>
         {
             MyMvc
              .Pipeline()
              .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithRole(email, 1, "", null))
                .WithLocation("api/v1.0/tags/create")
                .WithJsonBody(
                       string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
                           $"{name}4")
                )
              )
              .To<TagsController>(c => c.Create(new()
              {
                  TagJson = new()
                  {
                      Title = $"{name}4"
                  }
              }));

         }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/tags/create", "Create", "TagsController"));


        [Theory]
        [MemberData(nameof(ValidData))]
        public void Create_tag_with_one_char_should_return_validation_error(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/create")
               .WithJsonBody(
                      string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
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
             .ShouldReturn();
        }, new Dictionary<string, string[]>
         {
         { "TagJson.Title", new[] { "The length of 'Tag Json Title' must be at least 2 characters. You entered 1 characters." } },
         });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Create_tag_with_max_chars_should_return_validation_error(
        string fullName,
        string email,
        string password,
        string name,
        string title,
        string slug,
        string description)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/create")
               .WithJsonBody(
                      string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
                          $"{ValidMaxxNameLength}ab")
               )
             )
             .To<TagsController>(c => c.Create(new()
             {
                 TagJson = new()
                 {
                     Title = $"{ValidMaxxNameLength}ab"
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
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
        { "TagJson.Title", new[] { "The length of 'Tag Json Title' must be 420 characters or fewer. You entered 421 characters." } },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Edit_tag_should_return_success_with_data(
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
          .ShouldPassForThe<ActionAttributes>(attributes =>
          {
              Assert.Equal(5, attributes.Count());
          });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Edit_tag_with_one_char_should_return_validation_error(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Put)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/edit/2")
               .WithJsonBody(
                      string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
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
             .ShouldReturn();
        }, new Dictionary<string, string[]>
         {
         { "TagJson.Title", new[] { "The length of 'Tag Json Title' must be at least 2 characters. You entered 1 characters." } },
         });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Edit_tag_with_max_chars_should_return_validation_error(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
         => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
         () =>
         {
             MyMvc
              .Pipeline()
              .ShouldMap(request => request
                .WithMethod(HttpMethod.Put)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/tags/edit/2")
                .WithJsonBody(
                       string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
                           $"{ValidMaxxNameLength}ab")
                )
              )
              .To<TagsController>(c => c.Edit(2, new()
              {
                  TagJson = new()
                  {
                      Title = $"{ValidMaxxNameLength}ab"
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
              .ShouldReturn();
         }, new Dictionary<string, string[]>
         {
         { "TagJson.Title", new[] { "The length of 'Tag Json Title' must be 420 characters or fewer. You entered 421 characters." } },
         });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Edit_tag_with_wrong_id_should_fail(
        string fullName,
        string email,
        string password,
        string name,
        string title,
        string slug,
        string description)
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.InvocationAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Put)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/tags/edit/5")
               .WithJsonBody(
                      string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
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
             .ShouldReturn();
        }, string.Format(FromNotFoundException.Replace(Environment.NewLine, ""), "Edit", "TagsController", "NotFoundException", "tag", 5));


        [Theory]
        [MemberData(nameof(ValidData))]
        public void Edit_same_tag_with_same_name_should_return_success_with_data(
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
                   string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
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
          .ShouldPassForThe<ActionAttributes>(attributes =>
          {
              Assert.Equal(5, attributes.Count());
          });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Edit_tag_with_same_name_should_fail_with_validation_error(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
         => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
         () =>
         {
             MyMvc
              .Pipeline()
              .ShouldMap(request => request
                .WithMethod(HttpMethod.Put)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/tags/edit/2")
                .WithJsonBody(
                       string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
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
              .ShouldReturn();
         }, new Dictionary<string, string[]>
         {
         { "TagJson.Title", new[] { "'Tag Json Title' must be unique." } },
         });


        [Theory]
        [MemberData(nameof(ValidData))]
        public void Create_tag_with_same_name_should_fail_with_validation_error(
         string fullName,
         string email,
         string password,
         string name,
         string title,
         string slug,
         string description)
         => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
         () =>
         {
             MyMvc
              .Pipeline()
              .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/tags/create")
                .WithJsonBody(
                       string.Format(@"{{""tag"":{{""title"": ""{0}"" }}}}",
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
              .ShouldReturn();
         }, new Dictionary<string, string[]>
         {
         { "TagJson.Title", new[] { "'Tag Json Title' must be unique." } },
         });


        [Theory]
        [MemberData(nameof(ValidData))]
        public void Delete_tag_should_return_success_with_tag_id(
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
          .ShouldPassForThe<ActionAttributes>(attributes =>
          {
              Assert.Equal(5, attributes.Count());
          });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Delete_non_existing_tag_should_return_success_with_negative_one(
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
          .ShouldPassForThe<ActionAttributes>(attributes =>
          {
              Assert.Equal(5, attributes.Count());
          });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Listing_tags_with_limit_url_parameter_should_return_success_with_tag_list_with_limit(
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
                Models = Enumerable
                  .Range(1, 4)
                  .Select(i =>
                  {
                      return new TagResponseModel
                      {
                          Id = i,
                          Title = $"{name}{i}"
                      };
                  }).ToList(),
            }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Listing_tags_without_url_parameters_should_return_success_with_all_tags(
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
                Models = Enumerable
              .Range(1, 5)
              .Select(i =>
              {
                  return new TagResponseModel
                  {
                      Id = i,
                      Title = $"{name}{i}"
                  };
              }).ToList(),
            }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void Listing_tags_with_wrong_header_authorization_should_return_success_with_all_tags1(
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
#endif