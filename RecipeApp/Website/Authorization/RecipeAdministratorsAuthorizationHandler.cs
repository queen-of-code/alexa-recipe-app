using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Threading.Tasks;

using RecipeApp.Core.ExternalModels;

namespace Website.Authorization
{
    public class RecipeAdministratorsAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, RecipeModel>
    {
        protected override Task HandleRequirementAsync(
                                              AuthorizationHandlerContext context,
                                              OperationAuthorizationRequirement requirement,
                                              RecipeModel resource)
        {
            if (context.User == null)
            {
                return Task.CompletedTask;
            }

            // Administrators can do anything.
            if (context.User.IsInRole(Constants.RecipeAdministratorsRole))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
