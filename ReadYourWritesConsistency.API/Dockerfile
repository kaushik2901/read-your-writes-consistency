# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers to leverage Docker cache
COPY ["ReadYourWritesConsistency.API/ReadYourWritesConsistency.API.csproj", "ReadYourWritesConsistency.API/"]
RUN dotnet restore "ReadYourWritesConsistency.API/ReadYourWritesConsistency.API.csproj"

# Copy everything else and build
COPY ["ReadYourWritesConsistency.API/", "ReadYourWritesConsistency.API/"]
WORKDIR "/src/ReadYourWritesConsistency.API"

# Install dependencies for Native AOT compilation
RUN apt-get update && apt-get install -y clang zlib1g-dev

RUN dotnet publish "ReadYourWritesConsistency.API.csproj" -c Release -o /app/publish -r linux-x64 /p:PublishAot=true

# Stage 2: Create the final runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Set the entry point for the container
ENTRYPOINT ["./ReadYourWritesConsistency.API"]