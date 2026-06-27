# Smart Cattle Farm Management System — Setup Guide

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Clone & Open](#clone--open)
3. [Local Development Setup](#local-development-setup)
4. [Database Configuration](#database-configuration)
5. [Migrations](#migrations)
6. [Environment Configuration](#environment-configuration)
7. [Running the Application](#running-the-application)
8. [Production Deployment](#production-deployment)
9. [Database Backup Advice](#database-backup-advice)
10. [Troubleshooting](#troubleshooting)

---

## Prerequisites

| Requirement | Minimum Version | Notes |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 | `dotnet --version` to verify |
| SQL Server | 2019 / SQL Express 2019+ | LocalDB is fine for development |
| Visual Studio / VS Code | Any current | Rider also works |
| Git | 2.x | — |

> **Optional – for email alerts:**  
> A Gmail account with an [App Password](https://support.google.com/accounts/answer/185833) (2-step verification must be enabled), or any SMTP provider.

---

## Clone & Open

```bash
git clone <your-repo-url>
cd CattleFarm
```

Open `CattleFarm.sln` in Visual Studio, or open the folder in VS Code / Rider.

---

## Local Development Setup

### 1. Restore NuGet packages

```bash
dotnet restore
```

### 2. Configure local settings

The main configuration file is `CattleFarm/appsettings.json`.  
Edit the following keys before running for the first time:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=CattleFarmDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connect Timeout=30;"
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-gmail-app-password",
    "FromName": "Smart Cattle Farm",
    "FromAddress": "your-email@gmail.com"
  },
  "SSLCommerz": {
    "StoreId": "YOUR_STORE_ID",
    "StorePassword": "YOUR_STORE_PASSWORD",
    "IsSandbox": true
  }
}
```

> **Tip:** Use `appsettings.Development.json` to override only local values without touching `appsettings.json`.  
> `appsettings.Development.json` is **git-ignored** for safety.

---

## Database Configuration

### SQL Server connection string examples

| Scenario | Connection String |
|---|---|
| SQL Express (Windows Auth) | `Server=.\SQLEXPRESS;Database=CattleFarmDB;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL Express (SQL Auth) | `Server=.\SQLEXPRESS;Database=CattleFarmDB;User Id=sa;Password=YourPass;TrustServerCertificate=True;` |
| SQL Server on another host | `Server=192.168.1.10,1433;Database=CattleFarmDB;User Id=sa;Password=YourPass;TrustServerCertificate=False;` |
| Azure SQL | `Server=tcp:yourserver.database.windows.net,1433;Database=CattleFarmDB;User Id=dbuser@yourserver;Password=...;Encrypt=True;` |

---

## Migrations

All database schema changes are managed through **Entity Framework Core** migrations.

### Apply all pending migrations (first time or after a pull)

```bash
cd CattleFarm
dotnet ef database update --project CattleFarm.csproj
```

### Add a new migration (after modifying a model)

```bash
dotnet ef migrations add <MigrationName> --project CattleFarm.csproj
dotnet ef database update --project CattleFarm.csproj
```

### Roll back to a specific migration

```bash
dotnet ef database update <PreviousMigrationName> --project CattleFarm.csproj
```

### List all migrations

```bash
dotnet ef migrations list --project CattleFarm.csproj
```

> **Note:** Run all `dotnet ef` commands from inside the `CattleFarm/` subfolder (the project that contains the `DbContext`), not the solution root.

---

## Environment Configuration

### Configuration files overview

| File | Purpose | Git-tracked? |
|---|---|---|
| `appsettings.json` | Base defaults (safe placeholders only) | ✅ Yes |
| `appsettings.Development.json` | Local overrides | ❌ No (git-ignored) |
| `appsettings.Production.json` | Production secrets | ❌ **Never commit** |
| `appsettings.Production.example.json` | Production template (no real secrets) | ✅ Yes |

### Creating `appsettings.Production.json`

Copy the provided template and fill in real values:

```bash
cp CattleFarm/appsettings.Production.example.json CattleFarm/appsettings.Production.json
```

Then edit `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_HOST;Database=CattleFarmDB;User Id=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;TrustServerCertificate=False;MultipleActiveResultSets=True;Connect Timeout=30;"
  },
  "Email": {
    "Host": "smtp.your-provider.com",
    "Port": 587,
    "Username": "no-reply@your-domain.com",
    "Password": "REAL_SMTP_PASSWORD",
    "FromName": "Smart Cattle Farm",
    "FromAddress": "no-reply@your-domain.com"
  },
  "SSLCommerz": {
    "StoreId": "REAL_STORE_ID",
    "StorePassword": "REAL_STORE_PASSWORD",
    "IsSandbox": false
  },
  "AllowedHosts": "your-domain.com"
}
```

### Using environment variables instead (recommended for CI/CD)

ASP.NET Core reads environment variables automatically.  
Double underscores (`__`) replace JSON nesting in variable names:

```bash
# Linux / macOS / Docker
export ConnectionStrings__DefaultConnection="Server=..."
export Email__Password="smtp-secret"
export SSLCommerz__StorePassword="sslcommerz-secret"

# Windows PowerShell
$env:ConnectionStrings__DefaultConnection = "Server=..."
```

### Setting `ASPNETCORE_ENVIRONMENT`

| Value | Loads additional file |
|---|---|
| `Development` | `appsettings.Development.json` |
| `Production` | `appsettings.Production.json` |

```bash
# Linux / Docker
export ASPNETCORE_ENVIRONMENT=Production

# Windows PowerShell
$env:ASPNETCORE_ENVIRONMENT = "Production"
```

---

## Running the Application

### Development (hot-reload)

```bash
cd CattleFarm
dotnet watch run
```

Or press **F5** in Visual Studio.

### Build only

```bash
dotnet build CattleFarm.sln
```

### Run tests

```bash
dotnet test CattleFarm.Tests/CattleFarm.Tests.csproj
```

### Publish production bundle

```bash
dotnet publish CattleFarm/CattleFarm.csproj -c Release -o ./publish
```

---

## Production Deployment

### IIS (Windows Server)

1. Install the [.NET Hosting Bundle](https://dotnet.microsoft.com/download) on the server.
2. Publish the application: `dotnet publish -c Release -o C:\inetpub\CattleFarm`
3. Create an IIS Site pointing to the publish folder.
4. Set the Application Pool to **No Managed Code**.
5. Place `appsettings.Production.json` beside the published executable **or** use environment variables.
6. Run migrations on the production database before first start.

### Linux / Docker

A minimal `Dockerfile` approach:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY publish/ .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "CattleFarm.dll"]
```

Build and run:

```bash
dotnet publish CattleFarm/CattleFarm.csproj -c Release -o publish
docker build -t cattlefarm .
docker run -d -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=..." \
  -e Email__Password="..." \
  -e SSLCommerz__StorePassword="..." \
  cattlefarm
```

---

## Database Backup Advice

### SQL Server — automated backup (T-SQL job)

```sql
-- Run as a SQL Server Agent job or scheduled task
BACKUP DATABASE [CattleFarmDB]
TO DISK = N'D:\Backups\CattleFarmDB_' +
          CONVERT(VARCHAR, GETDATE(), 112) + '.bak'
WITH FORMAT, INIT, COMPRESSION, STATS = 10;
```

### PowerShell backup script (Windows)

```powershell
$date    = Get-Date -Format "yyyyMMdd_HHmm"
$backupPath = "D:\Backups\CattleFarmDB_$date.bak"

Invoke-Sqlcmd -ServerInstance ".\SQLEXPRESS" -Query @"
BACKUP DATABASE [CattleFarmDB]
TO DISK = N'$backupPath'
WITH FORMAT, COMPRESSION;
"@

Write-Host "Backup saved: $backupPath"
```

Schedule this with **Task Scheduler** to run nightly.

### Best practices

| Practice | Recommendation |
|---|---|
| Retention | Keep at least **7 daily** + **4 weekly** backups |
| Offsite copy | Sync backups to Azure Blob Storage or an S3 bucket |
| Test restores | Restore to a test server at least **once a month** |
| Transaction logs | Enable **Full Recovery Model** for point-in-time restore |
| Before migrations | Always take a manual backup before running `dotnet ef database update` in production |

### Restore from backup

```sql
-- Make sure no one is connected first
ALTER DATABASE [CattleFarmDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE [CattleFarmDB]
FROM DISK = N'D:\Backups\CattleFarmDB_20250101_0200.bak'
WITH REPLACE, RECOVERY;

ALTER DATABASE [CattleFarmDB] SET MULTI_USER;
```

---

## Troubleshooting

| Problem | Fix |
|---|---|
| `Login failed for user` | Check connection string credentials; ensure SQL Auth is enabled on the server |
| `Cannot open database` | Run `dotnet ef database update` to create/migrate the DB |
| Email alerts not sending | Verify `Email:Username` and `Email:Password` in appsettings; Gmail requires an App Password, not your account password |
| SSLCommerz payment fails | Confirm `IsSandbox` matches your credentials; sandbox creds don't work on production endpoint |
| `QuestPDF` license warning | Already initialized in `Program.cs` as Community license — safe to ignore if app is non-commercial |
| `ASPNETCORE_ENVIRONMENT` not set | Application defaults to `Production` in a published build; explicitly set the variable |
| Excel/PDF export errors | Ensure the `wwwroot/` folder has write permissions if temp files are needed |
