# Gift Helper App (v1)

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Web_App-blue)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License](https://img.shields.io/badge/License-BLS%201.1-blue.svg)](./LICENSE.md)

Gift Helper is a local-first web app for tracking gift ideas by recipient and occasion, with budgets, priorities, and statuses.

## What It Includes
- Dashboard with upcoming occasions, gift status summary, and quick-add gift form
- Recipients page with full CRUD and name search
- Recipient details page with recipient profile, upcoming occasions, and recipient-scoped gift filters
- Occasions page with full CRUD and recurring yearly days-until projection
- Gifts page with full CRUD and global filters (recipient, status, priority, search)

## Tech Stack
- .NET 8 (LTS)
- Blazor Web App (Interactive Server render mode)
- EF Core 8 + SQLite
- DataAnnotations validation

## Architecture
- `src/GiftHelper.Domain`: entities and enums
- `src/GiftHelper.Data`: `GiftHelperDbContext`, migrations, seed initializer, service layer
- `src/GiftHelper.Web`: UI pages/components, DI, startup migration logic

## Run Locally
1. Restore and build:
   ```powershell
   dotnet restore GiftHelper.sln
   dotnet build GiftHelper.sln
   ```
2. Restore local tools:
   ```powershell
   dotnet tool restore
   ```
3. Apply migration:
   ```powershell
   dotnet dotnet-ef database update --project src/GiftHelper.Data --startup-project src/GiftHelper.Web
   ```
4. Run:
   ```powershell
   dotnet run --project src/GiftHelper.Web
   ```

## Deploy Without Azure (Render)
This repo includes a Docker deployment path for Render (`Dockerfile` + `render.yaml`).

1. Push this repo to GitHub.
2. In Render, choose **Blueprint** deployment and select this repo.
3. Render will read `render.yaml` and create:
   - one web service (`gifthelper`)
   - one persistent disk mounted at `/var/data` for SQLite
4. Deploy. The app auto-runs EF migrations on startup.

### Important Render notes
- The provided `render.yaml` uses `plan: starter` so the SQLite database can persist on disk.
- Free plans do not support persistent disks, so data can be lost on restarts/redeploys.
- Database location in production is `Data Source=/var/data/gifthelper.db`.

## Public Project Links
- Repository: https://github.com/JamesGillDev/GiftHelper
- Latest Release: https://github.com/JamesGillDev/GiftHelper/releases/tag/v1.0.0
- Release Build (ZIP): https://github.com/JamesGillDev/GiftHelper/releases/download/v1.0.0/GiftHelper.Web.v1.0.0.zip

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
