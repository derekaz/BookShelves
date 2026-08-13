using BookShelves.WebApi.BooksDataAccess;
using CommunityToolkit.Datasync.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BookShelves.WebApi.Tests.Auth;

public sealed class BooksControllerWebApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        TestHostConfiguration.Apply(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CosmosDBConnectionString"] = TestHostConfiguration.CosmosConnectionString,
                ["AzureAd:Instance"] = "https://login.microsoftonline.com/",
                ["AzureAd:TenantId"] = "test-tenant",
                ["AzureAd:ClientId"] = "test-client"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.AddSingleton<IRepository<Book>>(BooksRepositoryProxy.Create());
        });
    }

    private class BooksRepositoryProxy : DispatchProxy
    {
        public static IRepository<Book> Create()
        {
            return DispatchProxy.Create<IRepository<Book>, BooksRepositoryProxy>();
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod is null)
            {
                return null;
            }

            return CreateResult(targetMethod.ReturnType);
        }

        private static object? CreateResult(Type returnType)
        {
            if (returnType == typeof(void))
            {
                return null;
            }

            if (returnType == typeof(Task))
            {
                return Task.CompletedTask;
            }

            if (returnType == typeof(ValueTask))
            {
                return default(ValueTask);
            }

            if (returnType.IsGenericType)
            {
                var genericType = returnType.GetGenericTypeDefinition();
                var genericArgument = returnType.GetGenericArguments()[0];

                if (genericType == typeof(Task<>))
                {
                    var result = CreateGenericResult(genericArgument);
                    return typeof(Task)
                        .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Single(method => method.Name == nameof(Task.FromResult) && method.IsGenericMethod)
                        .MakeGenericMethod(genericArgument)
                        .Invoke(null, new[] { result });
                }

                if (genericType == typeof(ValueTask<>))
                {
                    var result = CreateGenericResult(genericArgument);
                    return Activator.CreateInstance(returnType, result);
                }
            }

            return CreateGenericResult(returnType);
        }

        private static object? CreateGenericResult(Type type)
        {
            if (type == typeof(IQueryable<Book>))
            {
                return Array.Empty<Book>().AsQueryable();
            }

            if (type == typeof(IEnumerable<Book>))
            {
                return Array.Empty<Book>();
            }

            if (type == typeof(List<Book>))
            {
                return new List<Book>();
            }

            if (typeof(IQueryable).IsAssignableFrom(type))
            {
                return Array.Empty<Book>().AsQueryable();
            }

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                return Array.Empty<Book>();
            }

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
    }
}
