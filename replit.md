# APS Web API Project

## Overview
.NET 8 ASP.NET Core Web API application (aps-web/WebAPI) with Swagger UI. The solution consists of two main repositories: aps-core (common libraries) and aps-web (web application).

## Architecture
- **Runtime**: .NET 8 SDK (8.0.416)
- **Framework**: ASP.NET Core Web API with Swagger/OpenAPI
- **Database**: SQL Server via Entity Framework Core 9.0
- **Cloud Services**: Azure (Storage, KeyVault, Identity, SignalR)
- **Port**: 5000 (HTTP)

## Project Structure
- `aps-web/WebAPI/` - Main Web API project
- `aps-web/ReportsWebApp.DB/` - Database layer (373 source files, EF Core)
- `aps-web/ReportsWebApp.Common/` - Shared utilities
- `aps-web/Common Projects/PT.Logging/` - Logging library
- `aps-core/CommonProjects/Common/` - Core common library
- `aps-core/CommonProjects/Common.Sql/` - SQL utilities

## Running the Application
- Workflow: "Start application" runs `bash aps-web/WebAPI/run.sh`
- Swagger UI available at `/swagger`
- API requires header `AzureAPIKey: Pl@n3t10geth3r` (ApiKeyMiddleware)

## Configuration
- `aps-web/WebAPI/appsettings.json` - Main config
- `aps-web/WebAPI/Program.cs` - Listens on http://0.0.0.0:5000, HTTPS redirect disabled in dev
- `aps-web/global.json` - SDK version pinned to 8.0.416
- `aps-web/NuGet.Config` - Package sources (nuget.org only, Azure DevOps feeds removed)

## Build Notes
- OmniSharp language server can consume excessive memory; run.sh kills it before building
- Build produces ~1240 warnings (nullability, deprecation) but 0 errors
- Initial build takes ~21 seconds; subsequent builds are faster
- `GeneratePackageOnBuild` disabled in ReportsWebApp.DB.csproj
