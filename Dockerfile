FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SisControle.Domain/SisControle.Domain.csproj SisControle.Domain/
COPY SisControle.Application/SisControle.Application.csproj SisControle.Application/
COPY SisControle.Infrastructure/SisControle.Infrastructure.csproj SisControle.Infrastructure/
COPY SisControle.Web/SisControle.Web.csproj SisControle.Web/
RUN dotnet restore SisControle.Web/SisControle.Web.csproj

COPY . .
RUN dotnet publish SisControle.Web/SisControle.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SisControle.Web.dll"]
