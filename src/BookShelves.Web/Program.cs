using BookShelves.Shared;
using BookShelves.Web.Components;
using BookShelves.WebShared.Data;
using BookShelves.Shared.DataInterfaces;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAuthentication();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddRazorClassLibraryServices();
builder.Services.AddScoped(sp =>
        new HttpClient
        { BaseAddress = new Uri(builder.Configuration["API_Uri"] ?? builder.Configuration["API_Prefix"] ?? builder.Environment.WebRootPath) }
    );
builder.Services.AddScoped<IBooksDataService, BooksDataService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<WebApp>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(BookShelves.Shared._Imports).Assembly);

app.Run();
