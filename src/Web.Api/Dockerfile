FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only csproj files for dependency restoration (this layer will be cached)
COPY ["Web.Api/Web.Api.csproj", "Web.Api/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Shareds/Shared.Kernel/Shared.Kernel.csproj", "Shareds/Shared.Kernel/"]
COPY ["Shareds/Shared.Web/Shared.Web.csproj", "Shareds/Shared.Web/"]

# Copy build configuration files for dependency restoration
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

# Restore dependencies (this layer will be cached and only rebuilt when csproj or config files change)
RUN dotnet restore "Web.Api/Web.Api.csproj"

# Copy all source code (this layer will only be rebuilt when source files change)
COPY . .

WORKDIR "Web.Api"
RUN dotnet build "Web.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Web.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web.Api.dll"]