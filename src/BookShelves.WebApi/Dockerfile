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
ARG RUN_ID
# Set our workspace inside the container
WORKDIR /src

# 1. OPTIONAL CACHE BUSTER: This will now successfully print because 
# paths match and the build will not abort early!
RUN echo "=== RUN ID: ${RUN_ID} ===" && ls -la

# 2. Copy .NET global configuration settings first (Crucial for .NET 10/Directory properties)
COPY ["global.json", "./"]
COPY ["Directory.Build.Props", "./"]

# 3. Copy project files using their true paths from your repository root tree
COPY ["BookShelves.WebApi/BookShelves.WebApi.csproj", "BookShelves.WebApi/"]
COPY ["BookShelves.Web.Shared/BookShelves.Web.Shared.csproj", "BookShelves.Web.Shared/"]

# Note: Based on your file tree output, "BookShelves.Shared" does not exist at your root level.
# If it is missing, comment out or delete the line below so it doesn't cause a failure:
# COPY ["src/BookShelves.Shared/BookShelves.Shared.csproj", "BookShelves.Shared/"]

# 4. Restore dependencies using the nested project path
RUN dotnet restore "BookShelves.WebApi/BookShelves.WebApi.csproj"

# 5. Copy the remaining repository source code into the container /src directory
COPY . .

# 6. Change working directories to build the targeted app context
WORKDIR "/src/BookShelves.WebApi"
RUN dotnet build "BookShelves.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build


# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BookShelves.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookShelves.WebApi.dll"]