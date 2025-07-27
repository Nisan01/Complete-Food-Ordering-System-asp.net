# Use the official ASP.NET 9.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Use the .NET SDK 9.0 image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything and restore dependencies
COPY . .
RUN dotnet restore "FoodOrderingSystem.csproj"

# Publish the app to a folder
RUN dotnet publish "FoodOrderingSystem.csproj" -c Release -o /app/publish

# Final runtime container
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FoodOrderingSystem.dll"]
