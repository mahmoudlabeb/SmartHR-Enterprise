# SmartHR

ASP.NET Core (.NET 8) HR management system.

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB works for development)

## Configure

### Database

Connection string is in `SmartHR/appsettings.json` under `ConnectionStrings:DefaultConnection`.

### Seed SuperAdmin (Development/Staging)

By default, the app seeds roles and (optionally) a SuperAdmin user.

- Config key: `SeedAdmin:Enabled` (defaults to `true` in Development, otherwise `false`)
- Config keys: `SeedAdmin:Email`, `SeedAdmin:Password`

Recommended (User Secrets):

```bash
dotnet user-secrets init --project SmartHR/SmartHR.csproj
dotnet user-secrets set "SeedAdmin:Email" "superadmin@smarthr.com" --project SmartHR/SmartHR.csproj
dotnet user-secrets set "SeedAdmin:Password" "ChangeMe!123" --project SmartHR/SmartHR.csproj
```

If `SeedAdmin:Password` is not configured in Development, the app falls back to a development-only default and logs a warning.

### Email (SMTP)

Set `EmailSettings` in `SmartHR/appsettings.json` (or User Secrets / environment variables). If `EmailSettings:Host` is empty, the app will log email attempts instead of sending.

## Run

```bash
dotnet restore
dotnet dev-certs https --trust
dotnet ef database update --project SmartHR/SmartHR.csproj
dotnet run --project SmartHR/SmartHR.csproj
```

Default dev URL (launch profile): `https://localhost:7186`.
