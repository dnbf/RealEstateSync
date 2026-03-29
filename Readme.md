# RealEstateSync

RealEstateSync is a small ASP.NET Core MVC application that simulates the process of checking if real estate listings from an internal CRM exist on external portals (e.g. OLX).  
It was designed as a portfolio project to demonstrate layered architecture, CSV import, background orchestration, basic scraping/parsing and persistence with EF Core + SQLite.

---

## Features

- Upload a **CSV file** with real estate items (Code, Address, City, etc.)
- Orchestrated search against **external providers** (currently a simulated OLX provider)
- **Fake / sample-based scraping** using an HTML snapshot and `HtmlAgilityPack`
- **Search results screen** with:
  - Per-item status (`Found`, `NotFound`, `Error`)
  - Detailed messages from the provider
  - Summary panel (Total / Found / NotFound / Errors)
- **Search history** stored in SQLite with:
  - Historic entries per upload
  - Dashboard with aggregated statistics (last 30 days)
- Clean **layered architecture**:
  - `Core` (domain + interfaces + services)
  - `Web` (ASP.NET Core MVC, views, controllers)
  - `Providers` (external search providers)
  - `Infra` (EF Core + SQLite, repositories)

---

## Architecture Overview

The solution is split into four main projects:

- **RealEstateSync.Core**
  - Domain models: `RealEstateItem`, `SearchResult`, `SearchHistoryEntry`, `SearchConfig`, `RealEstateStatus`
  - Service interfaces:
    - `ICsvReader` – reads real estate items from a CSV stream
    - `ISearchProvider` – abstracts an external portal (e.g. OLX)
    - `ISearchOrchestrator` – orchestrates calls to multiple providers
    - `ISearchHistoryRepository` – persistence for history
  - Services:
    - `SearchOrchestrator` – loops through items and providers, returning a list of `SearchResult`

- **RealEstateSync.Web**
  - ASP.NET Core MVC application
  - Controllers:
    - `SearchController`
      - `GET /Search/Index` → upload form
      - `POST /Search/Index` → parses CSV, calls orchestrator, saves history, shows results
    - `HistoryController`
      - `GET /History/Index` → history dashboard + table
  - Views:
    - `Views/Search/Index` – CSV upload screen
    - `Views/Search/Results` – results table + summary cards
    - `Views/History/Index` – history table + summary cards
  - Uses EF Core + SQLite via `AppDbContext` (from Infra project)

- **RealEstateSync.Providers**
  - External provider implementations
  - `OlxHttpProvider`:
    - Builds a search URL for OLX
    - Uses an abstraction `IHtmlSource` to obtain HTML (demo mode reads from a local sample file)
    - Uses `HtmlAgilityPack` to parse the sample HTML and decide whether there are listings
  - HTML sources:
    - `LocalFileHtmlSource` – reads an `olx_sample.html` file (for demo mode)

- **RealEstateSync.Infra**
  - Data access with EF Core
  - `AppDbContext` (SQLite):
    - `DbSet<SearchHistoryEntry>`
    - `DbSet<SearchConfig>`
  - Repositories:
    - `SearchHistoryRepository` – save and query history + aggregated stats

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
   - `SearchOrchestrator` loops through each `RealEstateItem` and each `ISearchProvider`.
   - Currently, there is one provider: `OlxHttpProvider`.

3. **Provider Simulation (OLX)**
   - `OlxHttpProvider` builds a search URL like:
     `https://www.olx.com.br/imoveis/venda/estado-sp?q={address+city}`
   - Instead of hitting OLX directly (which is protected by Cloudflare), it uses `IHtmlSource`:
     - In demo mode, `LocalFileHtmlSource` reads a local HTML snapshot (`olx_sample.html`).
   - The provider uses `HtmlAgilityPack` to check if there are any listing-like nodes in the HTML.
   - It returns a `SearchResult` with:
     - `Status` (`Found`, `NotFound`, or `Error`)
     - `Details` and `ErrorMessage` when applicable.

4. **Persist History**
   - Once results are obtained, the controller:
     - Counts `Found` / `NotFound` / `Error` entries
     - Creates a `SearchHistoryEntry` with aggregates and file name
     - Saves it using `ISearchHistoryRepository` → `SearchHistoryRepository` → `AppDbContext` (SQLite)

5. **Display Results**
   - The `SearchController` passes a `SearchResultsViewModel` to `Views/Search/Results`:
     - Cards with Total / Found / NotFound / Errors for this execution
     - Table with each `SearchResult` row and colored status badges

6. **History Dashboard**
   - `/History/Index` uses `ISearchHistoryRepository` to:
     - Fetch the last N history entries
     - Compute aggregated statistics for the last 30 days (Total / Found / NotFound / Errors)
   - The view shows:
     - Summary cards with these aggregates
     - A table with each upload (file name, totals, notes, date)

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
   - The default route is `Search/Index`

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

Upload it on the **Search** page to see the results and populate the history.

---

## Notes About Scraping / Providers

- The current OLX provider uses an **HTML snapshot** (`olx_sample.html`) for demo purposes.
- Direct scraping from OLX is protected by Cloudflare and requires extra tooling (e.g. headless browser, external scraping APIs).
- The architecture is ready to:
  - Swap `IHtmlSource` for a real HTTP implementation,
  - Or plug new providers (e.g. additional real estate portals).

---

## Possible Future Enhancements

- Implement additional providers (other portals) and aggregate results per portal.
- Add filtering and search on the History page.
- Add authentication (only logged users can upload/search).
- Use a more robust database (e.g. SQL Server or PostgreSQL) behind EF Core.
- Integrate with external automation/orchestration tools (like n8n) for more complex workflows.

---

## About This Project

This project was created as a **learning and portfolio** exercise to showcase:

- ASP.NET Core MVC
- Clean separation of concerns and layered architecture
- EF Core + SQLite for persistence
- HTML parsing with HtmlAgilityPack
- Basic UI with Razor + Bootstrap