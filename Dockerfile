# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY IkkonAdmin.Web/IkkonAdmin.Web.csproj IkkonAdmin.Web/
RUN dotnet restore IkkonAdmin.Web/IkkonAdmin.Web.csproj

COPY . .
RUN dotnet publish IkkonAdmin.Web/IkkonAdmin.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "IkkonAdmin.Web.dll"]
