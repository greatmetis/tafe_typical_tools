using TypicalTechTools.DataAccess;
using TypicalTechTools.Models.Data;
using Microsoft.EntityFrameworkCore;
using TypicalTechTools.Models.Repositories;

// use Create Builder to setup configuration sources, including appsetting.json, user secrets environment variables
var builder = WebApplication.CreateBuilder(args);
//Grab the default connections string from the config in the appsettings file that was retrieved from secret.json
var connectionString = builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'Default' was not found.");
}

// Register  DbContext with dependency injection to the builder by supplying DbContextOptions
builder.Services.AddDbContext<TypicalTechToolDbContext>(options => options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(c =>
{
    c.IdleTimeout = TimeSpan.FromSeconds(120);
    c.Cookie.HttpOnly = true;
    c.Cookie.IsEssential = true;
});

builder.Services.AddDistributedMemoryCache();

// Add repository to DI by requesting interfaces and repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");

app.Run();
