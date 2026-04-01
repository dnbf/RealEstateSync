# RealEstateSync

RealEstateSync is a small ASP.NET Core MVC application that simulates the process of checking if real estate listings from an internal CRM exist on external portals (e.g. OLX, ZAP).  
It was designed as a portfolio project to demonstrate layered architecture, CSV import, multi‑provider orchestration, HTML parsing and a simple execution history using EF Core + SQLite.

---

## Features

- Upload a **CSV file** with real estate items (Code, Address, City, etc.)
- Orchestrated search against **multiple external providers**:
  - OLX (simulated via local HTML snapshot)
  - ZAP (simulated via local HTML snapshot)
- **Sample-based scraping** using HTML snapshots and `HtmlAgilityPack`
- **Search results screen** with:
  - Per-item status (`Found`, `NotFound`, `Error`)
  - Detailed messages from the providers
  - Summary panel (Total / Found / NotFound / Errors for that execution)
- **Search history** stored in SQLite with:
  - One entry per execution (per CSV upload)
  - Aggregated statistics for each execution (TotalItems / Found / NotFound / Errors)
  - Dashboard with global aggregates (sum of all executions)
- Clean **layered architecture**:
  - `Core` (domain + interfaces + services)
  - `Providers` (external search providers + HTML sources)
  - `Web` (ASP.NET Core MVC, views, controllers)
  - `Infra` (EF Core + SQLite, repositories)

---

## Architecture Overview

The solution is split into four main projects:

### RealEstateSync.Core

- Domain models:
  - `RealEstateItem` – a property coming from the internal CRM (parsed from CSV)
  - `SearchResult` – the result of a search on one provider for one item
  - `SearchHistoryEntry` – aggregated result of a full execution (one CSV upload)
  - `RealEstateStatus` – enum for `Found`, `NotFound`, `Error`
- Service interfaces:
  - `ICsvReader` – reads real estate items from a CSV stream
  - `ISearchProvider` – abstracts an external portal (e.g. OLX, ZAP)
  - `ISearchOrchestrator` – orchestrates calls to multiple providers
  - `ISearchHistoryRepository` – persistence for search executions
- Services:
  - `SearchOrchestrator` – loops through items and providers, returning a list of `SearchResult`

### RealEstateSync.Providers

- External provider implementations:
  - `OlxHttpProvider`
  - `ZapMockProvider`
- HTML sources:
  - `LocalFileHtmlSource` – reads `olx_sample.html` from the `Samples` folder
  - `ZapFileHtmlSource` – reads `zap_sample.html` from the `Samples` folder
- Both providers use:
  - `IHtmlSource` to obtain HTML (in demo mode, from local files)
  - `HtmlAgilityPack` to parse the HTML snapshot and extract:
    - listing title
    - price
    - URL
    - location
    - and decide whether the item was found or not

### RealEstateSync.Web

- ASP.NET Core MVC application
- Controllers:
  - `SearchController`
    - `GET /Search/Index` → upload form
    - `POST /Search/Index` → parses CSV, calls orchestrator, *saves history*, shows results
  - `HistoryController`
    - `GET /History/Index` → history dashboard + table
- Views:
  - `Views/Search/Index` – CSV upload screen
  - `Views/Search/Results` – results table + summary cards (for the current execution)
  - `Views/History/Index` – history table + summary cards (aggregated across executions)
- Layout:
  - Navbar with links to **Search** and **History**
- Uses EF Core + SQLite via `AppDbContext` (from Infra project)

### RealEstateSync.Infra

- Data access with EF Core
- `AppDbContext` (SQLite):
  - `DbSet<SearchHistoryEntry>`
  - `DbSet<SearchConfig>` (if present)
- Repositories:
  - `SearchHistoryRepository` – save and query history entries

---

## Technology Stack

- **Backend**
  - .NET 8 (ASP.NET Core MVC)
  - C#
  - Entity Framework Core
  - SQLite

- **Parsing / Providers**
  - HtmlAgilityPack (HTML parsing on the provider layer)

- **Frontend**
  - Razor Views
  - Bootstrap (from default ASP.NET Core MVC template)

---

## How It Works (End-to-End)

1. **Upload**
   - The user uploads a CSV file on `/Search/Index`.
   - `ICsvReader` reads and maps each row into `RealEstateItem`.

2. **Search Orchestration**
   - `SearchController` calls `ISearchOrchestrator.SearchAsync(items)`.
   - `SearchOrchestrator` loops through each `RealEstateItem` and each configured `ISearchProvider`.
   - Currently, there are two providers: `OlxHttpProvider` and `ZapMockProvider`.

