using System.Reflection;
using System.Linq.Expressions;
using BookShelves.WebApi.AuthorsDataAccess;
using CommunityToolkit.Datasync.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookShelves.WebApi.Tests.Auth;

public sealed class AuthorItemWebApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CosmosDBConnectionString"] = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDjDh4lYxT4j5s0Yxv9rQ2Yeq4i8M9Pv4Y0Qf9R7xM2f5d8Y0xR3M3Q1H7sA==;",
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

            services.AddSingleton<IRepository<AuthorItem>>(AuthorItemRepositoryProxy.Create());
        });
    }

    private class AuthorItemRepositoryProxy : DispatchProxy
    {
        public static IRepository<AuthorItem> Create()
        {
            return DispatchProxy.Create<IRepository<AuthorItem>, AuthorItemRepositoryProxy>();
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
            if (type == typeof(IQueryable<AuthorItem>))
            {
                return Array.Empty<AuthorItem>().AsQueryable();
            }

            if (type == typeof(IEnumerable<AuthorItem>))
            {
                return Array.Empty<AuthorItem>();
            }

            if (type == typeof(List<AuthorItem>))
            {
                return new List<AuthorItem>();
            }

            if (typeof(IQueryable).IsAssignableFrom(type))
            {
                return Array.Empty<AuthorItem>().AsQueryable();
            }

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                return Array.Empty<AuthorItem>();
            }

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
    }
}
