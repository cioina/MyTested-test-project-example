﻿using BlogAngular.Application.Identity.Commands.Common;
using BlogAngular.Application.Identity.Commands.Login;
using BlogAngular.Application.Identity.Commands.Register;
using BlogAngular.Application.Identity.Commands.Update;
using BlogAngular.Application.Identity.Queries.Profile;
using BlogAngular.Test.Data;
using BlogAngular.Web.Features;
using MyTested.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static BlogAngular.Domain.Common.Models.ModelConstants.Identity;
using static BlogAngular.Test.Routing.ControllerExceptionMessages;
using static MyTested.AspNetCore.Mvc.Test.Setups.Test;

namespace BlogAngular.Test.Routing
{
    public class ControllerExceptionMessages
    {
        //In browser, it returns 401
        public const string HeaderAuthorizationException =
    @"Expected route '{0}' to match {1} action in {2} but action could not be invoked because 
of the declared filters - ApiControllerAttribute (Controller), AuthorizeFilter (Action), 
UnsupportedContentTypeFilter (Global). Either a filter is setting the response result before the 
action itself, or you must set the request properties so that they will pass through the pipeline.";
        //In browser, it returns 422 Queried object tag was not found, Key: 5
        public const string FromNotFoundException =
    @"When calling {0} action in {1} expected no exception but AggregateException (containing 
{2} with 'Queried object {3} was not found, Key: {4}' message) was thrown without being caught.";
        //In browser, it returns 422
        public const string DifferenceException =
    @"Expected route '{0}' to contain route value with '{1}' key and the provided value but 
the value was different. Difference occurs at '{2}'.";
        //In browser, it returns 422
        public const string DifferenceExtendedException =
    @"Expected route '{0}' to contain route value with '{1}' key and the provided value but 
the value was different. Difference occurs at '{2}'. 
Expected a value of '{3}', but in fact it was '{4}'.";
        //In browser, it returns 422 Queried object tag was not found, Key: 0
        public const string DifferenceFormatException =
    @"Expected route '{0}' to contain route value with '{1}' key and the provided value but 
the value was different - {2}.";
        //In browser, it returns 404
        public const string RouteCouldNotBeMachedException =
    @"Expected route '{0}' to match {1} action in {2} but action could not be matched.";
        //In browser, it returns 422 Cannot find user profile.
        public const string HttpRequestException =
    @"When building HttpRequest expected location to be URI valid, but instead received '{0}'.";
    }

    public class IdentityControllerRouteTest
    {
        private static readonly string ValidMinUserNameLength = new('t', MinUserNameLength);
        private static readonly string ValidMaxUserNameLength = new('t', MaxUserNameLength - 1);

        private static readonly string ValidMinEmailLength = new('t', MinEmailLength);
        private static readonly string ValidMaxEmailLength = new('t', MaxEmailLength - 8);

        private static readonly string ValidMinPasswordLength = new('t', MinPasswordLength - 3);
        private static readonly string ValidMaxPasswordLength = new('t', MaxPasswordLength - 3);

        public static IEnumerable<object[]> RegisterValidData()
        {
            yield return new object[]
            {
            ValidMinUserNameLength,
            //Must be valid email address
            $"{ValidMinEmailLength}@a.bcde",
            //Password must contain Upper case, lower case, number, special symbols
            $"U!{ValidMinPasswordLength}",
            };

            yield return new object[]
            {
             ValidMaxUserNameLength,
             //Must be valid email address
             $"{ValidMaxEmailLength}@a.bcde",
             //Password must contain Upper case, lower case, number, special symbols
             $"U!{ValidMaxPasswordLength}"
            };
        }

        #region Update
        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_should_return_success_with_token(
         string fullName,
         string email,
         string password)
        => MyMvc
        .Pipeline()
        .ShouldMap(request => request
           .WithMethod(HttpMethod.Put)
           .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
           .WithLocation("api/v1.0/identity/update")
           .WithJsonBody(
                string.Format(@"{{""user"":{{""password"":""{0}"",""username"":""{1}""}}}}",
                    $"{password}4",
                    $"{fullName}4"
                )
           )
        )
        .To<IdentityController>(c => c.Update(new UserUpdateCommand
        {
            UserJson = new()
            {
                FullName = $"{fullName}4",
                Password = $"{password}4"
            }
        }))
        .Which(controller => controller
              .WithData(db => db
                   .WithEntities(entities => StaticTestData.GetUsersWithRole(
                       count: 3,
                       email: email,
                       userName: fullName,
                       password: password,
                       dbContext: entities))))
        .ShouldHave()
        .ActionAttributes(attrs => attrs
            .RestrictingForHttpMethod(HttpMethod.Put)
            .RestrictingForAuthorizedRequests())
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new UserResponseEnvelope
        {
            UserJson = new()
            {
                Email = $"{email}1",
                UserName = $"{fullName}4",
                Token = $"Token: {email}1"
            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes =>
        {
            Assert.Equal(5, attributes.Count());
        });
        
        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_name_only_should_return_success_with_token(
         string fullName,
         string email,
         string password)
        => MyMvc
        .Pipeline()
        .ShouldMap(request => request
           .WithMethod(HttpMethod.Put)
           .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
           .WithLocation("api/v1.0/identity/update")
           .WithJsonBody(
                string.Format(@"{{""user"":{{""username"":""{0}""}}}}",
                    $"{fullName}4"
                )
           )
        )
        .To<IdentityController>(c => c.Update(new UserUpdateCommand
        {
            UserJson = new()
            {
                FullName = $"{fullName}4",
                Password = null,
            }
        }))
        .Which(controller => controller
              .WithData(db => db
                   .WithEntities(entities => StaticTestData.GetUsersWithRole(
                       count: 3,
                       email: email,
                       userName: fullName,
                       password: password,
                       dbContext: entities))))
        .ShouldHave()
        .ActionAttributes(attrs => attrs
            .RestrictingForHttpMethod(HttpMethod.Put)
            .RestrictingForAuthorizedRequests())
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new UserResponseEnvelope
        {
            UserJson = new()
            {
                Email = $"{email}1",
                UserName = $"{fullName}4",
                Token = $"Token: {email}1"
            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes =>
        {
            Assert.Equal(5, attributes.Count());
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_password_only_should_return_success_with_token(
         string fullName,
         string email,
         string password)
        => MyMvc
        .Pipeline()
        .ShouldMap(request => request
           .WithMethod(HttpMethod.Put)
           .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
           .WithLocation("api/v1.0/identity/update")
           .WithJsonBody(
                string.Format(@"{{""user"":{{""password"":""{0}""}}}}",
                    $"{password}1"
                )
           )
        )
        .To<IdentityController>(c => c.Update(new UserUpdateCommand
        {
            UserJson = new()
            {
                FullName = null,
                Password = $"{password}1"
            }
        }))
        .Which(controller => controller
              .WithData(db => db
                   .WithEntities(entities => StaticTestData.GetUsersWithRole(
                       count: 3,
                       email: email,
                       userName: fullName,
                       password: password,
                       dbContext: entities))))
        .ShouldHave()
        .ActionAttributes(attrs => attrs
            .RestrictingForHttpMethod(HttpMethod.Put)
            .RestrictingForAuthorizedRequests())
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new UserResponseEnvelope
        {
            UserJson = new()
            {
                Email = $"{email}1",
                UserName = $"{fullName}1",
                Token = $"Token: {email}1",
            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes =>
        {
            Assert.Equal(5, attributes.Count());
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_without_authorization_header_should_fail(
         string fullName,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
         string email,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
         string password)
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                 .WithMethod(HttpMethod.Put)
                 // without WithHeaderAuthorization
                 .WithLocation("api/v1.0/identity/update")
                 .WithJsonBody(
                      string.Format(@"{{""user"":{{""password"":""{0}"",""username"":""{1}""}}}}",
                          $"{password}1",
                          $"{fullName}1"
                      )
                 )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName}1",
                     Password = $"{password}1"
                 }
             }));
        }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/identity/update", "Update", "IdentityController"));

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_with_altered_authorization_header_should_fail(
         string fullName,
         string email,
         string password)
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Put)
                .WithHeaderAuthorization($"{StaticTestData.GetJwtBearerAdministratorRole(email, 1)}a")
                .WithLocation("api/v1.0/identity/update")
                .WithJsonBody(
                     string.Format(@"{{""user"":{{""password"":""{0}"",""username"":""{1}""}}}}",
                         $"{password}1",
                         $"{fullName}1"
                     )
                )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName}1",
                     Password = $"{password}1"
                 }
             }));
        }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/identity/update", "Update", "IdentityController"));

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_with_malformated_authorization_header_should_fail(
         string fullName,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
         string email,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
         string password)
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Put)
                .WithHeaderAuthorization($"{Guid.NewGuid()}.{Guid.NewGuid()}")
                .WithLocation("api/v1.0/identity/update")
                .WithJsonBody(
                     string.Format(@"{{""user"":{{""password"":""{0}"",""username"":""{1}""}}}}",
                         $"{password}1",
                         $"{fullName}1"
                     )
                )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName}1",
                     Password = $"{password}1"
                 }
             }));
        }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/identity/update", "Update", "IdentityController"));

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_with_fake_authorization_header_should_fail(
         string fullName,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
         string email,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
         string password)
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Put)
                .WithHeaderAuthorization($"{Guid.NewGuid()}.{Guid.NewGuid()}.{Guid.NewGuid()}")
                .WithLocation("api/v1.0/identity/update")
                .WithJsonBody(
                     string.Format(@"{{""user"":{{""password"":""{0}"",""username"":""{1}""}}}}",
                         $"{password}1",
                         $"{fullName}1"
                     )
                )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName}1",
                     Password = $"{password}1"
                 }
             }));
        }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/identity/update", "Update", "IdentityController"));

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_with_incorrect_authorization_header_key_should_fail(
         string fullName,
         string email,
         string password)
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Put)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithIncorrectKey(email, 1))
                .WithLocation("api/v1.0/identity/update")
                .WithJsonBody(
                     string.Format(@"{{""user"":{{""password"":""{0}"",""username"":""{1}""}}}}",
                         $"{password}1",
                         $"{fullName}1"
                     )
                )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName}1",
                     Password = $"{password}1"
                 }
             }));
        }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/identity/update", "Update", "IdentityController"));

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_with_expired_authorization_header_should_fail(
         string fullName,
         string email,
         string password)
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                 .WithMethod(HttpMethod.Put)
                 .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithExpiredToken(email, 1))
                 .WithLocation("api/v1.0/identity/update")
                 .WithJsonBody(
                      string.Format(@"{{""user"":{{""password"":""{0}"",""username"":""{1}""}}}}",
                          $"{password}1",
                          $"{fullName}1"
                      )
                 )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName}1",
                     Password = $"{password}1"
                 }
             }));
        }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/identity/update", "Update", "IdentityController"));

        [Theory]
        [InlineData("n", "ValidEmail@a.bcde", "p")]
        public void Update_user_with_bad_input_should_return_validation_errors(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Put)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/identity/update")
                .WithJsonBody(
                     string.Format(@"{{""user"":{{""password"":""{0}"",""username"":""{1}""}}}}",
                         $"{password}",
                         $"{fullName}"
                     )
                )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = fullName,
                     Password = password,
                 }
             }))
             .Which(controller => controller
                .WithData(StaticTestData.GetUsers(3, email, fullName, password)))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
           { "UserJson.Password", ["The length of 'User Json Password' must be at least 16 characters. You entered 1 characters."] },
           { "UserJson.FullName", ["The length of 'User Json Full Name' must be at least 2 characters. You entered 1 characters."] },
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_with_name_taken_should_fail(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Put)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/identity/update")
               .WithJsonBody(
                    string.Format(@"{{""user"":{{""password"":""{0}"",""username"":""{1}""}}}}",
                        $"{password}1",
                        $"{fullName}1"
                    )
               )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName}1",
                     Password = $"{password}1"
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetUsers(3, email, fullName, password)))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
           { "name_error", ["The user name has been taken."] }
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_without_role_should_fail(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Put)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/identity/update")
                .WithJsonBody(
                     string.Format(@"{{""user"":{{""password"":""{0}"",""username"":""{1}""}}}}",
                         $"{password}4",
                         $"{fullName}4"
                     )
                )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName}4",
                     Password = $"{password}4"
                 }
             }))
             .Which(controller => controller
                .WithData(StaticTestData.GetUsers(3, email, fullName, password)))
             .ShouldHave()
             .ActionAttributes(attrs => attrs
                 .RestrictingForHttpMethod(HttpMethod.Put)
                 .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
           { "is_in_role_error", ["Cannot find role Administrator"] }
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_without_data_should_fail(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Put)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/identity/update")
                .WithJsonBody(
                     string.Format(@"{{""user"":{{}}}}")
                )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = null,
                     Password = null,
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetUsers(3, email, fullName, password)))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
           { "no_data_error", ["There is no data to process."] }
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_with_incorrect_password_should_fail(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Put)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/identity/update")
               .WithJsonBody(
                    string.Format(@"{{""user"":{{""password"":""{0}""}}}}",
                        $"{password} "
                    )
               )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = null,
                     Password = $"{password} "
                 }
             }))
             .Which(controller => controller
                    .WithData(db => db
                         .WithEntities(entities => StaticTestData.GetUsersWithRole(
                             count: 3,
                             email: email,
                             userName: fullName,
                             password: password,
                             dbContext: entities))))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
          { "PasswordRequiresDigit", ["Password required upper and lower case letters, digits, and at least one special symbol."] },
          { "PasswordDeleted", ["The old password was deleted. You must provide a new password."] },
        });

        [Theory]
        [InlineData("good_username", "good@email.com", "123456789123456789")]
        public void Update_user_with_bad_password_should_fail(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Put)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/identity/update")
               .WithJsonBody(
                    string.Format(@"{{""user"":{{""password"":""{0}""}}}}",
                        $"{password}"
                    )
               )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = null,
                     Password = $"{password}"
                 }
             }))
             .Which(controller => controller
                  .WithData(db => db
                       .WithEntities(entities => StaticTestData.GetUsersWithRole(
                           count: 3,
                           email: email,
                           userName: fullName,
                           password: $"Aa_{password}",
                           dbContext: entities))))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
          { "PasswordRequiresNonAlphanumeric", ["Passwords must have at least one non alphanumeric character."] },
          { "PasswordRequiresLower", ["Passwords must have at least one lowercase ('a'-'z')."] },
          { "PasswordRequiresUpper",["Passwords must have at least one uppercase ('A'-'Z')."] },
          { "PasswordDeleted", ["The old password was deleted. You must provide a new password."] },
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Update_user_with_incorrect_user_name_should_fail(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Put)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/identity/update")
               .WithJsonBody(
                    string.Format(@"{{""user"":{{""username"":""{0}""}}}}",
                        $"{fullName} "
                    )
               )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName} ",
                     Password = null,
                 }
             }))
             .Which(controller => controller
                  .WithData(db => db
                       .WithEntities(entities => StaticTestData.GetUsersWithRole(
                           count: 3,
                           email: email,
                           userName: fullName,
                           password: password,
                           dbContext: entities))))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
          { "InvalidUserName", ["Username must contain letters and numbers."] },
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        //TODO: Test this in browser
        public void Update_user_with_malformated_data_should_fail(
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
         string fullName,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
         string email,
         string password)
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Put)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/identity/update")
                .WithJsonBody(
                 string.Format(@"{{""password"":""{0}""}}",
                     $"{password}1"
                 ))
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = null,
                     Password = null,
                 }
             }));
        }, string.Format(DifferenceException.Replace(Environment.NewLine, ""), "/api/v1.0/identity/update", "command", "UserUpdateCommand.UserJson"));

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        //In browser, it returns 404
        public void Update_user_with_incorrect_route_should_fail(
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
         string fullName,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
         string email,
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
         string password
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
         )
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                 .WithMethod(HttpMethod.Put)
                 .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                 .WithLocation("api/v1.0/identity/noroute")
                 .WithJsonBody(
                      string.Format(@"{{""user"":{{}}}}")
                 )
             )
             .To<IdentityController>(c => c.Update(new UserUpdateCommand
             {
                 UserJson = new()
                 {
                     FullName = null,
                     Password = null,
                 }
             }));
        }, string.Format(RouteCouldNotBeMachedException.Replace(Environment.NewLine, ""), "/api/v1.0/identity/noroute", "Update", "IdentityController"));
        #endregion

        #region Login Password
        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Login_with_password_should_return_success_with_token(
         string fullName,
         string email,
         string password)
        => MyMvc
        .Pipeline()
        .ShouldMap(request => request
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
                   .WithEntities(entities => StaticTestData.GetUsersWithRole(
                       count: 3,
                       email: email,
                       userName: fullName,
                       password: password,
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
        [MemberData(nameof(RegisterValidData))]
        public void Login_with_password_without_role_should_fail(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
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
               .WithData(StaticTestData.GetUsers(3, email, fullName, password)))
             .ShouldHave()
             .ActionAttributes(attrs => attrs
                 .RestrictingForHttpMethod(HttpMethod.Post)
                 .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
          { "is_in_role_error", ["Cannot find role Administrator"] }
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Login_with_password_no_data_should_fail(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/identity")
               .WithJsonBody(
                   string.Format(@"{{""user"":{{ }}}}")
               ))
             .To<IdentityController>(c => c.LoginPassword(new LoginPasswordCommand
             {
                 UserJson = new()
                 {
                     FullName = null,
                     Password = null,
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetUsers(3, email, fullName, password)))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
            { "no_data_error", ["There is no data to process."] }
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Login_with_password_with_incorrect_password_should_fail(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
               .WithLocation("api/v1.0/identity")
               .WithJsonBody(
                   string.Format(@"{{""user"":{{""password"":""{0}""}}}}",
                        $"{password}2"
                   )
               ))
             .To<IdentityController>(c => c.LoginPassword(new LoginPasswordCommand
             {
                 UserJson = new()
                 {
                     FullName = null,
                     Password = $"{password}2"
                 }
             }))
            .Which(controller => controller
                  .WithData(db => db
                       .WithEntities(entities => StaticTestData.GetUsersWithRole(
                           count: 3,
                           email: email,
                           userName: fullName,
                           password: password,
                           dbContext: entities))))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
          { "invalid_error", ["Invalid credentials."] }
        });
        #endregion

        #region Register
        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Register_user_should_return_success_with_token(
         string fullName,
         string email,
         string password)
        => MyMvc
        .Pipeline()
        .ShouldMap(request => request
           .WithMethod(HttpMethod.Post)
           .WithLocation("api/v1.0/identity/register")
           .WithJsonBody(
               string.Format(@"{{""user"":{{""email"":""{0}"",""password"":""{1}"",""username"":""{2}""}}}}",
                   $"{email}2",
                   $"{password}2",
                   $"{fullName}2"
               ))
        )
        .To<IdentityController>(c => c.Register(new UserRegisterCommand
        {
            UserJson = new()
            {
                FullName = $"{fullName}2",
                Email = $"{email}2",
                Password = $"{password}2"
            }
        }))
        .Which(controller => controller
           .WithData(StaticTestData.GetUsers(1, email, fullName, password)))
        .ShouldReturn()
        .ActionResult(result => result.Result(new UserResponseEnvelope
        {
            UserJson = new()
            {
                Email = $"{email}2",
                UserName = $"{fullName}2",
                Token = $"Token: {email}2",
            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes =>
        {
            Assert.Equal(4, attributes.Count());
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Register_user_with_name_taken_should_return_validation_errors(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithLocation("api/v1.0/identity/register")
                .WithJsonBody(
                    string.Format(@"{{""user"":{{""email"":""{0}"",""password"":""{1}"",""username"":""{2}""}}}}",
                        $"{email}4",
                        $"{password}4",
                        $"{fullName}2"
                    ))
             )
             .To<IdentityController>(c => c.Register(new UserRegisterCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName}2",
                     Email = $"{email}4",
                     Password = $"{password}4"
                 }
             }))
             .Which(controller => controller
                .WithData(StaticTestData.GetUsers(3, email, fullName, password)))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
          { "name_error", ["The user name has been taken."] },
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Register_user_with_email_taken_should_return_validation_errors(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithLocation("api/v1.0/identity/register")
                .WithJsonBody(
                    string.Format(@"{{""user"":{{""email"":""{0}"",""password"":""{1}"",""username"":""{2}""}}}}",
                        $"{email}2",
                        $"{password}4",
                        $"{fullName}4"
                    ))
             )
             .To<IdentityController>(c => c.Register(new UserRegisterCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName}4",
                     Email = $"{email}2",
                     Password = $"{password}4"
                 }
             }))
             .Which(controller => controller
                .WithData(StaticTestData.GetUsers(3, email, fullName, password)))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
          { "email_error", ["The email has been taken."] },
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Register_user_with_icorrect_user_name_should_return_validation_errors(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithLocation("api/v1.0/identity/register")
                .WithJsonBody(
                    string.Format(@"{{""user"":{{""email"":""{0}"",""password"":""{1}"",""username"":""{2}""}}}}",
                        $"{email}2",
                        $"{password}2",
                        $"{fullName} "
                    ))
             )
             .To<IdentityController>(c => c.Register(new UserRegisterCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName} ",
                     Email = $"{email}2",
                     Password = $"{password}2",
                 }
             }))
             .Which(controller => controller
                .WithData(StaticTestData.GetUsers(1, email, fullName, password)))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
         { "InvalidUserName", ["Username must contain letters and numbers."] },
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Register_user_with_icorrect_passord_should_return_validation_errors(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithLocation("api/v1.0/identity/register")
                .WithJsonBody(
                    string.Format(@"{{""user"":{{""email"":""{0}"",""password"":""{1}"",""username"":""{2}""}}}}",
                        $"{email}2",
                        $"{password} ",
                        $"{fullName}2"
                    ))
             )
             .To<IdentityController>(c => c.Register(new UserRegisterCommand
             {
                 UserJson = new()
                 {
                     FullName = $"{fullName}2",
                     Email = $"{email}2",
                     Password = $"{password} "
                 }
             }))
             .Which(controller => controller
                .WithData(StaticTestData.GetUsers(1, email, fullName, password)))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
         { "PasswordRequiresDigit", ["Password required upper and lower case letters, digits, and at least one special symbol."] },
        });

        [Theory]
        [InlineData("n", "e@", "p")]
        public void Register_user_with_bad_input_should_return_validation_errors(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                 .WithMethod(HttpMethod.Post)
                 .WithLocation("api/v1.0/identity/register")
                 .WithJsonBody(
                     string.Format(@"{{""user"":{{""email"":""{0}"",""password"":""{1}"",""username"":""{2}""}}}}",
                         email,
                         password,
                         fullName
                     ))
              )
             .To<IdentityController>(c => c.Register(new UserRegisterCommand
             {
                 UserJson = new()
                 {
                     FullName = fullName,
                     Email = email,
                     Password = password,
                 }
             }))
             .Which(controller => controller
                .WithData(StaticTestData.GetUsers(1, email, fullName, password)))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
           { "UserJson.Password", ["The length of 'User Json Password' must be at least 16 characters. You entered 1 characters."] },
           { "UserJson.FullName", ["The length of 'User Json Full Name' must be at least 2 characters. You entered 1 characters."] },
           { "UserJson.Email", [
               "The length of 'User Json Email' must be at least 3 characters. You entered 2 characters.",
               "'User Json Email' is not a valid email address."]
           },
        });
        #endregion 

        #region Login
        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Login_user_should_return_success_with_token(
         string fullName,
         string email,
         string password)
        => MyMvc
        .Pipeline()
        .ShouldMap(request => request
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
          .WithData(StaticTestData.GetUsers(1, email, fullName, password)))
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
        [MemberData(nameof(RegisterValidData))]
        //In browser, it returns 422 with a validation error: userJson.Email 'User Json Email' must not be empty.
        public void Login_user_with_malformated_data_should_fail(
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
         string fullName,
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
         string email,
         string password)
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithLocation("api/v1.0/identity/login")
               .WithJsonBody(
                   string.Format(@"{{""user"":{{""em"":""{0}"",""pass"":""{1}""}}}}",
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
             }));
        }, string.Format(DifferenceException.Replace(Environment.NewLine, ""), "/api/v1.0/identity/login", "command", "UserLoginCommand.UserJson.Email"));

        [Theory]
        [InlineData("ValidUserName", "e@", "p")]
        public void Login_user_with_bad_input_should_return_validation_errors(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithLocation("api/v1.0/identity/login")
                .WithJsonBody(
                    string.Format(@"{{""user"":{{""email"":""{0}"",""password"":""{1}""}}}}",
                        email,
                        password
                    ))
             )
             .To<IdentityController>(c => c.Login(new UserLoginCommand
             {
                 UserJson = new()
                 {
                     Email = email,
                     Password = password
                 }
             }))
             .Which(controller => controller
                .WithData(StaticTestData.GetUsers(1, "ValidEmail@a.bcde", fullName, "validPassword1234")))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
           { "UserJson.Password", ["The length of 'User Json Password' must be at least 16 characters. You entered 1 characters."] },
           { "UserJson.Email", [
               "The length of 'User Json Email' must be at least 3 characters. You entered 2 characters.",
               "'User Json Email' is not a valid email address."]
           },
        });

        [Theory]
        [InlineData("ValidEmail@a.bcd", "!ValidPassWord111!")]
        public void Login_user_with_invalid_credintial_should_return_validation_errors(
          string email,
          string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithLocation("api/v1.0/identity/login")
               .WithJsonBody(
                   string.Format(@"{{""user"":{{""email"":""{0}"",""password"":""{1}""}}}}",
                       $"{email}1",
                       $"{password}1"
                   )
               ))
             .To<IdentityController>(c => c.Login(new UserLoginCommand
             {
                 UserJson = new()
                 {
                     Email = $"{email}1",
                     Password = $"{password}1"
                 }
             }))
             .Which(controller => controller
               .WithData(StaticTestData.GetUsers(1, "email@email.email", "SomeFullName", "somepassword1234")))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
             { "invalid_error", ["Invalid credentials."] }
        });
        #endregion
        
        #region Get Profile
        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Get_profile_should_return_success(
         string fullName,
         string email,
         string password)
        => MyMvc
        .Pipeline()
        .ShouldMap(request => request
          .WithMethod(HttpMethod.Get)
          .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
          .WithLocation($"api/v1.0/profile/{fullName}1")
        )
        .To<ProfileController>(c => c.Index($"{fullName}1"))
        .Which(controller => controller
          .WithData(StaticTestData.GetUsers(1, email, fullName, password)))
        .ShouldReturn()
        .ActionResult(result => result.Result(new ProfileResponseEnvelope
        {
            ProfileJson = new ProfileResponseModel(
                $"{fullName}1",
                $"{fullName}1",
                $"{fullName}1"
            )
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes =>
        {
            Assert.Equal(6, attributes.Count());
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        public void Get_profile_with_icorrect_username_should_return_validation_errors(
         string fullName,
         string email,
         string password)
        => AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Get)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/profile/incorrect_user_name")
             )
             .To<ProfileController>(c => c.Index("incorrect_user_name"))
             .Which(controller => controller
               .WithData(StaticTestData.GetUsers(1, email, fullName, password)))
             .ShouldReturn();
        }, new Dictionary<string, string[]>
        {
            { "profile_error", ["Cannot find user profile."] }
        });

        [Theory]
        [MemberData(nameof(RegisterValidData))]
        //In browser, same error as above
        public void Get_profile_with_icorrect_username_format_should_return_validation_errors(
         string fullName,
         string email,
         string password)
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.InvalidHttpRequestException>(
        () =>
        {
            MyMvc
             .Pipeline()
             .ShouldMap(request => request
                .WithMethod(HttpMethod.Get)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/profile/incorrect_user format")
             )
             .To<ProfileController>(c => c.Index("incorrect_user format"))
             .Which(controller => controller
               .WithData(StaticTestData.GetUsers(1, email, fullName, password)))
             .ShouldReturn();
        }, string.Format(HttpRequestException.Replace(Environment.NewLine, ""), "api/v1.0/profile/incorrect_user format"));
        #endregion
    }
}