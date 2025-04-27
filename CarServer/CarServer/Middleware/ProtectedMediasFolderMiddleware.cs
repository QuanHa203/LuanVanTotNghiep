
namespace CarServer.Middleware;

public class ProtectedMediasFolderMiddleware : IMiddleware
{
    private readonly string protectedFolder = "/Medias";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = context.Request.Path.Value;

        if (path == null)
            return;

        if (path.StartsWith(protectedFolder, StringComparison.OrdinalIgnoreCase))
        {
            if (context.User.Identity == null || context.User.Identity.IsAuthenticated == false)
            {
                context.Response.StatusCode = 401;
                return;
            }
        }
        await next(context);
    }
}
