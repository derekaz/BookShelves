# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR ./src
COPY ["BookShelves.Web/BookShelves.Web/BookShelves.Web.csproj", "BookShelves.Web/BookShelves.Web/"]
COPY ["BookShelves.Web.Shared/BookShelves.Web.Shared.csproj", "BookShelves.Web.Shared/"]
COPY ["BookShelves.Shared/BookShelves.Shared.csproj", "BookShelves.Shared/"]
COPY ["BookShelves.Web/BookShelves.Web.Client/BookShelves.Web.Client.csproj", "BookShelves.Web/BookShelves.Web.Client/"]
RUN dotnet restore "./BookShelves.Web/BookShelves.Web/BookShelves.Web.csproj"
COPY . .
WORKDIR "./src/BookShelves.Web/BookShelves.Web"
RUN dotnet build "./BookShelves.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BookShelves.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookShelves.Web.dll"]