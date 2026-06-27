# Production Readiness Guide

## Completed in this pass

- Vulnerable package references were updated.
- Test credentials were removed from `CattleFarm/gmail.md`.
- Cookie and antiforgery settings now require secure cookies outside development.
- Email configuration now fails fast when required SMTP settings are missing or still set to placeholder values.
- `.gitignore` covers local build output, logs, IDE state, archives, temp files, and local secret files.
- Reports now include a basic Excel export for summary, monthly trend, revenue breakdown, and expense breakdown.
- Milk production and payroll now include Excel exports.
- Existing vaccination/subscription alert generation is now wired to a background service.
- Alert coverage was expanded for sick cattle follow-up, pending appointments, unpaid salaries, and pending/failed order payments.
- A starter xUnit test project was added with coverage for identity claim generation and user initials.

## Prompt checklist status

| Area | Status | Notes |
| --- | --- | --- |
| Security/dependency update | Done | `dotnet list package --vulnerable --include-transitive` reports no vulnerable packages. |
| Sensitive data cleanup | Done | `gmail.md` no longer stores test passwords; real credentials still need private storage. |
| Production configuration | Partial | Production example file added; real environment values must be configured by the deployer. |
| Reports/export readiness | Partial | Excel export added for main reports, milk production, and payroll; PDF and remaining module exports remain future work. |
| Notifications/alerts | Partial | Vaccination overdue, subscription expiry, sick cattle follow-up, pending appointment, unpaid salary, and order/payment alerts are scheduled; low feed stock needs inventory fields before it can be implemented safely. |
| Testing | Partial | Starter test project added; full integration/flow tests remain future work. |
| UI/mobile polish | Not implemented | Needs visual pass in browser/mobile viewports. |
| Documentation | Done/ongoing | This guide and production config template were added. |

## Required manual setup

Use environment variables, deployment secrets, or ASP.NET user secrets for real values:

```powershell
dotnet user-secrets init --project CattleFarm/CattleFarm.csproj
dotnet user-secrets set "Email:Username" "your-email@example.com" --project CattleFarm/CattleFarm.csproj
dotnet user-secrets set "Email:Password" "your-email-app-password" --project CattleFarm/CattleFarm.csproj
dotnet user-secrets set "Email:FromAddress" "no-reply@your-domain.com" --project CattleFarm/CattleFarm.csproj
dotnet user-secrets set "SSLCommerz:StoreId" "your-store-id" --project CattleFarm/CattleFarm.csproj
dotnet user-secrets set "SSLCommerz:StorePassword" "your-store-password" --project CattleFarm/CattleFarm.csproj
```

For production, use `CattleFarm/appsettings.Production.example.json` as a template and keep real secrets in the hosting platform, not in Git.

## Recommended next improvements

- Add PDF exports and remaining module exports for cattle profiles, payroll slips, order invoices, and doctor prescriptions.
- Add feed inventory and threshold fields, then generate low feed stock alerts.
- Expand tests to cover login, registration, farm access, cattle creation, appointment creation, order creation, and payroll authorization.
- Review mobile layouts for landing, login/register, dashboards, cattle forms, worker tasks, doctor appointments, and reports.
- Replace any remaining sample marketing numbers with live database-backed metrics.
