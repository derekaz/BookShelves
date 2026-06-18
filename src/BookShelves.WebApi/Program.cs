using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using BookShelves.Shared.Services.AuthorizationPolicies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Auth failed: {context.Exception.Message}");
                var exception = context.Exception;
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token is valid!");
                var claims = context.Principal?.Claims;
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                // Put a breakpoint here to ensure the token is physically extracted from headers
                var token = context.Token;
                return Task.CompletedTask;
            }
        };
    },
    options =>
    {
        builder.Configuration.Bind("AzureAd", options);
    });

builder.Services.AddAuthorization(options =>
{
    // Add centralized app policies (AdminAccess, Authenticated)
    options.AddAppAuthorizationPolicies();

    // Add API-specific default policy if needed
    options.FallbackPolicy = options.DefaultPolicy;
});

// Add CORS if needed for Web client
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            builder.Configuration["AllowedOrigins"] ?? "https://localhost:7098",
            "https://localhost:7098",
            "http://localhost:5261"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();