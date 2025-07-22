using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Data;
using VocabMaster.Data.Repositories;
using VocabMaster.Services;
using System.IO;

// Set the content root path and web root path
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
    Args = args
});

// Add configuration
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("API/appsettings.json", optional: false)
    .AddJsonFile($"API/appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add HttpContext accessor
builder.Services.AddHttpContextAccessor();

// Add Session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Use connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContext")));

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.Cookie.Name = "VocabMaster.Auth";
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true; // reset the expiration time on each request
    });

// Register repositories and services
builder.Services.AddScoped<IUserRepository, UserRepo>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ILearnedVocabularyRepository, LearnedWordRepo>();
builder.Services.AddScoped<IVocabularyRepository, VocabRepo>();
builder.Services.AddScoped<IVocabularyService, VocabularyService>();
builder.Services.AddScoped<IDictionaryService, DictionaryService>();

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

// Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add Session middleware
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
