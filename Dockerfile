# Use the official .NET SDK image for the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything
COPY Notino_project.sln ./
COPY Notino_project.Models/*.csproj ./Notino_project.Models/
COPY Notino_project.Repositories.Interfaces/*.csproj ./Notino_project.Repositories.Interfaces/
COPY Notino_project.FakeInMemoryRepo/*.csproj ./Notino_project.FakeInMemoryRepo/
COPY Notino_project.Services.Interfaces/*.csproj ./Notino_project.Services.Interfaces/
COPY Notino_project.Services/*.csproj ./Notino_project.Services/
COPY Notino_project.UnitTests/*.csproj ./Notino_project.UnitTests/
COPY Notino_project.IntegrationTests/*.csproj ./Notino_project.IntegrationTests/
COPY Notino_project/*.csproj ./Notino_project/

# Restore as distinct layers
RUN dotnet restore

# Copy the rest of the code
COPY . .

# Build and publish the main project
WORKDIR /app/Notino_project
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/out .

# Expose the application port
EXPOSE 80

# Set the entry point
ENTRYPOINT ["dotnet", "Notino_project.dll"]