using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OpenTelemetryLabs.ASPNetCoreWeb.Pages
{
    public class ExceptionModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public ExceptionModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }


        public void OnGet()
        {
            var activity = Activity.Current;
            try
            {
                var reg = new Regex("[a");
            }
            catch (Exception e)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "error happened.");
                _logger.LogError(e, "error happened");
                throw;
            }
        }
    }
}
