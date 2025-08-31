# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers to leverage Docker cache
COPY ["ReadYourWritesConsistency.API/ReadYourWritesConsistency.API.csproj", "ReadYourWritesConsistency.API/"]
RUN dotnet restore "ReadYourWritesConsistency.API/ReadYourWritesConsistency.API.csproj"

# Copy everything else and build
COPY ["ReadYourWritesConsistency.API/", "ReadYourWritesConsistency.API/"]
WORKDIR "/src/ReadYourWritesConsistency.API"

# Publish the application normally without AOT
RUN dotnet publish "ReadYourWritesConsistency.API.csproj" -c Release -o /app/publish

# Stage 2: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "ReadYourWritesConsistency.API.dll"]