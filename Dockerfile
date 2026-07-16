# ══════════════════════════════════════════════════════════════════════════════
# Stage 1 — BUILD
# Uses the full .NET SDK to restore packages and publish the app
# ══════════════════════════════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file and project file first (for layer caching — faster rebuilds)
COPY CattleFarm.slnx ./
COPY CattleFarm/CattleFarm.csproj CattleFarm/

# Restore NuGet packages (cached unless .csproj changes)
RUN dotnet restore CattleFarm.slnx

# Copy the rest of the source code
COPY CattleFarm/ CattleFarm/

# Publish — Release mode, self-contained output to /app/publish
RUN dotnet publish CattleFarm/CattleFarm.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ══════════════════════════════════════════════════════════════════════════════
# Stage 2 — RUNTIME
# Lightweight ASP.NET runtime image (no SDK bloat)
# ══════════════════════════════════════════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for healthchecks (optional but useful)
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN addgroup --system cattlefarm && adduser --system --ingroup cattlefarm cattlefarm

# Copy published output from build stage
COPY --from=build /app/publish .

# Create directories for uploads and logs (will be volume-mounted in production)
RUN mkdir -p wwwroot/uploads/avatars \
             wwwroot/uploads/cattle \
             wwwroot/uploads/farms \
             wwwroot/uploads/products \
             wwwroot/uploads/workers \
             wwwroot/uploads/doctors \
             wwwroot/uploads/task-proofs \
             wwwroot/uploads/licenses \
             logs \
    && chown -R cattlefarm:cattlefarm /app

# Switch to non-root user
USER cattlefarm

# ASP.NET Core default port
EXPOSE 8080

# Environment — Production by default in container
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Health check — pings /Health endpoint every 30 seconds
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/Health || exit 1

ENTRYPOINT ["dotnet", "CattleFarm.dll"]
