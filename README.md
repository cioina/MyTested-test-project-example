# MyTested-test-project-example

## Introduction

The compiled code of our .NET Core 8 application is on [our GitHub repository](https://github.com/cioina/cioina.azurewebsites.net). For this test project, which is part our application, we will use [MyTested](https://github.com/ivaylokenov/MyTested.AspNetCore.Mvc) - a well-known library for testing ASP.NET Core MVC. Here, we adapted the library to work with .NET Core 8 and API controllers with Bearer Header Authorization based on JWT token implementation provided by .NET Core. Our .NET Core 8 project is based on [BookStore](https://github.com/kalintsenkov/BookStore) repository and adapted to work with MyTested library.

## MyTested Library Out of The Box

I found out about MyTested for the first time from [BlazorShop](https://github.com/kalintsenkov/BlazorShop/blob/master/src/BlazorShop.Tests/Controllers/AddressesControllerTests.cs)  and [CarRentalSystem](https://github.com/kalintsenkov/CarRentalSystem/blob/master/src/Startup/Specs/IdentityController.Specs.cs) repositories. At the same time, I found out about `Jwt Authentication` implementation from [BlazorShop](https://github.com/kalintsenkov/BlazorShop/blob/master/src/BlazorShop.Web/Server/Infrastructure/Extensions/ServiceCollectionExtensions.cs) and [RealWorld](https://github.com/gothinkster/aspnetcore-realworld-example-app/blob/master/src/Conduit/StartupExtensions.cs) repositories. Both `Jwt Authentication` implementations did not work with original [MyTested](https://github.com/ivaylokenov/MyTested.AspNetCore.Mvc) library, so I decided to find out why. I do not know who engineered MyTested, but I was not able to fully understand how it works. I was able only to add some small pieces of code to make MyTested and my own `Jwt Authentication` implementation work and not to break any original MyTested tests. But, what MyTested can do out of the box? The best answer is in [MusicStore](https://github.com/ivaylokenov/MyTested.AspNetCore.Mvc/tree/development/samples/MusicStore/MusicStore.Test) testing project. For the API controller, [here](https://github.com/cioina/MyTested-test-project-example/blob/main/src/BlogAngular.Test/Routing/FrontEndRouteTest.cs) is an example:

```csharp
namespace BlogAngular.Test.Routing;

using Application.Common.Version;
using MyTested.AspNetCore.Mvc;
using Web.Features;
using Xunit;

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
```

## Basic API Controller Testing

By basic API controller testing, we mean at least one test per CRUD concept.
[Here](https://github.com/cioina/MyTested-test-project-example/blob/main/src/BlogAngular.Test/Routing/TagsControllerRouteTest.cs) is an example:

- `Create_tag_should_return_success_with_data`- Create
- `Listing_tags_without_url_parameters_should_return_success_with_all_tags`- Read
- `Edit_tag_should_return_success_with_data`- Update
- `Delete_tag_should_return_success_with_tag_id` - Delete

A particular change we made to MyTested is adding the possibility of testing data validation. In fact, now, we can realize all following tests: [BookStore](https://github.com/kalintsenkov/BookStore/blob/main/src/Server/BookStore.Application/Catalog/Authors/Commands/Create/AuthorCreateCommandValidator.Specs.cs), [RealWorld](https://github.com/gothinkster/aspnetcore-realworld-example-app/blob/master/tests/Conduit.IntegrationTests/Features/Articles/EditTests.cs), [CleanArchitecture1](https://github.com/jasontaylordev/CleanArchitecture/blob/main/tests/Application.UnitTests/Common/Exceptions/ValidationExceptionTests.cs), and [CleanArchitecture2](https://github.com/jasontaylordev/CleanArchitecture/blob/main/tests/Web.AcceptanceTests/StepDefinitions/LoginStepDefinitions.cs) in a set of beautiful tests. [Here](https://github.com/cioina/MyTested-test-project-example/blob/main/src/BlogAngular.Test/Routing/TagsControllerRouteTest.cs) are examples of testing data validation using modified version of MyTested library:

- `Create_tag_with_one_char_should_return_validation_error`- Creates tag name length bellow allowed by database constraint
- `Create_tag_with_max_chars_should_return_validation_error`- Creates tag name length above allowed by database constraint
- `Edit_tag_with_one_char_should_return_validation_error`- Updates tag name length bellow allowed by database constraint
- `Edit_tag_with_max_chars_should_return_validation_error` - Updates tag name length above allowed by database constraint

Our validation implementation is based mostly on [BookStore](https://github.com/kalintsenkov/BookStore/blob/main/src/Server/BookStore.Application/Catalog/Authors/Commands/Common/AuthorCommandValidator.cs). One useful technique to validate unique data comes from [Conduit](https://github.com/gothinkster/aspnetcore-realworld-example-app/blob/master/src/Conduit/Features/Users/Create.cs) and [CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture/blob/main/src/Application/TodoLists/Commands/UpdateTodoList/UpdateTodoListCommandValidator.cs)

- `Create_tag_with_same_name_should_fail_with_validation_error`- Creates tag name length bellow allowed by database constraint
- `Edit_tag_with_same_name_should_fail_with_validation_error`- Creates tag name length above allowed by database constraint
- `Edit_same_tag_with_same_name_should_return_success_with_data`- Updates tag name length bellow allowed by database constraint

In [our application](https://github.com/cioina/cioina.azurewebsites.net), any `MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException` will return 422 with JSON string similar to this:

```json
{
   "TagJson.Title":  ["The length of 'Tag Json Title' must be 420 characters or fewer. You entered 421 characters."]
}
```

That represents a standard validation message from FluentValidation library which can be customized.

## MyTested Library Limitations

We applied modified version of MyTested library to three popular GitHub repositories: [BookStore](https://github.com/kalintsenkov/BookStore/tree/main/src/Server), [RealWorld](https://github.com/gothinkster/aspnetcore-realworld-example-app/tree/master/src/Conduit), and [CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture/tree/main/src). Our quick investigation shows that BookStore can be configurated to work 100% with MyTested while RealWorld works only with [anonymous controllers](https://github.com/gothinkster/aspnetcore-realworld-example-app/blob/master/src/Conduit/Features/Tags/TagsController.cs) and CleanArchitecture does not work at all.
