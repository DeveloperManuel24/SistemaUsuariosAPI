public class CustomAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public CustomAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            var authorizeAttribute = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

            // Si la ruta tiene el atributo [Authorize], entonces se debe validar la autenticación
            if (authorizeAttribute != null)
            {
                var user = context.User;
                if (!user.Identity.IsAuthenticated)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("No estás autenticado");
                    return;
                }

                // Verificar si el usuario tiene el claim "esadmin" solo si es necesario
                if (!user.HasClaim(c => c.Type == "esadmin"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("No eres administrador");
                    return;
                }
            }
        }

        await _next(context);
    }
}

// Extensión para agregar el middleware en la tubería
public static class CustomAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomAuthorizationMiddleware>();
    }
}
