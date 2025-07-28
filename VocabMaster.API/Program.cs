using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Data;
using VocabMaster.Data.Repositories;
using VocabMaster.Services;
using System;
using System.IO;
using VocabMaster.Core.Mapping;
using VocabMaster.Data.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactAppPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Add HttpContext accessor
builder.Services.AddHttpContextAccessor();

// Add Session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // change from None to Lax
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // change from Always to SameAsRequest
});

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/api/account/login";
        options.LogoutPath = "/api/account/logout";
        options.Cookie.Name = "VocabMaster.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax; // change from None to Lax
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // change from Always to SameAsRequest
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // increase expiration time
        options.SlidingExpiration = true; // reset the expiration time on each request
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

// Add HttpClient services
builder.Services.AddHttpClient("DictionaryApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("DictionaryApiUrl") ?? 
                                "https://api.dictionaryapi.dev/api/v2/entries/en/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add memory caching
builder.Services.AddMemoryCache();

// Use connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContext")));

// Register repositories
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IVocabularyRepo, VocabRepo>();
builder.Services.AddScoped<ILearnedWordRepo, LearnedWordRepo>();
builder.Services.AddScoped<IDictionaryDetailsRepo, DictionaryDetailsRepo>();

// Register services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IVocabularyService, VocabularyService>();
builder.Services.AddScoped<IDictionaryService, DictionaryService>();

// Add TranslationCrawlerService
builder.Services.AddScoped<ITranslationCrawlerService, TranslationCrawlerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Use CORS before authentication and authorization
app.UseCors("AllowAll");

// Add static files middleware
app.UseStaticFiles();

// Add routing middleware
app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add endpoints middleware
app.MapControllers();

// Apply migrations và seed data
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
