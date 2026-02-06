using MonBackendVTC.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ===== CORS (s�curis� pour production) =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "https://ndrive.fr",
                "https://www.ndrive.fr" // Ajouter www si n�cessaire
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Si vous utilisez des cookies/auth
    });
});

// ===== Rate Limiting (anti-spam) =====
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("devis", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 3; // Max 3 requ�tes par minute
        opt.QueueLimit = 0;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync(
            "Trop de requ�tes. Veuillez r�essayer dans quelques instants.",
            token
        );
    };
});

// ===== Services =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// EmailService avec injection de d�pendances
builder.Services.AddSingleton<EmailService>(sp =>
    new EmailService(
        sp.GetRequiredService<IHttpClientFactory>(),
        sp.GetRequiredService<ILogger<EmailService>>()
    )
);

// HttpClient pour le self-ping (gestion propre des ressources)
builder.Services.AddHttpClient("SelfPing", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Service de fond pour le self-ping
builder.Services.AddHostedService<SelfPingService>();

var app = builder.Build();

// ===== Middleware =====
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// Rate limiting middleware
app.UseRateLimiter();

app.UseAuthorization();
app.MapControllers();

// ===== Run =====
app.Run();