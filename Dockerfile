# Use the official .NET SDK image for the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy production project files for dependency restore
COPY Notino_project/Notino_project.csproj ./Notino_project/
COPY Notino_project.Application/Notino_project.Application.csproj ./Notino_project.Application/
COPY Notino_project.Contract/Notino_project.Contract.csproj ./Notino_project.Contract/
COPY Notino_project.Core/Notino_project.Core.csproj ./Notino_project.Core/
COPY Notino_project.Infrastructure/Notino_project.Infrastructure.csproj ./Notino_project.Infrastructure/

RUN dotnet restore Notino_project/Notino_project.csproj

# Copy the rest of the code
COPY . .

# Build and publish the main project
WORKDIR /app/Notino_project
RUN dotnet publish -c Release --no-restore -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/out .

# Expose the application port
EXPOSE 80

# Set the entry point
ENTRYPOINT ["dotnet", "Notino_project.dll"]