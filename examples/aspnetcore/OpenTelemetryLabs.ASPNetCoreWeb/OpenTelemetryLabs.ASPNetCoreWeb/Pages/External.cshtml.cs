using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OpenTelemetryLabs.ASPNetCoreWeb.Pages
{
    public class ExternalModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public ExternalModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet(bool error = false)
        {
            var httpClient = new HttpClient();
            if (error)
            {
                try
                {
                    var html = await httpClient.GetStringAsync("https://httpstat.us/502/");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }
            }
            else
            {
                var html = await httpClient.GetStringAsync("https://example.com/");
                
            }
        }
    }
}
