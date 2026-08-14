using BookShelves.WebApi.AuthorsDataAccess;
using CommunityToolkit.Datasync.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace BookShelves.WebApi.Tests.Auth;

public sealed class AuthorsWebApiFactory : WebApplicationFactory<Program>
{
    private readonly AuthorsRepositoryState repositoryState = new();

    public int GetInvocationCount(string methodName) => repositoryState.GetInvocationCount(methodName);

    public void ResetRepositoryState() => repositoryState.Reset();

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

            services.AddSingleton<IRepository<Author>>(AuthorsRepositoryProxy.Create(repositoryState));
        });
    }

    private sealed class AuthorsRepositoryState
    {
        private readonly ConcurrentDictionary<string, Author> store = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, int> invocationCounts = new(StringComparer.Ordinal);

        public IQueryable<Author> AsQueryable() => store.Values.AsQueryable();

        public Author? Read(string id)
        {
            return store.TryGetValue(id, out var author) ? author : null;
        }

        public void Create(Author author)
        {
            if (string.IsNullOrWhiteSpace(author.Id))
            {
                author.Id = Guid.NewGuid().ToString("N");
            }

            store[author.Id] = author;
        }

        public void Replace(string id, Author author)
        {
            if (string.IsNullOrWhiteSpace(author.Id))
            {
                author.Id = id;
            }

            store[id] = author;
        }

        public void Delete(string id)
        {
            store.TryRemove(id, out _);
        }

        public void Increment(string methodName)
        {
            invocationCounts.AddOrUpdate(methodName, 1, (_, current) => current + 1);
        }

        public int GetInvocationCount(string methodName)
        {
            return invocationCounts.TryGetValue(methodName, out var count) ? count : 0;
        }

        public void Reset()
        {
            store.Clear();
            invocationCounts.Clear();
        }
    }

    private class AuthorsRepositoryProxy : DispatchProxy
    {
        private AuthorsRepositoryState state = null!;

        public static IRepository<Author> Create(AuthorsRepositoryState state)
        {
            var proxy = DispatchProxy.Create<IRepository<Author>, AuthorsRepositoryProxy>();
            if (proxy is AuthorsRepositoryProxy implementation)
            {
                implementation.state = state;
            }

            return proxy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod is null)
            {
                return null;
            }

            state.Increment(targetMethod.Name);

            switch (targetMethod.Name)
            {
                case "AsQueryable":
                    return state.AsQueryable();
                case "ReadAsync":
                    return CreateResult(targetMethod.ReturnType, state.Read(args?[0] as string ?? string.Empty));
                case "CreateAsync":
                    if (args?[0] is Author created)
                    {
                        state.Create(created);
                    }

                    return CreateResult(targetMethod.ReturnType);
                case "ReplaceAsync":
                    if (args?[0] is string id && args.Length > 1 && args[1] is Author replaced)
                    {
                        state.Replace(id, replaced);
                    }

                    return CreateResult(targetMethod.ReturnType);
                case "DeleteAsync":
                    if (args?[0] is string deleteId)
                    {
                        state.Delete(deleteId);
                    }

                    return CreateResult(targetMethod.ReturnType);
                default:
                    return CreateResult(targetMethod.ReturnType, state.AsQueryable());
            }
        }

        private static object? CreateResult(Type returnType, object? value = null)
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
                    var result = CreateGenericResult(genericArgument, value);
                    return typeof(Task)
                        .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Single(method => method.Name == nameof(Task.FromResult) && method.IsGenericMethod)
                        .MakeGenericMethod(genericArgument)
                        .Invoke(null, new[] { result });
                }

                if (genericType == typeof(ValueTask<>))
                {
                    var result = CreateGenericResult(genericArgument, value);
                    return typeof(ValueTask)
                        .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .Single(method => method.Name == "FromResult" && method.IsGenericMethod)
                        .MakeGenericMethod(genericArgument)
                        .Invoke(null, new[] { result });
                }
            }

            return CreateGenericResult(returnType, value);
        }

        private static object? CreateGenericResult(Type type, object? value)
        {
            if (value is not null && type.IsInstanceOfType(value))
            {
                return value;
            }

            if (type == typeof(IQueryable<Author>))
            {
                return Array.Empty<Author>().AsQueryable();
            }

            if (type == typeof(IEnumerable<Author>))
            {
                return Array.Empty<Author>();
            }

            if (type == typeof(List<Author>))
            {
                return new List<Author>();
            }

            if (typeof(IQueryable).IsAssignableFrom(type))
            {
                return Array.Empty<Author>().AsQueryable();
            }

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                return Array.Empty<Author>();
            }

            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
    }
}
