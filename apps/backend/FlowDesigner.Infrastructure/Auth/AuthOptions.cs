namespace FlowDesigner.Infrastructure.Auth;

public sealed class AuthOptions
{
    public string Scheme { get; set; } = "Cookies";
    public string LoginRedirectUrl { get; set; } = "http://localhost:5173/login";
    public string CallbackRedirectUrl { get; set; } = "http://localhost:5173/";
}
