
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["/EShop.Api.csproj", "EShop.Api/"]
COPY ["../EShop.Application/EShop.Application.csproj", "EShop.Application/"]
COPY ["../EShop.Contracts/EShop.Contracts.csproj", "EShop.Contracts/"]
COPY ["../EShop.Domain/EShop.Domain.csproj", "EShop.Domain/"]
COPY ["../EShop.Infrastructure/EShop.Infrastructure.csproj", "EShop.Infrastructure/"]
RUN dotnet restore "./EShop.Api/EShop.Api.csproj"
COPY . .
WORKDIR "/src/EShop.Api"
RUN dotnet build "./EShop.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EShop.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EShop.Api.dll"]