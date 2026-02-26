# Gift Helper App (v1.2.0)

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Web_App-blue)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License](https://img.shields.io/badge/License-BLS%201.1-blue.svg)](./LICENSE.md)
[![Release](https://img.shields.io/badge/Release-v1.2.0-success)](https://github.com/JamesGillDev/GiftHelper/releases)

## Project Card
**Gift Helper App (In Progress / Planned)**
- Tags/Stack: C#/.NET, UX-first, Recommendation Engine, Local-first
- Description: "Guided gift-finder for people who do not know what to buy. The app asks a short series of questions (relationship, budget, interests, constraints) and generates personalized gift suggestions with reasoning and links."
- Repo: https://github.com/JamesGillDev/GiftHelper

## Release Versions
| Version | Date | Public Iteration |
| --- | --- | --- |
| `v1.0.0` | 2026-02-25 | Initial public release |
| `v1.0.1` | 2026-02-25 | README and release artifact iteration |
| `v1.1.0` | 2026-02-25 | Deployment iteration (Render + Docker + SQLite persistence) |
| `v1.2.0` | 2026-02-25 | Gift Finder iteration (local suggestion service and shortlist enhancements) |

Full change details are tracked in [CHANGELOG.md](./CHANGELOG.md).

## Pivot Summary
Gift Helper has been pivoted from a pure tracker into a **Gift Finder** app.

- Primary experience: guided gift discovery wizard + ranked suggestions
- Existing tracker functionality is preserved and repositioned as **Shortlist**
- Saving from Finder writes directly to the existing persisted `GiftIdeas` data

## MVP Flow
1. Start a new Gift Search in `Gift Finder`
2. Answer guided questions:
   - Relationship
   - Occasion
   - Budget min/max
   - Interests (multi-select + free text)
   - Constraints / deal-breakers
   - Style
   - Shipping timeline
   - Age range (optional)
   - Already has everything? (yes/no)
3. View ranked suggestions (top 20)
4. Save suggestions to `Shortlist`
5. Optionally mark saved items as purchased later

## Rule-Based Engine (Offline v1)
- Seed file: `src/GiftHelper.Web/Data/Seed/gift_ideas.json`
- Curated offline catalog (60 items)
- No external APIs
- Transparent scoring:
  - Relationship exact/partial: +30 / +15
  - Occasion exact/partial: +20 / +10
  - Interest matches: +8 each (cap +40)
  - Style match: +15
  - Budget fit: +25 in range, +10 if <=10% over, else -20
  - Constraint conflict: hard reject
  - Urgent shipping: penalize custom/slow-ship ideas

## Shortlist Persistence
Saved Finder suggestions are stored in existing `GiftIdea` records with added fields:
- `EstimatedMinPrice`
- `EstimatedMaxPrice`
- `Tags`
- `SeedId`

## Local Run
1. Restore dependencies:
   ```powershell
   dotnet restore GiftHelper.sln
   dotnet tool restore
   ```
2. Build:
   ```powershell
   dotnet build GiftHelper.sln
   ```
3. Apply migrations:
   ```powershell
   dotnet dotnet-ef database update --project src/GiftHelper.Data --startup-project src/GiftHelper.Web
   ```
4. Run:
   ```powershell
   dotnet run --project src/GiftHelper.Web
   ```

## GitHub Release Workflow
Use annotated tags for each iteration so version history is visible on GitHub:

```powershell
git tag -a v1.2.0 -m "Gift Helper App v1.2.0"
git push origin v1.2.0
```

For the next release iteration, increment SemVer and repeat (example: `v1.2.1`, `v1.3.0`).

## License
This project is licensed under the **Business Source License 1.1 (BLS)**.
See [LICENSE.md](./LICENSE.md) for full terms.
