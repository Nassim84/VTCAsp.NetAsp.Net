using MonBackendVTC.Services;
using System.Net.Http;
using System.Timers;

var builder = WebApplication.CreateBuilder(args);

// Autoriser CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<EmailService>();

var app = builder.Build();

// Middleware
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Self-ping automatique pour garder le serveur éveillé
var httpClient = new HttpClient();
var pingTimer = new System.Timers.Timer(5 * 60 * 1000); // toutes les 5 minutes
pingTimer.Elapsed += async (sender, e) =>
{
    try
    {
        // URL selon l'environnement
        var pingUrl = builder.Environment.IsDevelopment()
            ? "http://localhost:5044/api/health"
            : "https://uber-iiia.onrender.com/api/health";

        var response = await httpClient.GetAsync(pingUrl);
        if (response.IsSuccessStatusCode)
            Console.WriteLine("[Ping] Serveur éveillé " + DateTime.Now);
        else
            Console.WriteLine("[Ping] Erreur serveur " + DateTime.Now);
    }
    catch (Exception ex)
    {
        Console.WriteLine("[Ping] Impossible de ping le serveur : " + ex.Message);
    }
};
pingTimer.Start();

// Run
app.Run();
