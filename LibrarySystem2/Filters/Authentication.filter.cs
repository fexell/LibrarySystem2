using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Library2.Middlewares;

// Ett filter som kollar om användaren är inloggad (baserat på en cookie)
// och är inte en av de publika Blazor- eller framework-sidorna.
// Om inte, omdirigeras de till inloggningssidan
// eller får ett 401 Unauthorized för API-anrop.
public class AuthenticateFilter : IAsyncAuthorizationFilter {
    private static readonly string[] BypassPrefixes = [ "/_blazor", "/_framework" ];

    public Task OnAuthorizationAsync( AuthorizationFilterContext context ) {
        var httpContext = context.HttpContext;
        var path = httpContext.Request.Path;
        var username = httpContext.Request.Cookies[ "username" ];

        Log( path, username );

        if ( IsFrameworkPath( path ) )
            return Task.CompletedTask;

        if ( path.StartsWithSegments( "/Login" ) ) {
            if ( !string.IsNullOrEmpty( username ) )
                context.Result = new RedirectToPageResult( "/Index" );
            return Task.CompletedTask;
        }

        if ( string.IsNullOrEmpty( username ) ) {
            Log( path, username, blocked: true );
            context.Result = path.StartsWithSegments( "/api" )
                ? new UnauthorizedResult()
                : new RedirectToPageResult( "/Login" );
            return Task.CompletedTask;
        }

        httpContext.Items[ "username" ] = username;
        return Task.CompletedTask;
    }

    private static bool IsFrameworkPath( PathString path ) =>
        BypassPrefixes.Any( prefix => path.StartsWithSegments( prefix ) );

    private static void Log( PathString path, string? username, bool blocked = false ) {
        if ( blocked )
            Console.WriteLine( $"[AUTH] Blocking unauthenticated request to: {path}" );
        else
            Console.WriteLine( $"[AUTH] Path: {path} | Username: {username}" );
    }
}