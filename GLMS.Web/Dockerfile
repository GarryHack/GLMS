# ── Stage 1: SDK build ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore — copy project file first for layer caching
COPY GLMS.Web/GLMS.Web.csproj GLMS.Web/
RUN dotnet restore GLMS.Web/GLMS.Web.csproj

# Copy full source and publish
COPY GLMS.Web/ GLMS.Web/
RUN dotnet publish GLMS.Web/GLMS.Web.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ── Stage 2: Runtime image ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "GLMS.Web.dll"]
