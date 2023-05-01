using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PatientPlanner.Models;

namespace PatientPlanner.Pages
{
    public class PatientViewModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly PatientPlanner.Data.TimetableContext _context;
        private readonly IConfiguration _configuration;
        public const string SessionEndPoint = "_End";
        public const string SessionCurrentPatient = "_CurrPatient";
        public Patient CurrentPatient = null;

        public PatientViewModel(PatientPlanner.Data.TimetableContext context, ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public IList<Patient> Patients { get; set; } = new List<Patient>();
        public SettingsProfile settingsProfile { get; set; } = default!;
        public List<TimeSpan> times { get; set; } = new List<TimeSpan>();
        public List<string> displayTimesList { get; set; } = new();
        public List<int> intervalList { get; set; } = new();
        public List<PatientTask> taskList { get; set; } = new List<PatientTask>();
        public int PatientID = 0;

        public async Task OnGetAsync()
        {
            // Check for a session state, then get the device. Otherwise have automatic reminder for the user to refresh the page
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
            {
                var endpoint = HttpContext.Session.GetString(SessionEndPoint);
                if (_context.Devices != null)
                {
                    Device device = _context.Devices.FirstOrDefault(device => device.PushEndpoint == endpoint);

                    if (_context.Patients != null)
                    {
                        Patients = await _context.Patients.Where(p => p.DeviceID == device.ID).ToListAsync();
                    }

                    if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionCurrentPatient)))
                    {
                        CurrentPatient = _context.Patients.FirstOrDefault(p => p.RoomNumber == HttpContext.Session.GetString(SessionCurrentPatient));
                    }
                    else
                    {
                        CurrentPatient = Patients[0];
                    }

                    PatientID = CurrentPatient.PatientID;

                    taskList.AddRange(await _context.PatientTasks.Where(t => t.PatientID == CurrentPatient.PatientID).ToListAsync());

                    //Get the settings profile
                    settingsProfile = _context.SettingsProfiles.FirstOrDefault(s => s.DeviceID == device.ID);

                    //Calculate the time period

                    times = new List<TimeSpan>();

                    if (settingsProfile.StartTime > settingsProfile.EndTime)
                    {
                        var temp = settingsProfile.StartTime;
                        var tempTime = new TimeSpan(24, 0, 0);
                        while (temp < tempTime)
                        {
                            times.Add(temp);
                            temp = temp.Add(new TimeSpan(0, settingsProfile.Interval, 0));
                        }

                        temp = new TimeSpan(0, 0, 0);
                        while (temp < settingsProfile.EndTime.Add(new TimeSpan(0, settingsProfile.Interval, 0)))
                        {
                            times.Add(temp);
                            temp = temp.Add(new TimeSpan(0, settingsProfile.Interval, 0));
                        }
                    }
                    else
                    {
                        var i = settingsProfile.StartTime;
                        while (i < settingsProfile.EndTime.Add(new TimeSpan(0, settingsProfile.Interval, 0)))
                        {
                            times.Add(i);
                            i = i.Add(new TimeSpan(0, settingsProfile.Interval, 0));
                        }
                    }

                    foreach (var time in times)
                    {
                        displayTimesList.Add(time.ToString(@"hh\:mm"));
                    }

                    int interval = settingsProfile.Interval;
                    int j = 0;
                    // Create a list of intervals
                    while (j < 240)
                    {
                        j += interval;
                        intervalList.Add(j);
                    }
                }
            }
        }

        public IActionResult OnPostChangeCurrentPatient(string curr)
        {
            // Set the current patient in the settings profile
            HttpContext.Session.SetString(SessionCurrentPatient, curr);
            return RedirectToAction("Get");
        }

        public IActionResult OnPostAddTask(int patientID, string taskName, string startTime, string endTime, string interval)
        {

            PatientTask startPT = new PatientTask(patientID, taskName, startTime, "grey");
            _context.Add()
            return RedirectToAction("Get");
        }
    }
}
