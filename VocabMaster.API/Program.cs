using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VocabMaster.Core.Interfaces.Repositories;
using VocabMaster.Core.Interfaces.Services;
using VocabMaster.Core.Interfaces.Services.Dictionary;
using VocabMaster.Core.Interfaces.Services.Vocabulary;
using VocabMaster.Data;
using VocabMaster.Data.Repositories;
using VocabMaster.Data.Seed;
using VocabMaster.Services;
using VocabMaster.Services.Authentication;
using VocabMaster.Services.Dictionary;
using VocabMaster.Services.Vocabulary;
using VocabMaster.Core.DTOs;
using VocabMaster.Core.Interfaces.Services.Quiz;
using VocabMaster.Services.Quiz;

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
builder.Services.AddScoped<ILearnedWordRepo, LearnedWordRepo>();
builder.Services.AddScoped<IDictionaryDetailsRepo, DictionaryDetailsRepo>();
builder.Services.AddScoped<IQuizQuestionRepo, QuizQuestionRepo>();
builder.Services.AddScoped<ICompletedQuizRepo, CompletedQuizRepo>();


// services for authentication
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IExternalAuthService, ExternalAuthService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

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

// Add HttpClient services
builder.Services.AddHttpClient("DictionaryApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("DictionaryApiUrl") ??
                                "https://api.dictionaryapi.dev/api/v2/entries/en/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add HttpClient for Google API
builder.Services.AddHttpClient("GoogleApi", client =>
{
    client.BaseAddress = new Uri("https://www.googleapis.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add memory caching
builder.Services.AddMemoryCache();

// services for dictionary
builder.Services.AddScoped<IDictionaryCacheService, DictionaryCacheService>();
builder.Services.AddScoped<IDictionaryLookupService, DictionaryLookupService>();
builder.Services.AddScoped<IRandomWordService, RandomWordService>();


// services for vocabulary
builder.Services.AddScoped<IWordStatusService, WordStatusService>();
builder.Services.AddScoped<ILearnedWordService, LearnedWordService>();

// services for quiz
builder.Services.AddScoped<IQuizQuestionService, QuizQuestionService>();
builder.Services.AddScoped<IQuizAnswerService, QuizAnswerService>();
builder.Services.AddScoped<IQuizProgressService, QuizProgressService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

// Use CORS before Authentication
app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        // Seed data from SeedData
        var logger = services.GetRequiredService<ILogger<Program>>();
        await SeedData.Initialize(services, logger);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
