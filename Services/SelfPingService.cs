using System.Net.Http;

namespace MonBackendVTC.Services
{
    public class SelfPingService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<SelfPingService> _logger;

        public SelfPingService(
            IHttpClientFactory httpClientFactory,
            IHostEnvironment environment,
            ILogger<SelfPingService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _environment = environment;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Attendre 1 minute avant le premier ping
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("SelfPing");

                    var pingUrl = _environment.IsDevelopment()
                        ? "http://localhost:5044/api/health"
                        : "https://uber-iiia.onrender.com/api/health";

                    var response = await client.GetAsync(pingUrl, stoppingToken);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("[Ping] ✅ Serveur éveillé à {Time}", DateTime.Now);
                    }
                    else
                    {
                        _logger.LogWarning("[Ping] ⚠️ Réponse serveur : {StatusCode} à {Time}",
                            response.StatusCode, DateTime.Now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Ping] ❌ Erreur de ping à {Time}", DateTime.Now);
                }

                // Attendre 5 minutes avant le prochain ping
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}