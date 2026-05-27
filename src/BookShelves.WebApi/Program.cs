using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["AzureAD:Authority"];
        options.Audience = builder.Configuration["AzureAD:Audience"];
        // options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Auth failed: " + context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers();

builder.Services.AddAuthorization();
builder.Services.AddRequiredScopeAuthorization();
builder.Services.AddRequiredScopeOrAppPermissionAuthorization();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

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
