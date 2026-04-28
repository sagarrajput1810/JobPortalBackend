FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JobPortal.ProfileService/JobPortal.ProfileService.csproj", "JobPortal.ProfileService/"]
RUN dotnet restore "JobPortal.ProfileService/JobPortal.ProfileService.csproj"
COPY . .
WORKDIR "/src/JobPortal.ProfileService"
RUN dotnet build "JobPortal.ProfileService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "JobPortal.ProfileService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobPortal.ProfileService.dll"]
