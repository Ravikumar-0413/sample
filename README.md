# Library Management API (Local JSON Storage)

This repository contains a small ASP.NET Core 8.0 Web API for managing a library catalog, borrowers, borrow/return transactions and a simple product catalog. It uses local JSON files as storage (no database required).

## Contents

- `LibraryManagementAPI/` — main ASP.NET Core project
  - `Controllers/` — API controllers (Books, Borrowers, Borrows, Products, ExternalApi)
  - `Services/` — business logic and external API integration
  - `Data/` — `JsonStorageService` (reads/writes JSON files in `Data/Storage`)
  - `Middleware/` — global error handling
  - `logs/` — Serilog text logs (daily rolling)

## Quickstart (PowerShell)

Open PowerShell and run from the repo root:

```powershell
cd "C:\Users\pvlra\OneDrive\Attachments\Desktop\lib\LibraryManagementAPI"
# Set the environment to Development so HTTPS redirection is skipped and Swagger is enabled
$env:ASPNETCORE_ENVIRONMENT = "Development"
# Build
dotnet build
# Run
dotnet run --project "C:\Users\pvlra\OneDrive\Attachments\Desktop\lib\LibraryManagementAPI\LibraryManagementAPI.csproj"
```

When the app is running you can browse the Swagger UI at:

```
http://localhost:5000/
```

Health endpoint:

```
http://localhost:5000/health
```

## Notes & Diagnostics

- The app uses Serilog for logging to `LibraryManagementAPI/logs/`.
- Local JSON files are stored in `LibraryManagementAPI/Data/Storage/` (ignored by git).
- A small `HeartbeatService` logs periodic ticks to help diagnose unexpected shutdowns; remove it when not needed.

## Common Commands

- Run compiled DLL detached (keeps app running if terminal is closed):

```powershell
Start-Process dotnet -ArgumentList '"C:\Users\pvlra\OneDrive\Attachments\Desktop\lib\LibraryManagementAPI\bin\Debug\net8.0\LibraryManagementAPI.dll"' -PassThru
```

- Tail logs:

```powershell
Get-Content -Path "C:\Users\pvlra\OneDrive\Attachments\Desktop\lib\LibraryManagementAPI\logs\app-*.txt" -Wait
```

## Git

The repository is on GitHub: `https://github.com/Ravikumar-0413/sample.git`.

## Next steps

- Run the Postman collection (included at `LibraryManagementAPI/Postman/LibraryManagementAPI.postman_collection.json`) to validate endpoints.
- Remove or adjust verbose diagnostics (HeartbeatService, request logging) before merging into production.

If you want, I can:
- Run the Postman tests and report results,
- Move logs/data out of OneDrive to avoid sync issues,
- Create a feature branch and open a PR with these diagnostics changes.
