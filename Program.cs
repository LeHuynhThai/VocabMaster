using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VocabMaster.Data;
using VocabMaster.Models;
using VocabMaster.Repositories;
using VocabMaster.Services.Implementations;
using VocabMaster.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using VocabMaster.Repositories.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

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


// Đăng ký Repository và Service
builder.Services.AddScoped<ILearnedVocabularyRepository, LearnedVocabularyRepository>();
builder.Services.AddScoped<IVocabularyService, VocabularyService>();

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

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
