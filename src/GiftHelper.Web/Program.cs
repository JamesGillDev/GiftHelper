using GiftHelper.Data;
using GiftHelper.Data.Services;
using GiftHelper.Web.Components;
using GiftHelper.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var appDataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(appDataDirectory);

var configuredConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = string.IsNullOrWhiteSpace(configuredConnectionString)
    ? $"Data Source={Path.Combine(appDataDirectory, "gifthelper.db")}"
    : configuredConnectionString.Replace("|DataDirectory|", appDataDirectory);

builder.Services.AddDbContext<GiftHelperDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<RecipientService>();
builder.Services.AddScoped<OccasionService>();
builder.Services.AddScoped<GiftIdeaService>();
builder.Services.AddSingleton<LocalGiftSuggestionService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GiftHelperDbContext>();

    if (app.Environment.IsDevelopment())
    {
        await GiftHelperDbInitializer.InitializeAsync(dbContext);
    }
    else
    {
        await dbContext.Database.MigrateAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
