# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ParkLite.sln ./
COPY ParkLite.Api/ ParkLite.Api/
COPY ParkLite.Tests/ ParkLite.Tests/
WORKDIR /src/ParkLite.Api
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "ParkLite.Api.dll"]
