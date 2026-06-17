using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

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
//.AddJwtBearer("Bearer", options =>
//{
//    options.Authority = builder.Configuration["AzureAD:Authority"];
//    options.Audience = builder.Configuration["AzureAD:Audience"];
//    // options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };

//    options.Events = new JwtBearerEvents
//    {
//        OnAuthenticationFailed = context =>
//        {
//            Console.WriteLine("Auth failed: " + context.Exception.Message);
//            return Task.CompletedTask;
//        }
//    };
//});

// builder.Services.AddControllers();

builder.Services.AddAuthorization(options =>
{
    // Add centralized app policies (AdminAccess, Authenticated)
    //options.AddAppAuthorizationPolicies();

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

// builder.Services.AddRequiredScopeAuthorization();
// builder.Services.AddRequiredScopeOrAppPermissionAuthorization();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();
app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


//// Add services to the container.
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
//    .EnableTokenAcquisitionToCallDownstreamApi()
//    // .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
//    .AddInMemoryTokenCaches();

//builder.Services.AddControllers();

//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//if (!app.Environment.IsDevelopment())
//{
//    app.UseHttpsRedirection();
//}

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//app.Run();
