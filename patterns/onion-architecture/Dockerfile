# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY OnionArchitecture.slnx ./
COPY src/OnionArch.Domain/*.csproj ./src/OnionArch.Domain/
COPY src/OnionArch.Application/*.csproj ./src/OnionArch.Application/
COPY src/OnionArch.Infrastructure/*.csproj ./src/OnionArch.Infrastructure/
COPY src/OnionArch.Api/*.csproj ./src/OnionArch.Api/

# Restore dependencies
RUN dotnet restore src/OnionArch.Api/OnionArch.Api.csproj

# Copy everything else
COPY src/ ./src/

# Build and publish
WORKDIR /src/src/OnionArch.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Run the application
ENTRYPOINT ["dotnet", "OnionArch.Api.dll"]
