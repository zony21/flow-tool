using FlowDesigner.Api.Common;
using FlowDesigner.Application.Interfaces.Authorization;
using FlowDesigner.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FlowDesigner.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute(string permissionCode) : TypeFilterAttribute(typeof(RequirePermissionFilter))
{
    public string PermissionCode { get; } = permissionCode;

    private sealed class RequirePermissionFilter(
        string permissionCode,
        ICurrentUserService currentUserService,
        IPermissionService permissionService) : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!currentUserService.IsAuthenticated())
            {
                context.Result = new UnauthorizedObjectResult(
                    ApiError.Create(context.HttpContext, ApiErrorCodes.Unauthorized, "Authentication is required."));
                return;
            }

            if (!TryGetProjectId(context, out var projectId))
            {
                context.Result = new BadRequestObjectResult(
                    ApiError.Create(
                        context.HttpContext,
                        ApiErrorCodes.ValidationError,
                        "projectId is required.",
                        ApiError.ValidationDetails("projectId", "projectId is required.")));
                return;
            }

            var userId = currentUserService.GetCurrentUserId();
            if (userId is null)
            {
                context.Result = new UnauthorizedObjectResult(
                    ApiError.Create(context.HttpContext, ApiErrorCodes.Unauthorized, "Authentication is required."));
                return;
            }

            var can = await permissionService.CanAsync(userId.Value, projectId, permissionCode);
            if (!can)
            {
                context.Result = new ObjectResult(
                    ApiError.Create(context.HttpContext, ApiErrorCodes.Forbidden, $"{permissionCode} permission is required."))
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                };
            }
        }

        private static bool TryGetProjectId(AuthorizationFilterContext context, out Guid projectId)
        {
            projectId = default;
            var routeValue = context.RouteData.Values.TryGetValue("projectId", out var rawValue) ? rawValue?.ToString() : null;
            return Guid.TryParse(routeValue, out projectId);
        }
    }
}
