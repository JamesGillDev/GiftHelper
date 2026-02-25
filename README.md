# Gift Helper App (v1)

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Web_App-blue)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License](https://img.shields.io/badge/License-BLS%201.1-blue.svg)](./LICENSE.md)

Gift Helper is a local-first web app for tracking gift ideas by recipient and occasion, with budgets, priorities, and statuses.

## Live Deployment
- Azure App Service: deployment attempt blocked because the current Azure subscription is read-only/disabled for write operations.

## What It Includes
- Dashboard with:
  - upcoming occasions (default next 60 days)
  - gift status summary
  - quick-add gift idea form
- Recipients page with full CRUD and name search
- Recipient details page with:
  - recipient profile
  - upcoming occasions
  - recipient-scoped gift list with status/search filters
  - quick add gift idea
- Occasions page with full CRUD and recurring date projection / days-until display
- Gifts page with full CRUD and global filters:
  - recipient
  - status
  - priority
  - search (title/description/store/category)

## Tech Stack
- .NET 8 (LTS)
- Blazor Web App (Interactive Server render mode)
- EF Core 8 + SQLite
- DataAnnotations validation

## Architecture
- `src/GiftHelper.Domain`
  - POCO entities and enums
- `src/GiftHelper.Data`
  - `GiftHelperDbContext`
  - migrations
  - seed initialization
  - service layer (`RecipientService`, `OccasionService`, `GiftIdeaService`)
- `src/GiftHelper.Web`
  - UI components/pages
  - DI/configuration/startup migration

## Run Locally
1. Restore and build:
   ```powershell
   dotnet restore GiftHelper.sln
   dotnet build GiftHelper.sln
   ```
2. Ensure local tools are restored:
   ```powershell
   dotnet tool restore
   ```
3. Apply database migration:
   ```powershell
   dotnet dotnet-ef database update --project src/GiftHelper.Data --startup-project src/GiftHelper.Web
   ```
4. Run the app:
   ```powershell
   dotnet run --project src/GiftHelper.Web
   ```

## Release
- Repository: https://github.com/JamesGillDev/GiftHelper
- Latest Release: https://github.com/JamesGillDev/GiftHelper/releases/tag/v1.0.0

## License
This project is licensed under the **Business Source License 1.1 (BLS)**.
See [LICENSE.md](./LICENSE.md) for full terms.

- Current use grant: copy, modify, and redistribute for non-production use
- Additional Use Grant: None
- Change Date: 2029-01-01
- Change License: Apache License 2.0

## Roadmap
- Authentication and multi-user isolation
- Tags and richer categorization
- Cloud sync and backup options
- Mobile companion app
- AI gift recommendations
