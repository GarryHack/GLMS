# ── Stage 1: SDK build ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore — copy project files first for layer caching
COPY GLMS.Web/GLMS.Web.csproj  GLMS.Web/
COPY GLMS.Api/GLMS.Api.csproj  GLMS.Api/
RUN dotnet restore GLMS.Api/GLMS.Api.csproj

# Copy full source and publish
COPY GLMS.Web/ GLMS.Web/
COPY GLMS.Api/ GLMS.Api/
RUN dotnet publish GLMS.Api/GLMS.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:ErrorOnDuplicatePublishOutputFiles=false

# ── Stage 2: Runtime image ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create uploads directory for PDF storage
RUN mkdir -p /app/wwwroot/uploads

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "GLMS.Api.dll"]
