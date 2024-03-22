namespace BlogAngular.Test.Routing;

using Application.Blog.Articles.Commands.Common;
using Application.Blog.ArticleTags.Commands.Create;
using Application.Blog.ArticleTags.Commands.Listing;
using Application.Blog.Common;
using MyTested.AspNetCore.Mvc;
using MyTested.AspNetCore.Mvc.Test.Setups;
using System;
using System.Collections.Generic;
using System.Linq;
using Test.Data;
using Web.Features;
using Xunit;
using static ControllerExceptionMessages;
using static Domain.Common.Models.ModelConstants.Article;
using static Domain.Common.Models.ModelConstants.Identity;
using static Domain.Common.Models.ModelConstants.Tag;

public class ArticlesControllerRouteTest
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
    public void Link_tags_to_article_with_expired_authorization_header_should_fail(
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
     => Test.AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () =>
        {
            MyMvc
              .Pipeline()
              .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithExpiredToken(email, 1))
                .WithLocation("api/v1.0/articles/linktags")
                .WithJsonBody(
                      string.Format(@"{{""articleTags"":{{""articleId"": 4, ""tags"": [{0},{1},{2}]}}}}", 1, 2, 3)
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
              }));

        }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/articles/linktags", "LinkTags", "ArticlesController"));

    [Fact]
    public void Link_tags_to_article_without_authorization_header_should_fail()
     => Test.AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () =>
        {
            MyMvc
              .Pipeline()
              .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithLocation("api/v1.0/articles/linktags")
                .WithJsonBody(
                      string.Format(@"{{""articleTags"":{{""articleId"": 4, ""tags"": [{0},{1},{2}]}}}}", 1, 2, 3)
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
              }));

        }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/articles/linktags", "LinkTags", "ArticlesController"));

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Link_tags_to_article_with_incorrect_tag_should_fail(
     string fullName,
     string email,
     string password,
     string name,
     string title,
     string slug,
     string description)
     => Test.AssertException<MyTested.AspNetCore.Mvc.Exceptions.InvocationAssertionException>(
        () =>
        {
            MyMvc
            .Pipeline()
            .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/articles/linktags")
                .WithJsonBody(
                      string.Format(@"{{""articleTags"":{{""articleId"": 1, ""tags"": [{0},{1},{2}]}}}}", 1, 2, 3)
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
            .ShouldReturn();
        }, string.Format(FromBaseDomainException.Replace(Environment.NewLine, ""), "LinkTags", "ArticlesController", "NotFoundException", "tag", 3));

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Link_tags_to_article_with_incorrect_article_should_fail(
     string fullName,
     string email,
     string password,
     string name,
     string title,
     string slug,
     string description)
     => Test.AssertException<MyTested.AspNetCore.Mvc.Exceptions.InvocationAssertionException>(
        () =>
        {
            MyMvc
            .Pipeline()
            .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                .WithLocation("api/v1.0/articles/linktags")
                .WithJsonBody(
                      string.Format(@"{{""articleTags"":{{""articleId"": 6, ""tags"": [{0},{1},{2}]}}}}", 1, 2, 3)
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
            .ShouldReturn();
        }, string.Format(FromBaseDomainException.Replace(Environment.NewLine, ""), "LinkTags", "ArticlesController", "NotFoundException", "ArticleId", 6));


    [Theory]
    [MemberData(nameof(ValidData))]
    public void Link_tags_to_article_should_return_success_with_id(
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
              string.Format(@"{{""articleTags"":{{""articleId"": 4, ""tags"": [{0},{1},{2}]}}}}", 1, 2, 3)
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
      .ShouldPassForThe<ActionAttributes>(attributes =>
      {
          Assert.Equal(5, attributes.Count());
      });


    [Theory]
    [MemberData(nameof(ValidData))]
    public void Link_empty_tag_list_to_article_should_return_success_with_id(
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
              string.Format(@"{{""articleTags"":{{""articleId"": 4 }}}}")
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
      .ShouldPassForThe<ActionAttributes>(attributes =>
      {
          Assert.Equal(5, attributes.Count());
      });


    [Theory]
    [MemberData(nameof(ValidData))]
    public void Listing_articles_ascending_with_tag_filter_should_return_success_with_article_list(
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
              string.Format(@"{{""filter"":{{""limit"": 4, ""offset"": 0, ""tags"": [{0},{1},{2}], ""published"": true, ""createdAtAsc"": true}}}}", 1, 2, 3)
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
          Total = 5,
          Models = Enumerable
           .Range(1, 4)
           .Select(i =>
           {
               return new ArticleResponseModel
               {
                   Id = i,
                   Title = string.Format("{0}{1}", title, i),
                   Slug = string.Format("{0}{1}", slug, i),
                   Description = string.Format("{0}{1}", description, i),
                   Published = true,
                   CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
                   TagList = Enumerable
                              .Range(1, 3)
                              .Select(i =>
                              {
                                  int r = 0;
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
                              }).ToList(),

               };
           }).ToList(),
      }));


    [Theory]
    [MemberData(nameof(ValidData))]
    public void Listing_articles_descending_with_tag_filter_should_return_success_with_article_list(
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
              string.Format(@"{{""filter"":{{""limit"": 4, ""offset"": 0, ""tags"": [{0},{1},{2}], ""published"": true}}}}", 1, 2, 3)
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
          Total = 5,
          Models = Enumerable
         .Range(1, 4)
         .Select(i =>
         {
             var j = 6 - i;
             return new ArticleResponseModel
             {
                 Id = j,
                 Title = string.Format("{0}{1}", title, j),
                 Slug = string.Format("{0}{1}", slug, j),
                 Description = string.Format("{0}{1}", description, j),
                 Published = true,
                 CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(j))),
                 TagList = Enumerable
                            .Range(1, 3)
                            .Select(i =>
                            {
                                int r = 0;
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
                            }).ToList(),

             };
         }).ToList(),
      }));


    [Theory]
    [MemberData(nameof(ValidData))]
    public void Listing_articles_ascending_without_tag_filter_should_return_success_with_all_articles(
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
               string.Format(@"{{""filter"":{{""limit"": 4, ""offset"": 0, ""published"": true, ""createdAtAsc"": true }}}}")
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
            Enumerable
           .Range(1, 4)
           .Select(i =>
           {
               return new ArticleResponseModel
               {
                   Id = i,
                   Title = string.Format("{0}{1}", title, i),
                   Slug = string.Format("{0}{1}", slug, i),
                   Description = string.Format("{0}{1}", description, i),
                   Published = true,
                   CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
               };
           }).ToList(),
      }));

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Listing_articles_descendig_without_tag_filter_should_return_success_with_all_articles(
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
               string.Format(@"{{""filter"":{{""limit"": 4, ""offset"": 0, ""published"": true }}}}")
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
          Models = Enumerable
           .Range(1, 4)
           .Select(i =>
           {
               var j = 6 - i;
               return new ArticleResponseModel
               {
                   Id = j,
                   Title = string.Format("{0}{1}", title, j),
                   Slug = string.Format("{0}{1}", slug, j),
                   Description = string.Format("{0}{1}", description, j),
                   Published = true,
                   CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(j))),
               };
           }).ToList(),
      }));


    [Fact]
    public void Listing_all_articles_without_authorization_header_should_fail()
      => Test.AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
         () =>
         {
             MyMvc
               .Pipeline()
               .ShouldMap(request => request
                 .WithMethod(HttpMethod.Post)
                 .WithLocation("api/v1.0/articles/all")
                 .WithJsonBody(
                        string.Format(@"{{""filter"":{{""limit"": 4, ""offset"": 0 }}}}")
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
       }));

         }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/articles/all", "All", "ArticlesController"));


    [Theory]
    [MemberData(nameof(ValidData))]
    public void Listing_all_articles_with_expired_authorization_header_should_fail(
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
     => Test.AssertException<MyTested.AspNetCore.Mvc.Exceptions.RouteAssertionException>(
        () =>
        {
            MyMvc
              .Pipeline()
              .ShouldMap(request => request
                .WithMethod(HttpMethod.Post)
                .WithHeaderAuthorization(StaticTestData.GetJwtBearerWithExpiredToken(email, 1))
                .WithLocation("api/v1.0/articles/all")
                .WithJsonBody(
                       string.Format(@"{{""filter"":{{""limit"": 4, ""offset"": 0 }}}}")
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
      }));

        }, string.Format(HeaderAuthorizationException.Replace(Environment.NewLine, ""), "/api/v1.0/articles/all", "All", "ArticlesController"));


    [Theory]
    [MemberData(nameof(ValidData))]
    public void Listing_all_articles_ascending_with_tag_filter_should_return_success_with_article_list(
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
              string.Format(@"{{""filter"":{{""limit"": 4, ""offset"": 0, ""tags"": [{0},{1},{2}], ""published"": true, ""createdAtAsc"": true}}}}", 1, 2, 3)
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
          Total = 5,
          Models = Enumerable
           .Range(1, 4)
           .Select(i =>
           {
               return new ArticleResponseModel
               {
                   Id = i,
                   Title = string.Format("{0}{1}", title, i),
                   Slug = string.Format("{0}{1}", slug, i),
                   Description = string.Format("{0}{1}", description, i),
                   Published = true,
                   CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(i))),
                   TagList = Enumerable
                              .Range(1, 3)
                              .Select(i =>
                              {
                                  int r = 0;
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
                              }).ToList(),

               };
           }).ToList(),
      }));


    [Theory]
    [MemberData(nameof(ValidData))]
    public void Create_article_should_return_success_with_data(
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
               string.Format(@"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}"", ""published"": ""{3}""}}}}",
                   string.Format("{0}{1}", title, 4),
                   string.Format("{0}{1}", slug, 4),
                   string.Format("{0}{1}", description, 4),
                   true)
        )
      )
      .To<ArticlesController>(c => c.Create(new()
      {
          ArticleJson = new()
          {
              Title = string.Format("{0}{1}", title, 4),
              Slug = string.Format("{0}{1}", slug, 4),
              Description = string.Format("{0}{1}", description, 4),
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
              Title = string.Format("{0}{1}", title, 4),
              Slug = string.Format("{0}{1}", slug, 4),
              Description = string.Format("{0}{1}", description, 4),
              CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today)),
              Published = true
          }
      }))
      .AndAlso()
      .ShouldPassForThe<ActionAttributes>(attributes =>
      {
          Assert.Equal(5, attributes.Count());
      });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Create_article_without_published_should_return_success_with_data(
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
               string.Format(@"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                   string.Format("{0}{1}", title, 4),
                   string.Format("{0}{1}", slug, 4),
                   string.Format("{0}{1}", description, 4))
        )
      )
      .To<ArticlesController>(c => c.Create(new()
      {
          ArticleJson = new()
          {
              Title = string.Format("{0}{1}", title, 4),
              Slug = string.Format("{0}{1}", slug, 4),
              Description = string.Format("{0}{1}", description, 4)
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
              Title = string.Format("{0}{1}", title, 4),
              Slug = string.Format("{0}{1}", slug, 4),
              Description = string.Format("{0}{1}", description, 4),
              CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today)),
              Published = false
          }
      }))
      .AndAlso()
      .ShouldPassForThe<ActionAttributes>(attributes =>
      {
          Assert.Equal(5, attributes.Count());
      });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Create_article_with_same_title_and_slug_should_return_validation_error(
    string fullName,
    string email,
    string password,
    string name,
    string title,
    string slug,
    string description)
    => Test.AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
    () =>
    {
        MyMvc
         .Pipeline()
         .ShouldMap(request => request
           .WithMethod(HttpMethod.Post)
           .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
           .WithLocation("api/v1.0/articles/create")
           .WithJsonBody(
                  string.Format(@"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                      string.Format("{0}{1}", title, 1),
                      string.Format("{0}{1}", slug, 1),
                      string.Format("{0}{1}", description, 4))
           )
         )
         .To<ArticlesController>(c => c.Create(new()
         {
             ArticleJson = new()
             {
                 Title = string.Format("{0}{1}", title, 1),
                 Slug = string.Format("{0}{1}", slug, 1),
                 Description = string.Format("{0}{1}", description, 4)
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
            { "ArticleJson.Title", new[] { "'Article Json Title' must be unique." } },
            { "ArticleJson.Slug", new[] { "'Article Json Slug' must be unique." } },
    });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Create_article_with_max_chars_should_return_validation_error(
     string fullName,
     string email,
     string password,
     string name,
     string title,
     string slug,
     string description)
    => Test.AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
    () =>
    {
        MyMvc
        .Pipeline()
        .ShouldMap(request => request
          .WithMethod(HttpMethod.Post)
          .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
          .WithLocation("api/v1.0/articles/create")
          .WithJsonBody(
                 string.Format(@"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                     string.Format("{0}{1}", ValidMaxxTitleLength, "ab"),
                     string.Format("{0}{1}", ValidMaxxTitleLength, "ab"),
                     string.Format("{0}{1}", ValidMaxxDescriptionLength, "ab"))
          )
        )
        .To<ArticlesController>(c => c.Create(new()
        {
            ArticleJson = new()
            {
                Title = string.Format("{0}{1}", ValidMaxxTitleLength, "ab"),
                Slug = string.Format("{0}{1}", ValidMaxxTitleLength, "ab"),
                Description = string.Format("{0}{1}", ValidMaxxDescriptionLength, "ab")
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
        { "ArticleJson.Title", new[] { "The length of 'Article Json Title' must be 320 characters or fewer. You entered 321 characters." } },
        { "ArticleJson.Slug", new[] { "The length of 'Article Json Slug' must be 320 characters or fewer. You entered 321 characters." } },
    });


    [Theory]
    [MemberData(nameof(ValidData))]
    public void Create_article_with_min_chars_should_return_validation_error(
     string fullName,
     string email,
     string password,
     string name,
     string title,
     string slug,
     string description)
    => Test.AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
    () =>
    {
        MyMvc
        .Pipeline()
        .ShouldMap(request => request
          .WithMethod(HttpMethod.Post)
          .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
          .WithLocation("api/v1.0/articles/create")
          .WithJsonBody(
                 string.Format(@"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
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
        .ShouldReturn();
    }, new Dictionary<string, string[]>
    {
            { "ArticleJson.Title", new[] { "The length of 'Article Json Title' must be at least 2 characters. You entered 1 characters." } },
            { "ArticleJson.Slug", new[] { "The length of 'Article Json Slug' must be at least 2 characters. You entered 1 characters." } },
            { "ArticleJson.Description", new[] { "The length of 'Article Json Description' must be at least 2 characters. You entered 1 characters." } },
    });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Edit_article_should_return_success_with_data(
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
               string.Format(@"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}"", ""published"": ""{3}""}}}}",
                   string.Format("{0}{1}", title, 4),
                   string.Format("{0}{1}", slug, 4),
                   string.Format("{0}{1}", description, 4),
                   true)
        )
      )
      .To<ArticlesController>(c => c.Edit(2, new()
      {
          ArticleJson = new()
          {
              Title = string.Format("{0}{1}", title, 4),
              Slug = string.Format("{0}{1}", slug, 4),
              Description = string.Format("{0}{1}", description, 4),
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
              Title = string.Format("{0}{1}", title, 4),
              Slug = string.Format("{0}{1}", slug, 4),
              Description = string.Format("{0}{1}", description, 4),
              CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(2))),
              Published = true
          }
      }))
      .AndAlso()
      .ShouldPassForThe<ActionAttributes>(attributes =>
      {
          Assert.Equal(5, attributes.Count());
      });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Edit_article_without_published_should_return_success_with_data(
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
               string.Format(@"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                   string.Format("{0}{1}", title, 4),
                   string.Format("{0}{1}", slug, 4),
                   string.Format("{0}{1}", description, 4))
        )
      )
      .To<ArticlesController>(c => c.Edit(2, new()
      {
          ArticleJson = new()
          {
              Title = string.Format("{0}{1}", title, 4),
              Slug = string.Format("{0}{1}", slug, 4),
              Description = string.Format("{0}{1}", description, 4),
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
              Title = string.Format("{0}{1}", title, 4),
              Slug = string.Format("{0}{1}", slug, 4),
              Description = string.Format("{0}{1}", description, 4),
              CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(2))),
              Published = false
          }
      }))
      .AndAlso()
      .ShouldPassForThe<ActionAttributes>(attributes =>
      {
          Assert.Equal(5, attributes.Count());
      });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Edit_same_article_with_same_title_should_return_success_with_data(
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
               string.Format(@"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                   string.Format("{0}{1}", title, 2),
                   string.Format("{0}{1}", slug, 2),
                   string.Format("{0}{1}", description, 4))
        )
      )
      .To<ArticlesController>(c => c.Edit(2, new()
      {
          ArticleJson = new()
          {
              Title = string.Format("{0}{1}", title, 2),
              Slug = string.Format("{0}{1}", slug, 2),
              Description = string.Format("{0}{1}", description, 4),
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
              Title = string.Format("{0}{1}", title, 2),
              Slug = string.Format("{0}{1}", slug, 2),
              Description = string.Format("{0}{1}", description, 4),
              CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(2))),
              Published = false
          }
      }))
      .AndAlso()
      .ShouldPassForThe<ActionAttributes>(attributes =>
      {
          Assert.Equal(5, attributes.Count());
      });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Edit_article_with_same_title_and_slug_should_return_validation_error(
     string fullName,
     string email,
     string password,
     string name,
     string title,
     string slug,
     string description)
     => Test.AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
     () =>
     {
         MyMvc
               .Pipeline()
               .ShouldMap(request => request
                 .WithMethod(HttpMethod.Put)
                 .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
                 .WithLocation("api/v1.0/articles/edit/2")
                 .WithJsonBody(
                        string.Format(@"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                            string.Format("{0}{1}", title, 1),
                            string.Format("{0}{1}", slug, 1),
                            string.Format("{0}{1}", description, 4))
                 )
               )
               .To<ArticlesController>(c => c.Edit(2, new()
               {
                   ArticleJson = new()
                   {
                       Title = string.Format("{0}{1}", title, 1),
                       Slug = string.Format("{0}{1}", slug, 1),
                       Description = string.Format("{0}{1}", description, 4),
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
         { "ArticleJson.Title", new[] { "'Article Json Title' must be unique." } },
         { "ArticleJson.Slug", new[] { "'Article Json Slug' must be unique." } },
     });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Edit_article_with_max_chars_should_return_validation_error(
     string fullName,
     string email,
     string password,
     string name,
     string title,
     string slug,
     string description)
    => Test.AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
    () =>
    {
        MyMvc
        .Pipeline()
        .ShouldMap(request => request
          .WithMethod(HttpMethod.Put)
          .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
          .WithLocation("api/v1.0/articles/edit/2")
          .WithJsonBody(
                 string.Format(@"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                     string.Format("{0}{1}", ValidMaxxTitleLength, "ab"),
                     string.Format("{0}{1}", ValidMaxxTitleLength, "ab"),
                     string.Format("{0}{1}", ValidMaxxDescriptionLength, "ab"))
          )
        )
        .To<ArticlesController>(c => c.Edit(2, new()
        {
            ArticleJson = new()
            {
                Title = string.Format("{0}{1}", ValidMaxxTitleLength, "ab"),
                Slug = string.Format("{0}{1}", ValidMaxxTitleLength, "ab"),
                Description = string.Format("{0}{1}", ValidMaxxDescriptionLength, "ab")
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
           { "ArticleJson.Title", new[] { "The length of 'Article Json Title' must be 320 characters or fewer. You entered 321 characters." } },
           { "ArticleJson.Slug", new[] { "The length of 'Article Json Slug' must be 320 characters or fewer. You entered 321 characters." } },
    });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Edit_article_with_min_chars_should_return_validation_error(
     string fullName,
     string email,
     string password,
     string name,
     string title,
     string slug,
     string description)
    => Test.AssertValidationErrorsException<MyTested.AspNetCore.Mvc.Exceptions.ValidationErrorsAssertionException>(
    () =>
    {
        MyMvc
        .Pipeline()
            .ShouldMap(request => request
              .WithMethod(HttpMethod.Put)
              .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
              .WithLocation("api/v1.0/articles/edit/2")
              .WithJsonBody(
                     string.Format(@"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
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
            .ShouldReturn();
    }, new Dictionary<string, string[]>
    {
                { "ArticleJson.Title", new[] { "The length of 'Article Json Title' must be at least 2 characters. You entered 1 characters." } },
                { "ArticleJson.Slug", new[] { "The length of 'Article Json Slug' must be at least 2 characters. You entered 1 characters." } },
                { "ArticleJson.Description", new[] { "The length of 'Article Json Description' must be at least 2 characters. You entered 1 characters." } },
    });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Edit_article_with_wrong_id_should_fail(
     string fullName,
     string email,
     string password,
     string name,
     string title,
     string slug,
     string description)
    => Test.AssertException<MyTested.AspNetCore.Mvc.Exceptions.InvocationAssertionException>(
    () =>
    {
        MyMvc
         .Pipeline()
         .ShouldMap(request => request
           .WithMethod(HttpMethod.Put)
           .WithHeaderAuthorization(StaticTestData.GetJwtBearerAdministratorRole(email, 1))
           .WithLocation("api/v1.0/articles/edit/5")
           .WithJsonBody(
                  string.Format(@"{{""article"":{{""title"": ""{0}"", ""slug"": ""{1}"", ""description"": ""{2}""}}}}",
                      string.Format("{0}{1}", title, 4),
                      string.Format("{0}{1}", slug, 4),
                      string.Format("{0}{1}", description, 4))
           )
         )
         .To<ArticlesController>(c => c.Edit(5, new()
         {
             ArticleJson = new()
             {
                 Title = string.Format("{0}{1}", title, 4),
                 Slug = string.Format("{0}{1}", slug, 4),
                 Description = string.Format("{0}{1}", description, 4),
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
    }, string.Format(FromBaseDomainException.Replace(Environment.NewLine, ""), "Edit", "ArticlesController", "NotFoundException", "request", 5));


    [Theory]
    [MemberData(nameof(ValidData))]
    public void Get_article_should_return_success_with_data(
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
              Title = string.Format("{0}{1}", title, 4),
              Slug = string.Format("{0}{1}", slug, 4),
              Description = string.Format("{0}{1}", description, 4),
              CreatedAt = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.FromDateTime(DateTime.Today.AddSeconds(4))),
              TagList = Enumerable
                         .Range(1, 3)
                         .Select(i =>
                         {
                             int r = 0;
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
                         }).ToList(),

          }
      }))
      .AndAlso()
      .ShouldPassForThe<ActionAttributes>(attributes =>
      {
          Assert.Equal(5, attributes.Count());
      });


    [Theory]
    [MemberData(nameof(ValidData))]
    public void Get_article_with_wrong_id_should_fail(
     string fullName,
     string email,
     string password,
     string name,
     string title,
     string slug,
     string description)
    => Test.AssertException<MyTested.AspNetCore.Mvc.Exceptions.InvocationAssertionException>(
    () =>
    {
        MyMvc
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
          .ShouldReturn();
    }, string.Format(FromBaseDomainException.Replace(Environment.NewLine, ""), "Details", "ArticlesController", "NotFoundException", "request", 6));

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Delete_article_should_return_success_with_article_id(
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
      .ShouldPassForThe<ActionAttributes>(attributes =>
      {
          Assert.Equal(5, attributes.Count());
      });

    [Theory]
    [MemberData(nameof(ValidData))]
    public void Delete_non_existing_article_should_return_success_with_negative_one(
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
      .ShouldPassForThe<ActionAttributes>(attributes =>
      {
          Assert.Equal(5, attributes.Count());
      });

    public static IEnumerable<object[]> ValidData()
    {
        yield return new object[]
        {
            ValidMinUserNameLength,
            //Must be valid email address
            $"{ValidMinEmailLength}@a.bcde",
            //Password must contain Upper case, lower case, number, special symbols
            string.Format("U!{0}",  ValidMinPasswordLength),

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
            string.Format("U!{0}", ValidMaxPasswordLength),

            ValidMaxxNameLength,

            ValidMaxxTitleLength,
            ValidMaxxTitleLength,
            ValidMaxxDescriptionLength,
        };
    }
}

