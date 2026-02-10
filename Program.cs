using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ScioApp.Components;
using ScioApp.Data;
using ScioApp.Hubs;
using ScioApp.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Database Context
var connectionString = builder.Environment.IsDevelopment() 
    ? builder.Configuration.GetConnectionString("DefaultConnection")
    : builder.Configuration.GetConnectionString("ProductionConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string not found.");
}

builder.Services.AddDbContext<ScioDbContext>(options =>
    options.UseSqlServer(connectionString));

// Unique cookie and antiforgery names to avoid conflict with root app
builder.Services.AddAntiforgery(options => 
{
    options.Cookie.Name = "Scio_Antiforgery";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddDataProtection()
    .SetApplicationName("ScioApp");

// Authentication & Authorization
builder.Services.AddAuthentication(options => 
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.Name = "Scio_Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    })
    .AddCookie("TempCookie") // Temporary scheme for Google handshake
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "YOUR_GOOGLE_CLIENT_ID";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "YOUR_GOOGLE_CLIENT_SECRET";
        options.SignInScheme = "TempCookie";
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Custom Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAIService, GeminiAIService>();
builder.Services.AddHttpContextAccessor();

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

app.UsePathBase("/scio");

// ENABLE DETAILED ERRORS even in production for debugging
app.UseDeveloperExceptionPage();

// HTTPS and Security
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Forwarded Headers for HTTPS detection behind proxies
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ScioHub>("/sciohub");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
