var builder = DistributedApplication.CreateBuilder(args);

var webApi = builder.AddProject<Projects.BookShelves_WebApi>("webapi");

builder.AddProject<Projects.BookShelves_Web>("webapp")
    .WithReference(webApi)
    .WithEnvironment("BooksApi__BaseUrl", webApi.GetEndpoint("https"))
    .WithEnvironment("WeatherApi__BaseUrl", webApi.GetEndpoint("https"))
    .WaitFor(webApi);

builder.Build().Run();
