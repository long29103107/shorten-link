# Testing Standard

## Backend Verification

Run backend build after API, C#, config, package, or persistence changes:

```powershell
dotnet build
```

Run tests after core logic, validation, service, repository, or endpoint changes:

```powershell
dotnet test
```

For package-boundary tasks, verify packability:

```powershell
dotnet pack
```

For EF Core or SQLite changes, verify schema behavior with the smallest relevant command for the touched project.

## Frontend Verification

Run frontend build after React, routing, Tailwind, Vite, or TypeScript changes:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

If dependencies are not installed, report that frontend build was skipped and why.

## Smoke Tests

When dev servers are running:

- Create link: `POST /api/short-links`
- Redirect: `GET /{code}`
- Detail: `GET /api/short-links/{code}`
- Delete/deactivate: `DELETE /api/short-links/{code}`
- Unknown code follows configured fallback behavior.

## Reporting

Final responses must say which checks passed, which checks were skipped, and why.

