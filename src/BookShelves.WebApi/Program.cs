using BookShelves.WebApi.AuthorsDataAccess;
using BookShelves.WebApi.BooksDataAccess;
using CommunityToolkit.Datasync.Server;
using CommunityToolkit.Datasync.Server.Abstractions.Json;
using CommunityToolkit.Datasync.Server.CosmosDb;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Web;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

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

builder.Services.AddRequiredScopeAuthorization();
builder.Services.AddRequiredScopeOrAppPermissionAuthorization();
//builder.Services.AddAuthorization(options =>
//{
//    // Add centralized app policies (AdminAccess, Authenticated)
//    options.AddAppAuthorizationPolicies();

//    // Add API-specific default policy if needed
//    options.FallbackPolicy = options.DefaultPolicy;
//});

// Add CORS if needed for Web client
//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(policy =>
//    {
//        policy.WithOrigins(
//            builder.Configuration["AllowedOrigins"] ?? "https://localhost:7098",
//            "https://localhost:7098",
//            "http://localhost:5261"
//        )
//        .AllowAnyMethod()
//        .AllowAnyHeader()
//        .AllowCredentials();
//    });
//});

string connectionString = builder.Configuration.GetConnectionString("CosmosDBConnectionString")
    ?? throw new ApplicationException("CosmosDBConnectionString is not set");

CosmosClient cosmosClient = new CosmosClient(connectionString,
    new CosmosClientOptions()
    {
        UseSystemTextJsonSerializerWithOptions = new()
        {
            Converters =
            {
                new JsonStringEnumConverter(),
                new DateTimeOffsetConverter(),
                new DateTimeConverter(),
                new TimeOnlyConverter(),
                new SpatialGeoJsonConverter()
            },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        }
    });

builder.Services.AddSingleton(cosmosClient);
builder.Services.AddSingleton<ICosmosTableOptions<Author>>(new CosmosSharedTableOptions<Author>("azmoore-westus2-db1", "azmoore-bookshelvessync-westus2-dbc1"));
builder.Services.AddSingleton(typeof(IRepository<>), typeof(CosmosTableRepository<>));

builder.Services.AddTransient(x =>
{
    IConfiguration? configuration = x.GetService<IConfiguration>();

    return new BookRepository(
        x.GetRequiredService<ILogger<BookRepository>>(),
        new CosmosClient(configuration!["ConnectionStrings:CosmosDBConnectionString"]),
        "azmoore-westus2-db1",
        "azmoore-books-westus2-dbc1"
    );
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

builder.Services.AddDatasyncServices();
builder.Services.AddControllers();

var app = builder.Build();

// Place this at the VERY top of your middleware pipeline, before Auth or Routing
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// If the environment variable from Docker Compose is present, enforce it
if (builder.Configuration["ASPNETCORE_FORWARDEDHEADERS_ENABLED"] == "true")
{
    // Allows handling headers from reverse proxy containers on the internal network
    app.UseForwardedHeaders();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/version", () => new
{
    ProductVersion = typeof(Program).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion ?? "Unknown",
    AssemblyVersion = typeof(Program).Assembly.GetName().Version?.ToString()
}).RequireAuthorization();

app.Run();