using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PatientPlanner.Models;

namespace PatientPlanner.Pages
{
    public class SettingsModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly PatientPlanner.Data.TimetableContext _context;
        private readonly IConfiguration _configuration;
        public const string SessionEndPoint = "_End";

        public SettingsModel(PatientPlanner.Data.TimetableContext context, ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public SettingsProfile settingsProfile { get; set; } = default!;
        public List<string> timesList = new();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
            {
                var endpoint = HttpContext.Session.GetString(SessionEndPoint);
                if (_context.Devices != null)
                {
                    Device device = _context.Devices.FirstOrDefault(d => d.PushP256DH == endpoint);

                    // Get the current settingsProfile if it exists
                    if (_context.SettingsProfiles != null)
                    {
                        settingsProfile = _context.SettingsProfiles.FirstOrDefault(s => s.DeviceID == device.ID);
                    }
                }
            }


        }
    }
}
