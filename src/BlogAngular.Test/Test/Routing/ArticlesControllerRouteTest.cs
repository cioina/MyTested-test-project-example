using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BlogAngular.Application.Blog.Articles.Commands.Common;
using BlogAngular.Application.Blog.ArticleTags.Commands.Create;
using BlogAngular.Application.Blog.ArticleTags.Commands.Listing;
using BlogAngular.Application.Blog.Common;
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
    public class ArticlesControllerRouteTest
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
        #region Link Tags
        [Theory]
        [MemberData(nameof(ValidData))]
        public void LinkTagsToArticleShouldReturnSuccessWithId(
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
          .WithLocation("api/v1.0/articles/linktags")
          .WithJsonBody(
                string.Format(CultureInfo.InvariantCulture, @"{{""articleTags"":{{""articleId"": 4, ""tags"": [{0},{1},{2}]}}}}", 1, 2, 3)
          )
        )
        .To<ArticlesController>(c => c.LinkTags(new ArticleTagsCreateCommand
        {
            ArticleTagsJson = new()
            {
                ArticleId = 4,
                Tags = Enumerable
                             .Range(1, 3)
                             .Select(i => i).ToList(),
            }
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
            .RestrictingForHttpMethod(HttpMethod.Post)
            .RestrictingForAuthorizedRequests())
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(1))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void LinkEmptyTagListToArticleShouldReturnSuccessWithId(
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
          .WithLocation("api/v1.0/articles/linktags")
          .WithJsonBody(
                string.Format(CultureInfo.InvariantCulture, @"{{""articleTags"":{{""articleId"": 4 }}}}")
          )
        )
        .To<ArticlesController>(c => c.LinkTags(new ArticleTagsCreateCommand
        {
            ArticleTagsJson = new()
            {
                ArticleId = 4,
                Tags = null,
            }
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
            .RestrictingForHttpMethod(HttpMethod.Post)
            .RestrictingForAuthorizedRequests())
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(0))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void LinkTagsToArticleWithExpiredAuthorizationHeaderShouldFail(
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
               .WithMethod(HttpMethod.Post)
               .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithExpiredToken(email, 1))
               .WithLocation("api/v1.0/articles/linktags")
               .WithJsonBody(
                     string.Format(CultureInfo.InvariantCulture, @"{{""articleTags"":{{""articleId"": 4, ""tags"": [{0},{1},{2}]}}}}", 1, 2, 3)
               )
             )
             .To<ArticlesController>(c => c.LinkTags(new ArticleTagsCreateCommand
             {
                 ArticleTagsJson = new()
                 {
                     ArticleId = 4,
                     Tags = Enumerable
                          .Range(1, 3)
                          .Select(i => i).ToList(),
                 }
             })), string.Format(CultureInfo.InvariantCulture, HEADER_AUTHORIZATION_EXCEPTION.Replace(Environment.NewLine, ""), "/api/v1.0/articles/linktags", "LinkTags", "ArticlesController"));

        [Fact]
        public void LinkTagsToArticleWithoutAuthorizationHeaderShouldFail()
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () => MyMvc
             .Pipeline()
             .ShouldMap(request => request
               .WithMethod(HttpMethod.Post)
               .WithLocation("api/v1.0/articles/linktags")
               .WithJsonBody(
                     string.Format(CultureInfo.InvariantCulture, @"{{""articleTags"":{{""articleId"": 4, ""tags"": [{0},{1},{2}]}}}}", 1, 2, 3)
               )
             )
             .To<ArticlesController>(c => c.LinkTags(new ArticleTagsCreateCommand
             {
                 ArticleTagsJson = new()
                 {
                     ArticleId = 4,
                     Tags = Enumerable
                          .Range(1, 3)
                          .Select(i => i).ToList(),
                 }
             })), string.Format(CultureInfo.InvariantCulture, HEADER_AUTHORIZATION_EXCEPTION.Replace(Environment.NewLine, ""), "/api/v1.0/articles/linktags", "LinkTags", "ArticlesController"));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void LinkTagsToArticleWithIncorrectTagShouldFail(
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
                 .WithMethod(HttpMethod.Post)
                 .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                 .WithLocation("api/v1.0/articles/linktags")
                 .WithJsonBody(
                       string.Format(CultureInfo.InvariantCulture, @"{{""articleTags"":{{""articleId"": 1, ""tags"": [{0},{1},{2}]}}}}", 1, 2, 3)
                 )
             )
             .To<ArticlesController>(c => c.LinkTags(new ArticleTagsCreateCommand
             {
                 ArticleTagsJson = new()
                 {
                     ArticleId = 1,
                     Tags = Enumerable
                            .Range(1, 3)
                            .Select(i => i).ToList(),
                 }
             }))
             .Which(controller => controller
                 .WithData(StaticTestData.GetArticlesTagsUsers(2,
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
             .ShouldReturn(), string.Format(CultureInfo.InvariantCulture, FROM_NOT_FOUND_EXCEPTION.Replace(Environment.NewLine, ""), "LinkTags", "ArticlesController", "NotFoundException", "tag", 3));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void LinkTagsToArticleWithIncorrectArticleShouldFail(
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
                 .WithMethod(HttpMethod.Post)
                 .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                 .WithLocation("api/v1.0/articles/linktags")
                 .WithJsonBody(
                       string.Format(CultureInfo.InvariantCulture, @"{{""articleTags"":{{""articleId"": 6, ""tags"": [{0},{1},{2}]}}}}", 1, 2, 3)
                 )
             )
             .To<ArticlesController>(c => c.LinkTags(new ArticleTagsCreateCommand
             {
                 ArticleTagsJson = new()
                 {
                     ArticleId = 6,
                     Tags = Enumerable
                            .Range(1, 3)
                            .Select(i => i).ToList(),
                 }
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
                 .RestrictingForHttpMethod(HttpMethod.Post)
                 .RestrictingForAuthorizedRequests())
             .AndAlso()
             .ShouldReturn(), string.Format(CultureInfo.InvariantCulture, FROM_NOT_FOUND_EXCEPTION.Replace(Environment.NewLine, ""), "LinkTags", "ArticlesController", "NotFoundException", "article", 6));
        #endregion

        #region Listing Articles
        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesContainsTitleAscendingWithTagFilterShouldReturnSuccessWithArticleList(
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
          .WithLocation("api/v1.0/articles")
          .WithJsonBody(
                string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0, ""tags"": [{0},{1},{2}], ""published"": true, ""createdAtAsc"": true, ""title"": ""{3}""}}}}", 1, 2, 3, $"{title}1")
          )
        )
        .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
        {
            ArticleTagsJson = new()
            {
                CreatedAtAsc = true,
                Published = true,
                Title = $"{title}1",
                Tags = Enumerable
                       .Range(1, 3)
                       .Select(i => i).ToList(),
                Limit = 4,
                Offset = 0,
            }
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
                 true)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticlesResponseEnvelope
        {
            Total = 1,
            Models = [.. Enumerable
             .Range(1, 1)
             .Select(i => new ArticleResponseModel
                 {
                     Id = i,
                     Title = $"{title}{i}",
                     Slug = $"{slug}{i}",
                     Description = $"{description}{i}",
                     Published = true,
                     CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
                     TagList = [.. Enumerable
                                .Range(1, 3)
                                .Select(i =>
                                {
                                    var r = 0;
                                    switch (i)
                                    {
                                        case 1:
                                            r = 5;
                                            break;
                                        case 2:
                                            r = 4;
                                            break;
                                        case 3:
                                            r = 3;
                                            break;
                                    }
                                    return r;
                                })],

                 })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesAscendingWithTagFilterShouldReturnSuccessWithArticleList(
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
          .WithLocation("api/v1.0/articles")
          .WithJsonBody(
                string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0, ""tags"": [{0},{1},{2}], ""published"": true, ""createdAtAsc"": true}}}}", 1, 2, 3)
          )
        )
        .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
        {
            ArticleTagsJson = new()
            {
                CreatedAtAsc = true,
                Published = true,
                Tags = Enumerable
                       .Range(1, 3)
                       .Select(i => i).ToList(),
                Limit = 4,
                Offset = 0,
            }
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
                 true)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticlesResponseEnvelope
        {
            Total = 4,
            Models = [.. Enumerable
             .Range(1, 4)
             .Select(i => new ArticleResponseModel
                 {
                     Id = i,
                     Title = $"{title}{i}",
                     Slug = $"{slug}{i}",
                     Description = $"{description}{i}",
                     Published = true,
                     CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
                     TagList = [.. Enumerable
                                .Range(1, 3)
                                .Select(i =>
                                {
                                    var r = 0;
                                    switch (i)
                                    {
                                        case 1:
                                            r = 5;
                                            break;
                                        case 2:
                                            r = 4;
                                            break;
                                        case 3:
                                            r = 3;
                                            break;
                                    }
                                    return r;
                                })],

                 })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesDescendingWithTagFilterShouldReturnSuccessWithArticleList(
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
          .WithLocation("api/v1.0/articles")
          .WithJsonBody(
                string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0, ""tags"": [{0},{1},{2}], ""published"": true}}}}", 1, 2, 3)
          )
        )
        .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
        {
            ArticleTagsJson = new()
            {
                CreatedAtAsc = null,
                Published = true,
                Tags = Enumerable
                       .Range(1, 3)
                       .Select(i => i).ToList(),
                Limit = 4,
                Offset = 0,
            }
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
                 true)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticlesResponseEnvelope
        {
            Total = 4,
            Models = [.. Enumerable
           .Range(1, 4)
           .Select(i =>
           {
               var j = 6 - i - 1;
               return new ArticleResponseModel
               {
                   Id = j,
                   Title = $"{title}{j}",
                   Slug = $"{slug}{j}",
                   Description = $"{description}{j}",
                   Published = true,
                   CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(j))),
                   TagList = [.. Enumerable
                              .Range(1, 3)
                              .Select(i =>
                              {
                                  var r = 0;
                                  switch (i)
                                  {
                                      case 1:
                                          r = 5;
                                          break;
                                      case 2:
                                          r = 4;
                                          break;
                                      case 3:
                                          r = 3;
                                          break;
                                  }
                                  return r;
                              })],

               };
           })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesWithCaseSensitiveTitleShouldReturnSuccessWithEmpty(
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
          .WithLocation("api/v1.0/articles")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0, ""title"": ""T1""  }}}}")
          )
        )
        .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
        {
            ArticleTagsJson = new()
            {
                Title = "T1",
                Limit = 4,
                Offset = 0,
            }
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
                 true)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticlesResponseEnvelope
        {
            Total = 0,
            Models = [],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesNotPublishedAscendingWithoutTagFilterShouldReturnSuccessWithEmpty(
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
          .WithLocation("api/v1.0/articles")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0, ""published"": false, ""createdAtAsc"": true }}}}")
          )
        )
        .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
        {
            ArticleTagsJson = new()
            {
                CreatedAtAsc = true,
                Published = false,
                Tags = null,
                Limit = 4,
                Offset = 0,
            }
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
                 true)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticlesResponseEnvelope
        {
            Total = 0,
            Models = [],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesAscendingWithoutTagFilterShouldReturnSuccessWithAllArticles(
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
          .WithLocation("api/v1.0/articles")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0, ""published"": true, ""createdAtAsc"": true }}}}")
          )
        )
        .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
        {
            ArticleTagsJson = new()
            {
                CreatedAtAsc = true,
                Published = true,
                Tags = null,
                Limit = 4,
                Offset = 0,
            }
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
                 true)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticlesResponseEnvelope
        {
            Total = 5,
            Models =
              [.. Enumerable
             .Range(1, 4)
             .Select(i => new ArticleResponseModel
                 {
                     Id = i,
                     Title = $"{title}{i}",
                     Slug = $"{slug}{i}",
                     Description = $"{description}{i}",
                     Published = true,
                     CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
                 })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesContainsTitleAscendingWithoutTagFilterShouldReturnSuccessWithAllArticles(
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
          .WithLocation("api/v1.0/articles")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0, ""published"": true, ""createdAtAsc"": true, ""title"": ""{0}"" }}}}", $"{title}1")
          )
        )
        .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
        {
            ArticleTagsJson = new()
            {
                CreatedAtAsc = true,
                Published = true,
                Title = $"{title}1",
                Tags = null,
                Limit = 4,
                Offset = 0,
            }
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
                 true)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticlesResponseEnvelope
        {
            Total = 1,
            Models =
              [.. Enumerable
                     .Range(1, 1)
                     .Select(i => new ArticleResponseModel
                         {
                             Id = i,
                             Title = $"{title}{i}",
                             Slug = $"{slug}{i}",
                             Description = $"{description}{i}",
                             Published = true,
                             CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
                         })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesDescendigWithoutTagFilterShouldReturnSuccessWithAllArticles(
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
          .WithLocation("api/v1.0/articles")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0, ""published"": true, ""createdAtAsc"": false }}}}")
          )
        )
        .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
        {
            ArticleTagsJson = new()
            {
                CreatedAtAsc = false,
                Published = true,
                Tags = null,
                Limit = 4,
                Offset = 0,
            }
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
                 true)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticlesResponseEnvelope
        {
            Total = 5,
            Models = [.. Enumerable
             .Range(1, 4)
             .Select(i =>
             {
                 var j = 6 - i;
                 return new ArticleResponseModel
                 {
                     Id = j,
                     Title = $"{title}{j}",
                     Slug = $"{slug}{j}",
                     Description = $"{description}{j}",
                     Published = true,
                     CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(j))),
                 };
             })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesWithoutCreateAtAscWithoutTagFilterShouldReturnSuccessWithAllArticles(
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
          .WithLocation("api/v1.0/articles")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0, ""published"": true }}}}")
          )
        )
        .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
        {
            ArticleTagsJson = new()
            {
                CreatedAtAsc = null,
                Published = true,
                Tags = null,
                Limit = 4,
                Offset = 0,
            }
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
                 true)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticlesResponseEnvelope
        {
            Total = 5,
            Models = [.. Enumerable
             .Range(1, 4)
             .Select(i =>
             {
                 var j = 6 - i;
                 return new ArticleResponseModel
                 {
                     Id = j,
                     Title = $"{title}{j}",
                     Slug = $"{slug}{j}",
                     Description = $"{description}{j}",
                     Published = true,
                     CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(j))),
                 };
             })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesWithoutCreateAtAscWithoutPublishedWithoutTagFilterShouldReturnSuccessWithAllArticles(
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
          .WithLocation("api/v1.0/articles")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0 }}}}")
          )
        )
        .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
        {
            ArticleTagsJson = new()
            {
                CreatedAtAsc = null,
                Published = null,
                Tags = null,
                Limit = 4,
                Offset = 0,
            }
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
                 true)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticlesResponseEnvelope
        {
            Total = 5,
            Models = [.. Enumerable
                     .Range(1, 4)
                     .Select(i =>
                     {
                         var j = 6 - i;
                         return new ArticleResponseModel
                         {
                             Id = j,
                             Title = $"{title}{j}",
                             Slug = $"{slug}{j}",
                             Description = $"{description}{j}",
                             Published = true,
                             CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(j))),
                         };
                     })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesWithTitleEndingWithSpaceShouldReturnValidationError(
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
             .WithLocation("api/v1.0/articles")
             .WithJsonBody(
                    string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 0, ""offset"": -1, ""published"": true, ""title"": ""{0}""}}}}", $"{VALID_MAX_TITLE_LENGTH} ")
             )
           )
           .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
           {
               ArticleTagsJson = new()
               {
                   CreatedAtAsc = null,
                   Published = true,
                   Title = $"{VALID_MAX_TITLE_LENGTH} ",
                   Tags = null,
                   Limit = 0,
                   Offset = -1,
               }
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
                    true)))
           .ShouldHave()
           .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
           .AndAlso()
           .ShouldReturn(), new Dictionary<string, string[]>
        {
           { "ArticleTagsJson.Title", ["The specified condition was not met for 'Article Tags Json Title'."] },
           { "ArticleTagsJson.Limit", ["'Article Tags Json Limit' must be greater than '0'."] },
           { "ArticleTagsJson.Offset", ["'Article Tags Json Offset' must be greater than '-1'."] }
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesWithTitleStartingWithSpaceShouldReturnValidationError(
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
             .WithLocation("api/v1.0/articles")
             .WithJsonBody(
                    string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 0, ""offset"": -1, ""published"": true, ""title"": "" ""}}}}")
             )
           )
           .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
           {
               ArticleTagsJson = new()
               {
                   CreatedAtAsc = null,
                   Published = true,
                   Title = " ",
                   Tags = null,
                   Limit = 0,
                   Offset = -1,
               }
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
                    true)))
           .ShouldHave()
           .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
           .AndAlso()
           .ShouldReturn(), new Dictionary<string, string[]>
        {
           { "ArticleTagsJson.Title", ["The specified condition was not met for 'Article Tags Json Title'."] },
           { "ArticleTagsJson.Limit", ["'Article Tags Json Limit' must be greater than '0'."] },
           { "ArticleTagsJson.Offset", ["'Article Tags Json Offset' must be greater than '-1'."] }
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesWithMinTitleAndWrongLimitAndOffsetShouldReturnValidationError(
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
             .WithLocation("api/v1.0/articles")
             .WithJsonBody(
                    string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 0, ""offset"": -1, ""published"": true, ""title"": """"}}}}")
             )
           )
           .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
           {
               ArticleTagsJson = new()
               {
                   CreatedAtAsc = null,
                   Published = true,
                   Title = "",
                   Tags = null,
                   Limit = 0,
                   Offset = -1,
               }
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
                    true)))
           .ShouldHave()
           .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
           .AndAlso()
           .ShouldReturn(), new Dictionary<string, string[]>
        {
           { "ArticleTagsJson.Title", ["The length of 'Article Tags Json Title' must be at least 1 characters. You entered 0 characters."] },
           { "ArticleTagsJson.Limit", ["'Article Tags Json Limit' must be greater than '0'."] },
           { "ArticleTagsJson.Offset", ["'Article Tags Json Offset' must be greater than '-1'."] }
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingArticlesWithMaxTitleAndWrongLimitAndOffsetShouldReturnValidationError(
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
             .WithLocation("api/v1.0/articles")
             .WithJsonBody(
                    string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 0, ""offset"": -1, ""published"": true, ""title"": ""{0}""}}}}", $"{VALID_MAX_TITLE_LENGTH}ab")
             )
           )
           .To<ArticlesController>(c => c.Articles(new ArticleTagsListingCommand
           {
               ArticleTagsJson = new()
               {
                   CreatedAtAsc = null,
                   Published = true,
                   Title = $"{VALID_MAX_TITLE_LENGTH}ab",
                   Tags = null,
                   Limit = 0,
                   Offset = -1,
               }
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
                    true)))
           .ShouldHave()
           .ActionAttributes(attrs => attrs.RestrictingForHttpMethod(HttpMethod.Post))
           .AndAlso()
           .ShouldReturn(), new Dictionary<string, string[]>
        {
           { "ArticleTagsJson.Title", ["The length of 'Article Tags Json Title' must be 320 characters or fewer. You entered 321 characters."] },
           { "ArticleTagsJson.Limit", ["'Article Tags Json Limit' must be greater than '0'."] },
           { "ArticleTagsJson.Offset", ["'Article Tags Json Offset' must be greater than '-1'."] }
        });
        #endregion

        #region Listing All Articles
        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingAllArticlesAscendingWithTagFilterShouldReturnSuccessWithArticleList(
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
          .WithLocation("api/v1.0/articles/all")
          .WithJsonBody(
                string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0, ""tags"": [{0},{1},{2}], ""published"": true, ""createdAtAsc"": true}}}}", 1, 2, 3)
          )
        )
        .To<ArticlesController>(c => c.All(new ArticleTagsListingCommand
        {
            ArticleTagsJson = new()
            {
                CreatedAtAsc = true,
                Published = true,
                Tags = Enumerable
                       .Range(1, 3)
                       .Select(i => i).ToList(),
                Limit = 4,
                Offset = 0,
            }
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
                 true)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs
              .RestrictingForHttpMethod(HttpMethod.Post)
              .RestrictingForAuthorizedRequests())
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticlesResponseEnvelope
        {
            Total = 4,
            Models = [.. Enumerable
             .Range(1, 4)
             .Select(i => new ArticleResponseModel
                 {
                     Id = i,
                     Title = $"{title}{i}",
                     Slug = $"{slug}{i}",
                     Description = $"{description}{i}",
                     Published = true,
                     CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
                     TagList = [.. Enumerable
                                .Range(1, 3)
                                .Select(i =>
                                {
                                    var r = 0;
                                    switch (i)
                                    {
                                        case 1:
                                            r = 5;
                                            break;
                                        case 2:
                                            r = 4;
                                            break;
                                        case 3:
                                            r = 3;
                                            break;
                                    }
                                    return r;
                                })],

                 })],
        }));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingAllArticlesAscendingWithOffsetWithTagFilterShouldReturnSuccessWithEmpty(
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
          .WithLocation("api/v1.0/articles/all")
          .WithJsonBody(
                string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 4, ""tags"": [{0},{1},{2}], ""published"": true}}}}", 1, 2, 3)
          )
        )
        .To<ArticlesController>(c => c.All(new ArticleTagsListingCommand
        {
            ArticleTagsJson = new()
            {
                CreatedAtAsc = null,
                Published = true,
                Tags = Enumerable
                       .Range(1, 3)
                       .Select(i => i).ToList(),
                Limit = 4,
                Offset = 4,
            }
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
                 true)))
        .ShouldHave()
        .ActionAttributes(attrs => attrs
              .RestrictingForHttpMethod(HttpMethod.Post)
              .RestrictingForAuthorizedRequests())
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticlesResponseEnvelope
        {
            Total = 4,
            Models = [],
        }));

        [Fact]
        public void ListingAllArticlesWithoutAuthorizationHeaderShouldFail()
        => AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () => MyMvc
              .Pipeline()
              .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithLocation("api/v1.0/articles/all")
                .WithJsonBody(
                       string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0 }}}}")
                )
            )
            .To<ArticlesController>(c => c.All(new ArticleTagsListingCommand
            {
                ArticleTagsJson = new()
                {
                    CreatedAtAsc = null,
                    Published = null,
                    Tags = null,
                    Limit = 4,
                    Offset = 0,
                }
            })), string.Format(CultureInfo.InvariantCulture, HEADER_AUTHORIZATION_EXCEPTION.Replace(Environment.NewLine, ""), "/api/v1.0/articles/all", "All", "ArticlesController"));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void ListingAllArticlesWithExpiredAuthorizationHeaderShouldFail(
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
                .WithMethod(HttpMethod.Post)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithExpiredToken(email, 1))
                .WithLocation("api/v1.0/articles/all")
                .WithJsonBody(
                       string.Format(CultureInfo.InvariantCulture, @"{{""filter"":{{""limit"": 4, ""offset"": 0 }}}}")
                )
            )
          .To<ArticlesController>(c => c.All(new ArticleTagsListingCommand
          {
              ArticleTagsJson = new()
              {
                  CreatedAtAsc = null,
                  Published = null,
                  Tags = null,
                  Limit = 4,
                  Offset = 0,
              }
          })), string.Format(CultureInfo.InvariantCulture, HEADER_AUTHORIZATION_EXCEPTION.Replace(Environment.NewLine, ""), "/api/v1.0/articles/all", "All", "ArticlesController"));
        #endregion

        #region Create Article
        [Theory]
        [MemberData(nameof(ValidData))]
        public void CreateArticleShouldReturnSuccessWithData(
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
          .WithLocation("api/v1.0/articles/create")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}"", ""published"": ""{3}""}}}}",
                     $"{title}4",
                     $"{slug}4",
                     $"{description}4",
                     true)
          )
        )
        .To<ArticlesController>(c => c.Create(new()
        {
            ArticleJson = new()
            {
                Title = $"{title}4",
                Slug = $"{slug}4",
                Description = $"{description}4",
                Published = true
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
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticleResponseEnvelope
        {
            ArticleJson = new()
            {
                Id = 4,
                Title = $"{title}4",
                Slug = $"{slug}4",
                Description = $"{description}4",
                CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today)),
                Published = true
            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void CreateArticleWithoutPublishedShouldReturnSuccessWithData(
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
          .WithLocation("api/v1.0/articles/create")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                     $"{title}4",
                     $"{slug}4",
                     $"{description}4")
          )
        )
        .To<ArticlesController>(c => c.Create(new()
        {
            ArticleJson = new()
            {
                Title = $"{title}4",
                Slug = $"{slug}4",
                Description = $"{description}4"
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
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticleResponseEnvelope
        {
            ArticleJson = new()
            {
                Id = 4,
                Title = $"{title}4",
                Slug = $"{slug}4",
                Description = $"{description}4",
                CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today)),
                Published = false
            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void CreateArticleWithSameTitleAndSlugShouldReturnValidationError(
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
               .WithLocation("api/v1.0/articles/create")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                          $"{title}1",
                          $"{slug}1",
                          $"{description}4")
               )
             )
             .To<ArticlesController>(c => c.Create(new()
             {
                 ArticleJson = new()
                 {
                     Title = $"{title}1",
                     Slug = $"{slug}1",
                     Description = $"{description}4"
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
            { "ArticleJson.Title", ["'Article Json Title' must be unique."] },
            { "ArticleJson.Slug", ["'Article Json Slug' must be unique."] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void CreateArticleWithMaxCharsShouldReturnValidationError(
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
              .WithLocation("api/v1.0/articles/create")
              .WithJsonBody(
                     string.Format(CultureInfo.InvariantCulture, @"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                         $"{VALID_MAX_TITLE_LENGTH}ab",
                         $"{VALID_MAX_TITLE_LENGTH}ab",
                         $"{VALID_MAX_DESCRIPTION_LENGTH}ab")
              )
            )
            .To<ArticlesController>(c => c.Create(new()
            {
                ArticleJson = new()
                {
                    Title = $"{VALID_MAX_TITLE_LENGTH}ab",
                    Slug = $"{VALID_MAX_TITLE_LENGTH}ab",
                    Description = $"{VALID_MAX_DESCRIPTION_LENGTH}ab"
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
           { "ArticleJson.Title", ["The length of 'Article Json Title' must be 320 characters or fewer. You entered 321 characters."] },
           { "ArticleJson.Slug", ["The length of 'Article Json Slug' must be 320 characters or fewer. You entered 321 characters."] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void CreateArticleWithMinCharsShouldReturnValidationError(
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
              .WithLocation("api/v1.0/articles/create")
              .WithJsonBody(
                     string.Format(CultureInfo.InvariantCulture, @"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                         "a",
                         "a",
                         "a")
              )
            )
            .To<ArticlesController>(c => c.Create(new()
            {
                ArticleJson = new()
                {
                    Title = "a",
                    Slug = "a",
                    Description = "a"
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
            { "ArticleJson.Title", ["The length of 'Article Json Title' must be at least 2 characters. You entered 1 characters."] },
            { "ArticleJson.Slug", ["The length of 'Article Json Slug' must be at least 2 characters. You entered 1 characters."] },
            { "ArticleJson.Description", ["The length of 'Article Json Description' must be at least 2 characters. You entered 1 characters."] },
        });
        #endregion

        #region Edit Article
        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditArticleShouldReturnSuccessWithData(
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
          .WithLocation("api/v1.0/articles/edit/2")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}"", ""published"": ""{3}""}}}}",
                     $"{title}4",
                     $"{slug}4",
                     $"{description}4",
                     true)
          )
        )
        .To<ArticlesController>(c => c.Edit(2, new()
        {
            ArticleJson = new()
            {
                Title = $"{title}4",
                Slug = $"{slug}4",
                Description = $"{description}4",
                Published = true
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
        .ActionResult(result => result.Result(new ArticleResponseEnvelope
        {
            ArticleJson = new()
            {
                Id = 2,
                Title = $"{title}4",
                Slug = $"{slug}4",
                Description = $"{description}4",
                CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(2))),
                Published = true
            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditArticleWithoutPublishedShouldReturnSuccessWithData(
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
          .WithLocation("api/v1.0/articles/edit/2")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                     $"{title}4",
                     $"{slug}4",
                     $"{description}4")
          )
        )
        .To<ArticlesController>(c => c.Edit(2, new()
        {
            ArticleJson = new()
            {
                Title = $"{title}4",
                Slug = $"{slug}4",
                Description = $"{description}4",
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
        .ActionResult(result => result.Result(new ArticleResponseEnvelope
        {
            ArticleJson = new()
            {
                Id = 2,
                Title = $"{title}4",
                Slug = $"{slug}4",
                Description = $"{description}4",
                CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(2))),
                Published = false
            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditSameArticleWithSameTitleShouldReturnSuccessWithData(
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
          .WithLocation("api/v1.0/articles/edit/2")
          .WithJsonBody(
                 string.Format(CultureInfo.InvariantCulture, @"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                     $"{title}2",
                     $"{slug}2",
                     $"{description}4")
          )
        )
        .To<ArticlesController>(c => c.Edit(2, new()
        {
            ArticleJson = new()
            {
                Title = $"{title}2",
                Slug = $"{slug}2",
                Description = $"{description}4"
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
        .ActionResult(result => result.Result(new ArticleResponseEnvelope
        {
            ArticleJson = new()
            {
                Id = 2,
                Title = $"{title}2",
                Slug = $"{slug}2",
                Description = $"{description}4",
                CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(2))),
                Published = false
            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditArticleWithSameTitleAndSlugShouldReturnValidationError(
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
               .WithLocation("api/v1.0/articles/edit/2")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                          $"{title}1",
                          $"{slug}1",
                          $"{description}4")
               )
             )
             .To<ArticlesController>(c => c.Edit(2, new()
             {
                 ArticleJson = new()
                 {
                     Title = $"{title}1",
                     Slug = $"{slug}1",
                     Description = $"{description}4"
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
          { "ArticleJson.Title", ["'Article Json Title' must be unique."] },
          { "ArticleJson.Slug", ["'Article Json Slug' must be unique."] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditArticleWithMaxCharsShouldReturnValidationError(
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
              .WithLocation("api/v1.0/articles/edit/2")
              .WithJsonBody(
                     string.Format(CultureInfo.InvariantCulture, @"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                         $"{VALID_MAX_TITLE_LENGTH}ab",
                         $"{VALID_MAX_TITLE_LENGTH}ab",
                         $"{VALID_MAX_DESCRIPTION_LENGTH}ab")
              )
            )
            .To<ArticlesController>(c => c.Edit(2, new()
            {
                ArticleJson = new()
                {
                    Title = $"{VALID_MAX_TITLE_LENGTH}ab",
                    Slug = $"{VALID_MAX_TITLE_LENGTH}ab",
                    Description = $"{VALID_MAX_DESCRIPTION_LENGTH}ab"
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
           { "ArticleJson.Title", ["The length of 'Article Json Title' must be 320 characters or fewer. You entered 321 characters."] },
           { "ArticleJson.Slug", ["The length of 'Article Json Slug' must be 320 characters or fewer. You entered 321 characters."] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditArticleWithMinCharsShouldReturnValidationError(
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
                  .WithLocation("api/v1.0/articles/edit/2")
                  .WithJsonBody(
                         string.Format(CultureInfo.InvariantCulture, @"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                             "a",
                             "a",
                             "a")
                  )
                )
                .To<ArticlesController>(c => c.Edit(2, new()
                {
                    ArticleJson = new()
                    {
                        Title = "a",
                        Slug = "a",
                        Description = "a"
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
           { "ArticleJson.Title", ["The length of 'Article Json Title' must be at least 2 characters. You entered 1 characters."] },
           { "ArticleJson.Slug", ["The length of 'Article Json Slug' must be at least 2 characters. You entered 1 characters."] },
           { "ArticleJson.Description", ["The length of 'Article Json Description' must be at least 2 characters. You entered 1 characters."] },
        });

        [Theory]
        [MemberData(nameof(ValidData))]
        public void EditArticleWithWrongIdShouldFail(
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
               .WithLocation("api/v1.0/articles/edit/5")
               .WithJsonBody(
                      string.Format(CultureInfo.InvariantCulture, @"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                          $"{title}4",
                          $"{slug}4",
                          $"{description}4")
               )
             )
             .To<ArticlesController>(c => c.Edit(5, new()
             {
                 ArticleJson = new()
                 {
                     Title = $"{title}4",
                     Slug = $"{slug}4",
                     Description = $"{description}4"
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
             .ShouldReturn(), string.Format(CultureInfo.InvariantCulture, FROM_NOT_FOUND_EXCEPTION.Replace(Environment.NewLine, ""), "Edit", "ArticlesController", "NotFoundException", "article", 5));
        #endregion

        #region Get Article
        [Theory]
        [MemberData(nameof(ValidData))]
        public void GetArticleWithTagsShouldReturnSuccessWithData(
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
          .WithMethod(HttpMethod.Get)
          .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
          .WithLocation("api/v1.0/articles/details/4")
        )
        .To<ArticlesController>(c => c.Details(new()
        {
            Id = 4,
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
             .RestrictingForHttpMethod(HttpMethod.Get)
             .RestrictingForAuthorizedRequests())
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticleResponseEnvelope
        {
            ArticleJson = new()
            {
                Id = 4,
                Title = $"{title}4",
                Slug = $"{slug}4",
                Description = $"{description}4",
                CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(4))),
                TagList = [.. Enumerable
                           .Range(1, 3)
                           .Select(i =>
                           {
                               var r = 0;
                               switch (i)
                               {
                                   case 1:
                                       r = 5;
                                       break;
                                   case 2:
                                       r = 4;
                                       break;
                                   case 3:
                                       r = 3;
                                       break;
                               }
                               return r;
                           })],

            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void GetArticleWithoutTagsShouldReturnSuccessWithData(
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
          .WithMethod(HttpMethod.Get)
          .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
          .WithLocation("api/v1.0/articles/details/5")
        )
        .To<ArticlesController>(c => c.Details(new()
        {
            Id = 5,
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
             .RestrictingForHttpMethod(HttpMethod.Get)
             .RestrictingForAuthorizedRequests())
        .AndAlso()
        .ShouldReturn()
        .ActionResult(result => result.Result(new ArticleResponseEnvelope
        {
            ArticleJson = new()
            {
                Id = 5,
                Title = $"{title}5",
                Slug = $"{slug}5",
                Description = $"{description}5",
                CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(5)))
            }
        }))
        .AndAlso()
        .ShouldPassForThe<ActionAttributes>(attributes => Assert.InRange(attributes.Count(), 4, 5));

        [Theory]
        [MemberData(nameof(ValidData))]
        public void GetArticleWithWrongIdShouldFail(
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
                .WithMethod(HttpMethod.Get)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/articles/details/6")
              )
              .To<ArticlesController>(c => c.Details(new()
              {
                  Id = 6,
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
                   .RestrictingForHttpMethod(HttpMethod.Get)
                   .RestrictingForAuthorizedRequests())
              .AndAlso()
              .ShouldReturn(), string.Format(CultureInfo.InvariantCulture, FROM_NOT_FOUND_EXCEPTION.Replace(Environment.NewLine, ""), "Details", "ArticlesController", "NotFoundException", "article", 6));
        #endregion

        #region Delete Article
        [Theory]
        [MemberData(nameof(ValidData))]
        public void DeleteArticleShouldReturnSuccessWithArticleId(
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
          .WithLocation("api/v1.0/articles/delete/2")
        )
        .To<ArticlesController>(c => c.Delete(new()
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
        public void DeleteNonExistingArticleShouldReturnSuccessWithNegativeOne(
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
          .WithLocation("api/v1.0/articles/delete/20")
        )
        .To<ArticlesController>(c => c.Delete(new()
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
        #endregion
    }
}