3. **Provider Simulation (OLX and ZAP)**
   - Each provider builds an internal search key from the item (code / address / city).
   - Instead of hitting real websites, they use `IHtmlSource` implementations that read from local HTML samples:
     - `olx_sample.html`
     - `zap_sample.html`
   - The providers use `HtmlAgilityPack` to parse the HTML and extract:
     - listing title, price, location, direct URL
   - For each item + provider, a `SearchResult` is returned with:
     - `Status` (`Found`, `NotFound`, or `Error`)
     - `ListingTitle`, `ListingPrice`, `ListingUrl`, `ListingLocation`
     - `ErrorMessage` when applicable

4. **Persist History (per execution)**
   - Once all results are obtained, `SearchController`:
     - Counts `Found` / `NotFound` / `Error` entries for this execution
     - Creates a `SearchHistoryEntry` with:
       - `FileName` (CSV file name)
       - `SearchDate` (UTC)
       - `TotalItems`, `FoundCount`, `NotFoundCount`, `ErrorCount`
     - Saves it using `ISearchHistoryRepository` → `SearchHistoryRepository` → `AppDbContext` (SQLite)

5. **Display Results**
   - The `SearchController` passes a `SearchResultsViewModel` to `Views/Search/Results`:
     - Summary cards with Total / Found / NotFound / Errors for *this* execution
     - A table with each `SearchResult` row:
       - one line per (Item, Provider)
       - colored badges for status
       - “View” button linking to the simulated listing URL

6. **History Dashboard**
   - `/History/Index` uses `ISearchHistoryRepository` to:
     - Fetch the last N history entries (most recent executions)
     - Compute aggregates across those executions:
       - total sessions
       - sum of TotalItems / Found / NotFound / Errors
   - The view shows:
     - Summary cards with these global aggregates
     - A table with one row per execution (file name, totals, date, notes)

---

## Running the Project

### Prerequisites

- .NET 8 SDK
- (Optional) Visual Studio 2022 or VS Code

### Steps

1. **Clone the repository**

   ```bash
   git clone https://github.com/your-user/RealEstateSync.git
   cd RealEstateSync
   ```

2. **Restore packages**

   ```bash
   dotnet restore
   ```

3. **Apply migrations and create SQLite database**

   From the solution root:

   ```bash
   dotnet tool install --global dotnet-ef  # if not installed

   dotnet ef migrations add InitialCreate -p src/RealEstateSync.Infra -s src/RealEstateSync.Web
   dotnet ef database update -p src/RealEstateSync.Infra -s src/RealEstateSync.Web
   ```

4. **Run the Web project**

   ```bash
   dotnet run --project src/RealEstateSync.Web
   ```

5. **Access the app**

   - Go to `https://localhost:5001` or `http://localhost:5000` (depending on your configuration)
   - The default route will take you to the **Search** screen

---

## Sample CSV

You can start with a simple CSV like:

```csv
Code,Address,Neighborhood,City,State
001,Rua A,Bairro A,São Paulo,SP
002,Rua B,Bairro B,São Paulo,SP
003,Rua C,Bairro C,São Paulo,SP
004,Rua D,Bairro D,São Paulo,SP
```

Upload it on the **Search** page to see the multi-provider results and populate the history.

---

## Notes About Scraping / Providers

- The current providers (OLX and ZAP) use **HTML snapshots** (`olx_sample.html`, `zap_sample.html`) for demo purposes.
- Direct scraping from real portals often involves:
  - anti-bot protections (Cloudflare, captchas),
  - terms of use considerations,
  - and sometimes headless browsers or third‑party scraping APIs.
- The architecture is ready to:
  - Swap `IHtmlSource` for a real HTTP implementation,
  - Plug new providers (e.g. additional real estate portals),
  - Or move from mock HTML snapshots to real-time scraping when appropriate.

---

## Possible Future Enhancements

- Implement additional providers (more real estate portals) and aggregate results per portal.
- Add filters (date range, file name) and search on the History page.
- Group results by property code, showing providers as sub‑rows.
- Add authentication (only logged-in users can upload/search).
- Use a more robust database (e.g. SQL Server or PostgreSQL) behind EF Core.
- Integrate with external automation/orchestration tools (e.g. n8n) for scheduled runs.

---

## About This Project

This project was created as a **learning and portfolio** exercise to showcase:

- ASP.NET Core MVC
- Clean separation of concerns and layered architecture
- EF Core + SQLite for persistence
- HTML parsing with HtmlAgilityPack
- Aggregated execution history
- Basic UI with Razor + Bootstrap