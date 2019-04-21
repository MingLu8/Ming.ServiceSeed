FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["ServiceSeed/ServiceSeed.csproj", "ServiceSeed/"]
RUN dotnet restore "ServiceSeed/ServiceSeed.csproj"
COPY . .
WORKDIR "/src/ServiceSeed"
RUN dotnet build "ServiceSeed.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ServiceSeed.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ServiceSeed.dll"]