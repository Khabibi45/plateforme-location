# Use the official .NET 10 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the solution file and project files
COPY ["PlateformeLocationDisques.sln", "./"]
COPY ["src/PlateformeLocationDisques.WebApi/PlateformeLocationDisques.WebApi.csproj", "src/PlateformeLocationDisques.WebApi/"]
COPY ["PlateformeLocationDisques.Tests/PlateformeLocationDisques.Tests.csproj", "PlateformeLocationDisques.Tests/"]

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build and publish the Web API
WORKDIR "/src/src/PlateformeLocationDisques.WebApi"
RUN dotnet publish "PlateformeLocationDisques.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "PlateformeLocationDisques.WebApi.dll"]
