using BlogAngular.Application.Common.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace BlogAngular.Web.Services
{
    public class CurrentUserService : ICurrentUser
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;

            if (user is null)
            {
                throw new InvalidOperationException("This request does not have an authenticated user.");
            }

            this.UserId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }

        public string UserId { get; }
    }
}