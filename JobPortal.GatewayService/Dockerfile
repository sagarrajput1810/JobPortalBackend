FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JobPortal.GatewayService/JobPortal.GatewayService.csproj", "JobPortal.GatewayService/"]
RUN dotnet restore "JobPortal.GatewayService/JobPortal.GatewayService.csproj"
COPY . .
WORKDIR "/src/JobPortal.GatewayService"
RUN dotnet build "JobPortal.GatewayService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "JobPortal.GatewayService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobPortal.GatewayService.dll"]
