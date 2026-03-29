using Microsoft.EntityFrameworkCore;
using RealEstateSync.Core.Interfaces;
using RealEstateSync.Core.Services;
using RealEstateSync.Infra.Data;
using RealEstateSync.Infra.Repositories;
using RealEstateSync.Providers.Providers;
using RealEstateSync.Providers.Sources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// DbContext (SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Core
builder.Services.AddScoped<ICsvReader, CsvReaderService>();
builder.Services.AddScoped<ISearchOrchestrator, SearchOrchestrator>();

// Infra repositories
builder.Services.AddScoped<ISearchHistoryRepository, SearchHistoryRepository>();
// (SearchConfigRepository vai depois, se quiser)

// Providers
builder.Services.AddScoped<IHtmlSource, LocalFileHtmlSource>();
builder.Services.AddScoped<ISearchProvider, OlxHttpProvider>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Rota padrão apontando para Search/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Search}/{action=Index}/{id?}");

app.Run();