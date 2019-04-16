using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using RecipeApp.Core.ExternalModels;
using System.Threading.Tasks;

namespace Website.Authorization
{
    public class IsRecipeOwnerAuthorizationHandler :  AuthorizationHandler<OperationAuthorizationRequirement, RecipeModel>
    {
        private readonly UserManager<IdentityUser> _userManager;

        public IsRecipeOwnerAuthorizationHandler(UserManager<IdentityUser>
            userManager)
        {
            _userManager = userManager;
        }

        protected override Task
            HandleRequirementAsync(AuthorizationHandlerContext context,
                                   OperationAuthorizationRequirement requirement,
                                   RecipeModel resource)
        {
            if (context.User == null || resource == null)
            {
                // Return Task.FromResult(0) if targeting a version of
                // .NET Framework older than 4.6:
                return Task.CompletedTask;
            }

            // If we're not asking for CRUD permission, return.

            //if (requirement.Name != Constants.CreateOperationName &&
            //    requirement.Name != Constants.ReadOperationName &&
            //    requirement.Name != Constants.UpdateOperationName &&
            //    requirement.Name != Constants.DeleteOperationName)
            //{
            //    return Task.CompletedTask;
            //}

            if (resource.UserId.ToString() == _userManager.GetUserId(context.User))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

