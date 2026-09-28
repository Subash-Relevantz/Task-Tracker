using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TrackerApi.Data;
using TrackerApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// CONTROLLERS
// ======================================================

builder.Services.AddControllers();


// ======================================================
// DATABASE
// ======================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is missing."
    );
}

builder.Services.AddDbContext<TrackerDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});


// ======================================================
// JWT CONFIGURATION
// ======================================================

var jwtKey =
    builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key is missing."
    );
}

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? "TrackerApi";

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? "TrackerFrontend";


// ======================================================
// JWT AUTHENTICATION
// ======================================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Local development is using HTTP.
        options.RequireHttpsMetadata = false;

        options.SaveToken = true;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,

                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                ClockSkew = TimeSpan.Zero
            };
    });


// ======================================================
// AUTHORISATION
// ======================================================

builder.Services.AddAuthorization();


// ======================================================
// SERVICES
// ======================================================

builder.Services.AddScoped<TokenService>();


// ======================================================
// CORS
//
// Your Vite frontend has been using 5173, 5174 and 5175.
// ======================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174",
                "http://127.0.0.1:5175"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ======================================================
// BUILD APP
// ======================================================

var app = builder.Build();


// ======================================================
// DATABASE MIGRATION + SEEDING
// ======================================================

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<TrackerDbContext>();

        Console.WriteLine(
            "Connecting to TrackerDb..."
        );

        await dbContext.Database.MigrateAsync();

        Console.WriteLine(
            "Database migration completed."
        );

        await DbSeeder.SeedAsync(dbContext);

        Console.WriteLine(
            "Database seeding completed."
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            "Database initialisation failed."
        );

        Console.WriteLine(ex.Message);

        throw;
    }
}


// ======================================================
// IMPORTANT
//
// Do not redirect HTTP → HTTPS during local development.
// Your API is running at:
//
// http://localhost:5022
// ======================================================

// app.UseHttpsRedirection();


// ======================================================
// CORS
//
// Must execute before Authentication and Authorization.
// ======================================================

app.UseCors("ReactApp");


// ======================================================
// AUTHENTICATION
// ======================================================

app.UseAuthentication();


// ======================================================
// AUTHORISATION
// ======================================================

app.UseAuthorization();


// ======================================================
// CONTROLLERS
// ======================================================

app.MapControllers();


// ======================================================
// ROOT API
// ======================================================

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        application = "TrackerApi",
        status = "Running",
        message = "Work Tracker API is running."
    });
});


// ======================================================
// STATUS API
// ======================================================

app.MapGet("/api/status", () =>
{
    return Results.Ok(new
    {
        application = "TrackerApi",
        status = "OK",
        serverTimeUtc = DateTime.UtcNow
    });
});


// ======================================================
// RUN
// ======================================================

app.Run();