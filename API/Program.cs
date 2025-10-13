using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Repository.SeedData;
using Service.Interfaces;
using Repository.Interfaces;
using Repository.Implementation;
using Repository;
using Service.Implementation;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContext")));

// Add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add Repositories
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IVocabularyRepo, VocabRepo>();


// services for authentication
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // change from None to Lax
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // change from Always to SameAsRequest
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWT");
var jwtSecret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };

    // Configure JWT Bearer Events
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers["Token-Expired"] = "true";
            }
            return Task.CompletedTask;
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization();


// Add HttpClient for Google API
builder.Services.AddHttpClient("GoogleApi", client =>
{
    client.BaseAddress = new Uri("https://www.googleapis.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add memory caching
builder.Services.AddMemoryCache();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Use CORS before Authentication
app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllers();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        await SeedUser.Initialize(services);
        await SeedVocabulary.Initialize(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

app.Run();
