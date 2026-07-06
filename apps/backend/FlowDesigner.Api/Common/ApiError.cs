using Microsoft.AspNetCore.Mvc;

namespace FlowDesigner.Api.Common;

public static class ApiErrorCodes
{
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string NotFound = "NOT_FOUND";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string RevisionConflict = "REVISION_CONFLICT";
    public const string InternalError = "INTERNAL_ERROR";
}

public sealed record ApiErrorResponse(
    string Code,
    string Message,
    string TraceId,
    IReadOnlyDictionary<string, string[]>? Details = null);

public static class ApiError
{
    public static ApiErrorResponse Create(
        HttpContext httpContext,
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? details = null)
    {
        return new ApiErrorResponse(code, message, httpContext.TraceIdentifier, details);
    }

    public static IReadOnlyDictionary<string, string[]> ValidationDetails(string field, string message)
    {
        return new Dictionary<string, string[]>
        {
            [field] = [message],
        };
    }

    public static ActionResult<T> BadRequest<T>(ControllerBase controller, string message, string field = "request")
    {
        return controller.BadRequest(Create(
            controller.HttpContext,
            ApiErrorCodes.ValidationError,
            message,
            ValidationDetails(field, message)));
    }

    public static ActionResult<T> NotFound<T>(ControllerBase controller, string message)
    {
        return controller.NotFound(Create(controller.HttpContext, ApiErrorCodes.NotFound, message));
    }

    public static ActionResult<T> Conflict<T>(ControllerBase controller, string code, string message)
    {
        return controller.Conflict(Create(controller.HttpContext, code, message));
    }

    public static ObjectResult Internal(ControllerBase controller, string message)
    {
        return controller.StatusCode(500, Create(controller.HttpContext, ApiErrorCodes.InternalError, message));
    }
}
