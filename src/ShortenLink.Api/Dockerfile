FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ShortenLink.Api/ShortenLink.Api.csproj", "src/ShortenLink.Api/"]
COPY ["src/ShortenLink.AspNetCore/ShortenLink.AspNetCore.csproj", "src/ShortenLink.AspNetCore/"]
COPY ["src/ShortenLink.Core/ShortenLink.Core.csproj", "src/ShortenLink.Core/"]
COPY ["src/ShortenLink.Infrastructure/ShortenLink.Infrastructure.csproj", "src/ShortenLink.Infrastructure/"]
RUN dotnet restore "src/ShortenLink.Api/ShortenLink.Api.csproj"

COPY . .
RUN dotnet publish "src/ShortenLink.Api/ShortenLink.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ShortenLink.Api.dll"]
