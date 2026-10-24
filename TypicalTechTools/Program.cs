using Microsoft.AspNetCore.Authentication.Cookies;
using TypicalTechTools.DataAccess;
using TypicalTechTools.Models.Data;
using Microsoft.EntityFrameworkCore;
using TypicalTechTools.Models;
using TypicalTechTools.Models.Repositories;

// use Create Builder to setup configuration sources, including appsetting.json, or user secrets environment variables
var builder = WebApplication.CreateBuilder(args);
//Grab the default connections string from the config in the appsettings file that could be retrieved from secret.json
var connectionString = builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'Default' was not found.");
}

// Register  DbContext with dependency injection to the builder by supplying DbContextOptions
builder.Services.AddDbContext<TypicalTechToolDbContext>(options => options.UseSqlServer(connectionString));


// Add MVC Authentication session using cookie for tracking the login data of the users.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Set the path to redirect
        options.LoginPath = "/Authentication/Login";
        options.AccessDeniedPath = "/Authentication/AccessDenied";
        // Sets the length of time the user's login set for
        options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
        // Allow the login to be refreshed if it is still being used.
        options.SlidingExpiration = true;
    });

//Adds an authorization section to our system so we can setup policy rules etc.
builder.Services.AddAuthorization(options => 
{
    //Creates and names a policy that can be applied in our [Authorize] tags
    options.AddPolicy("ManagementAccess", policy => 
    {
        //Set the rules the user needs to pass to meet the policy
        policy.RequireRole("ADMIN", "MANAGER", "SUPERVISOR");
    });
    options.AddPolicy("GuestAccess", policy => 
    {
        //Set the rules the user needs to pass to meet the policy
        policy.RequireRole("GUEST");
        // policy.RequireClaim("DEPARTMENT", "ACCOUNTS"); <==Example of looking for a certain claim type
    });
});

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
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();

var app = builder.Build();

// Allow us to use and run any of our Scoped services
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TypicalTechToolDbContext>();
    var admin = context.AppUsers.FirstOrDefault(user=>user.UserName == "Admin");
    if (admin == null)
    {
        var newUser = new AppUser
        {
            UserName = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword("admin_password"),
            Role = "ADMIN"
        };
        context.AppUsers.Add(newUser);
        context.SaveChanges();
    }
}


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
app.UseAuthentication(); // this must go above authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");

app.Run();
