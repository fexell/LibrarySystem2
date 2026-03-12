using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Library2.Middlewares;
using Library2.Contexts;
using Library2.Interfaces;
using Library2.Services;

var builder = WebApplication.CreateBuilder( args );

// ── Databas ─────────────────────────────────────────────────────
builder.Services.AddDbContext<LibraryContext>( options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString( "DefaultConnection" )
    )
);

// ── Repository Pattern ──────────────────────────────────────────
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();

// ── Razor Pages & Blazor Server ─────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// ── Autentisering / Filter ──────────────────────────────────────
builder.Services.AddScoped<AuthenticateFilter>();

builder.Services.Configure<MvcOptions>( options => {
    options.Filters.Add<AuthenticateFilter>();
} );

var app = builder.Build();

// ── Felhantering ────────────────────────────────────────────────
if ( !app.Environment.IsDevelopment() ) {
    app.UseExceptionHandler( "/Error" );
    app.UseHsts();
}

// ── Middleware-pipeline ─────────────────────────────────────────
app.UseHttpsRedirection();
app.UseWebSockets();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

// ── Routing: Razor Pages ─────────────────────────────────────────
app.MapRazorPages().WithStaticAssets();

// ── Routing: Blazor Server ───────────────────────────────────────
app.MapBlazorHub();

// Fallback för Blazor-sidor
app.MapFallbackToPage( "/blazor", "/_BlazorHost" );
app.MapFallbackToPage( "/blazor/{*path}", "/_BlazorHost" );

// ── Databas-seedning ────────────────────────────────────────────
using ( var scope = app.Services.CreateScope() ) {
    var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    await Library2.Infrastructure.DbSeeder.SeedAsync( context );
}

app.Run();
