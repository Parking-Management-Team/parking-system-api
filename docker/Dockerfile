FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/PBMS.API/PBMS.API.csproj", "src/PBMS.API/"]
COPY ["src/PBMS.Domain/PBMS.Domain.csproj", "src/PBMS.Domain/"]
COPY ["src/PBMS.Application/PBMS.Application.csproj", "src/PBMS.Application/"]
COPY ["src/PBMS.Infrastructure/PBMS.Infrastructure.csproj", "src/PBMS.Infrastructure/"]
COPY ["tests/PBMS.UnitTests/PBMS.UnitTests.csproj", "tests/PBMS.UnitTests/"]

RUN dotnet restore "src/PBMS.API/PBMS.API.csproj"

# Copy everything else and build the project
COPY . .
WORKDIR "/src/src/PBMS.API"
RUN dotnet build "PBMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PBMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PBMS.API.dll"]
