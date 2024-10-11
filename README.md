# MyTested-test-project-example

## Introduction

The compiled code of our .NET Core 8 application is on [our GitHub repository](https://github.com/cioina/cioina.azurewebsites.net). For this test project, which is part our application, we will use [MyTested](https://github.com/ivaylokenov/MyTested.AspNetCore.Mvc) - a well-known library for testing ASP.NET Core MVC. Here, we adapted the library to work with .NET Core 8 and API controllers with Bearer Header Authorization based on JWT token implementation provided by .NET Core. Our .NET Core 8 project is based on [BookStore](https://github.com/kalintsenkov/BookStore) repository and adapted to work with MyTested library.

## MyTested Library Out of The Box

I found out about MyTested for the first time from [BlazorShop](https://github.com/kalintsenkov/BlazorShop/blob/master/src/BlazorShop.Tests/Controllers/AddressesControllerTests.cs) repository. At the same time, I found out about `Jwt Authentication` implementation from same [BlazorShop](https://github.com/kalintsenkov/BlazorShop/blob/master/src/BlazorShop.Web/Server/Infrastructure/Extensions/ServiceCollectionExtensions.cs) repository and from [aspnetcore-realworld-example](https://github.com/gothinkster/aspnetcore-realworld-example-app/blob/master/src/Conduit/StartupExtensions.cs) repository. Both `Jwt Authentication` implementations did not work with original [MyTested](https://github.com/ivaylokenov/MyTested.AspNetCore.Mvc) library, so I decided to find out why. I do not know who engineered MyTested, but I was not able to fully understand how it works. I was able only to add some small pieces of code to make MyTested and my own `Jwt Authentication` implementation work and not to break any original MyTested tests. But, what MyTested can do out of the box? The best answer is in [MusicStore](https://github.com/ivaylokenov/MyTested.AspNetCore.Mvc/tree/development/samples/MusicStore/MusicStore.Test) testing project. For the API controller, [here](https://github.com/cioina/MyTested-test-project-example/blob/main/src/BlogAngular.Test/Test/Routing/FrontEndRouteTest.cs) is an example:

```csharp
#if DEBUG
using BlogAngular.Application.Common.Version;
using BlogAngular.Web.Features;
using MyTested.AspNetCore.Mvc;
using Xunit;

namespace BlogAngular.Test.Routing
{
    public class FrontEndRouteTest
    {
        [Fact]
        public void VersionShouldBeRouted()
            => MyMvc
            .Pipeline()
            .ShouldMap(request => request
                .WithMethod(HttpMethod.Get)
                .WithLocation("api/v1.0/version"))
            .To<VersionController>(c => c.Index())
            .Which()
            .ShouldReturn()
            .ActionResult(result => result.Result(new VersionResponseEnvelope
            {
                VersionJson = new VersionResponseModel()
            }));
    }
}
#endif

```

## Basic API Controller Testing

By basic API controller testing, we mean at least one test per CRUD concept.
[Here](https://github.com/cioina/MyTested-test-project-example/blob/main/src/BlogAngular.Test/Test/Routing/TagsControllerRouteTest.cs) is an example:

- `Create_tag_should_return_success_with_data`- Create
- `Listing_tags_without_url_parameters_should_return_success_with_all_tags`- Read
- `Edit_tag_should_return_success_with_data`- Update
- `Delete_tag_should_return_success_with_tag_id` - Delete

## Data Validation with FluentValidation Library

A particular change we made to MyTested is adding the possibility of testing data validation. In fact, now, we can implement all following tests: [BookStore](https://github.com/kalintsenkov/BookStore/blob/main/src/Server/BookStore.Application/Catalog/Authors/Commands/Create/AuthorCreateCommandValidator.Specs.cs), [RealWorld](https://github.com/gothinkster/aspnetcore-realworld-example-app/blob/master/tests/Conduit.IntegrationTests/Features/Articles/EditTests.cs), [CleanArchitecture1](https://github.com/jasontaylordev/CleanArchitecture/blob/main/tests/Application.UnitTests/Common/Exceptions/ValidationExceptionTests.cs), and [CleanArchitecture2](https://github.com/jasontaylordev/CleanArchitecture/blob/main/tests/Web.AcceptanceTests/StepDefinitions/LoginStepDefinitions.cs) in a set of beautiful tests. [Here](https://github.com/cioina/MyTested-test-project-example/blob/main/src/BlogAngular.Test/Test/Routing/TagsControllerRouteTest.cs) are examples of testing data validation using modified version of MyTested library:

- `Create_tag_with_one_char_should_return_validation_error`- Creates tag name length bellow allowed by database constraint
- `Create_tag_with_max_chars_should_return_validation_error`- Creates tag name length above allowed by database constraint
- `Edit_tag_with_one_char_should_return_validation_error`- Updates tag name length bellow allowed by database constraint
- `Edit_tag_with_max_chars_should_return_validation_error` - Updates tag name length above allowed by database constraint

Our validation implementation is based mostly on [BookStore](https://github.com/kalintsenkov/BookStore/blob/main/src/Server/BookStore.Application/Catalog/Authors/Commands/Common/AuthorCommandValidator.cs). One useful technique to validate unique data comes from [Conduit](https://github.com/gothinkster/aspnetcore-realworld-example-app/blob/master/src/Conduit/Features/Users/Create.cs) and [CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture/blob/main/src/Application/TodoLists/Commands/UpdateTodoList/UpdateTodoListCommandValidator.cs). Following are [three tests](https://github.com/cioina/MyTested-test-project-example/blob/main/src/BlogAngular.Test/Test/Routing/TagsControllerRouteTest.cs) with the constraint that the tag name is unique:

- `Create_tag_with_same_name_should_fail_with_validation_error`- Creates tag with name when the name has already taken.
- `Edit_tag_with_same_name_should_fail_with_validation_error`- Updates tag name when the name has already taken.
- `Edit_same_tag_with_same_name_should_return_success_with_data`- Updates tag name when the name did not change.

In [our application](https://github.com/cioina/cioina.azurewebsites.net), any `MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException` will return 422 with JSON string similar to this:

```json
{
   "TagJson.Title":  ["The length of 'Tag Json Title' must be 420 characters or fewer. You entered 421 characters."]
}
```

That represents a standard validation message from `FluentValidation` library which can be customized.

## Exception Testing

In our application, we use `Ardalis.GuardClauses.NotFoundException` instead of [BaseDomainException](https://github.com/kalintsenkov/BookStore/blob/main/src/Server/BookStore.Domain/Common/BaseDomainException.cs). In addition, we use [ValidationExceptionHandlerMiddleware](https://github.com/cioina/MyTested-test-project-example/blob/main/src/BlogAngular.Web/Web/Middleware/ValidationExceptionHandlerMiddleware.cs) to intercept all validation exceptions that return `HttpStatusCode.UnprocessableEntity`(422). Unfortunately, MyTested does not work with the middleware concept. But, we can use `MyTested.AspNetCore.Mvc.Exceptions.InvocationAssertionException` and [FromNotFoundException](https://github.com/cioina/MyTested-test-project-example/blob/main/src/BlogAngular.Test/Test/Routing/IdentityControllerRouteTest.cs) to test against two common exceptions:

- `Edit_tag_with_wrong_id_should_fail`- The tag with the specified id does not exist in the database.
- `Update_user_with_malformated_data_should_fail`- The webserver cannot create the object from the json data request.

## Identity Controller Testing

When it comes to JWT authorization, a big amount of testing consists in testing for invalid JWT tokens:

- `Update_user_without_authorization_header_should_fail`- tests when JWT token is absent
- `Update_user_with_altered_authorization_header_should_fail`- tests when to a valid JWT token is added one character
- `Update_user_with_malformated_authorization_header_should_fail`- tests when JWT token has format `a.b`
- `Update_user_with_fake_authorization_header_should_fail`- tests when JWT token has correct format `a.b.c` but random characters
- `Update_user_with_incorrect_authorization_header_key_should_fail`- tests when JWT token is valid but was encrypted with a different key
- `Update_user_with_expired_authorization_header_should_fail`- tests when a valid JWT token was expired

These are the most common case scenarios to test against an invalid JWT token and must be done just for one controller!
MyTested cannot catch 401 error code directly. We found a workaround by using [HeaderAuthorizationException](https://github.com/cioina/MyTested-test-project-example/blob/main/src/BlogAngular.Test/Test/Routing/IdentityControllerRouteTest.cs)
The full source code for the .NET Core `IdentityService` implementation can be found [here](https://cioina.azurewebsites.net/articles/dotnet-core-testing#h-f5dcf3f1743665e0100f5e709e3971acd67ebe05)

## MyTested Library Limitations

We applied modified version of MyTested library to three popular GitHub repositories: [BookStore](https://github.com/kalintsenkov/BookStore/tree/main/src/Server), [RealWorld](https://github.com/gothinkster/aspnetcore-realworld-example-app/tree/master/src/Conduit), and [CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture/tree/main/src). Our quick investigation shows that BookStore can be configurated to work 100% with MyTested while RealWorld works only with [anonymous controllers](https://github.com/gothinkster/aspnetcore-realworld-example-app/blob/master/src/Conduit/Features/Tags/TagsController.cs) and CleanArchitecture does not work at all.
The full test project source code can be found on [our GitHub repository]( https://github.com/cioina/MyTested-test-project-example/tree/main/src/BlogAngular.Test/Test).

## Credits

- [Ivaylo Kenov](https://github.com/ivaylokenov)
- [Kalin Tsenkov](https://github.com/kalintsenkov)
- [Steve Smith](https://github.com/ardalis)
- [Jason Taylor](https://github.com/jasontaylordev)
- [Stefan Prodan](https://github.com/stefanprodan)
- [Mark Cilia Vincenti](https://github.com/MarkCiliaVincenti)
- [Jimmy Bogard](https://github.com/jbogard)
- [Ben Morris](https://github.com/BenMorris)
