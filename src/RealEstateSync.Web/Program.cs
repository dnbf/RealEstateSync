using Microsoft.EntityFrameworkCore;
using RealEstateSync.Core.Interfaces;
using RealEstateSync.Core.Services;
using RealEstateSync.Infra.Data;
using RealEstateSync.Infra.Repositories;
using RealEstateSync.Providers;
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

// OLX provider
builder.Services.AddScoped<ISearchProvider>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();

    // ContentRootPath = ...\RealEstateSync.Web
    // Queremos ...\RealEstateSync.Providers\Samples\olx_sample.html
    var providersRoot = Path.Combine(env.ContentRootPath, "..", "RealEstateSync.Providers");
    var samplesPath = Path.Combine(providersRoot, "Samples");
    var olxPath = Path.Combine(samplesPath, "olx_sample.html");

    var htmlSource = new LocalFileHtmlSource(olxPath);
    return new OlxHttpProvider(htmlSource);
});

// ZAP provider
builder.Services.AddScoped<ISearchProvider>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();

    var providersRoot = Path.Combine(env.ContentRootPath, "..", "RealEstateSync.Providers");
    var samplesPath = Path.Combine(providersRoot, "Samples");
    var zapPath = Path.Combine(samplesPath, "zap_sample.html");

    var htmlSource = new ZapFileHtmlSource(zapPath);
    return new ZapMockProvider(htmlSource);
});

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