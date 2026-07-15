FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Directory.Build.props", "src/"]
COPY ["src/Directory.Packages.props", "src/"]

COPY ["src/Web.Api/Web.Api.csproj", "src/Web.Api/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Core/Core.csproj", "src/Core/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Shareds/Shared.Kernel/Shared.Kernel.csproj", "src/Shareds/Shared.Kernel/"]
COPY ["src/Shareds/Shared.Web/Shared.Web.csproj", "src/Shareds/Shared.Web/"]

RUN dotnet restore "src/Web.Api/Web.Api.csproj"

COPY . .
WORKDIR "/src/src/Web.Api"
RUN dotnet build "Web.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Web.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web.Api.dll"]