using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PatientPlanner.Models;
using System.Diagnostics;

namespace PatientPlanner.Pages;

public class IndexModel : PageModel
{

    private readonly ILogger<IndexModel> _logger;
    private readonly PatientPlanner.Data.TimetableContext _context;
    private readonly IConfiguration _configuration;
    public const string SessionEndPoint = "_End";

    public IndexModel(PatientPlanner.Data.TimetableContext context, ILogger<IndexModel> logger, IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _configuration = configuration;
    }

    public IList<Patient> Patients { get; set; } = default!;
    public List<PatientDisplayTask> taskList { get; set; } = new List<PatientDisplayTask>();
    public SettingsProfile settingsProfile { get; set; } = default!;
    public List<TimeSpan> times { get; set; } = new List<TimeSpan>();

    public async Task OnGetAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
        {
            Console.WriteLine("Empty");
        }
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
        {
            var endpoint = HttpContext.Session.GetString(SessionEndPoint);
            ViewData["publicKey"] = _configuration["VapidKeys:PublicKey"];

            if (_context.Devices != null)
            {
                Device device = _context.Devices.FirstOrDefault(device => device.PushP256DH == endpoint);

                //Get a list of all of the devices patients

                if (_context.Patients != null)
                {
                    Patients = await _context.Patients.Where(p => p.DeviceID == device.ID).ToListAsync();
                }

                foreach (Patient patient in Patients)
                {
                    taskList.AddRange(await _context.PatientDisplayTasks.Where(t => t.PatientID == patient.PatientID).ToListAsync());
                }

                //Get the settings profile
                settingsProfile = _context.SettingsProfiles.FirstOrDefault(s => s.DeviceID == device.ID);

                //Calculate the time period

                // TODO: FIX
                // Note: Not sure what to fix here.
                if (settingsProfile == null)
                {
                    SettingsProfile settings = new SettingsProfile(device.ID, 30, new TimeSpan(6, 0, 0), new TimeSpan(18, 0, 0), true);

                    _context.SettingsProfiles.Add(settings);

                    settingsProfile = settings;

                    await _context.SaveChangesAsync();
                }

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
            }
        }
    }

    public async Task<IActionResult> OnPostCheckSub(string PushEndpoint, string PushP256DH, string PushAuth)
    {
        Device device = _context.Devices.FirstOrDefault(d => d.PushP256DH == PushP256DH);
        Console.WriteLine(device);

        //set the session state
        if (device != null && string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
        {
            HttpContext.Session.SetString(SessionEndPoint, PushP256DH);
            Console.WriteLine("refresh");
            return new JsonResult("false");
        }

        //Device doesnt exist therefore create the device
        if (device == null && string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
        {
            Device Device = new Device(PushEndpoint, PushP256DH, PushAuth);
            _context.Devices.Add(Device);

            await _context.SaveChangesAsync();

            // Add basic setup for testing, Remove in production

            var patients = new Patient[]{
                new Patient(Device.ID, "2AB"),
                new Patient(Device.ID, "3GC")
            };

            _context.Patients.AddRange(patients);

            await _context.SaveChangesAsync();

            // Get the patient ID

            var patientBasicTasks = new PatientTask[]{
                new PatientTask(Device.ID, "OBS", "#2ECC71"),
                new PatientTask(Device.ID, "Blood", "#C0392B"),
                new PatientTask(Device.ID, "IV Med", "#641E16"),
                new PatientTask(Device.ID, "Med", "#2980B9"),
                new PatientTask(Device.ID, "Feed", "#D35400"),
                new PatientTask(Device.ID, "Turn", "#17202A"),
                new PatientTask(Device.ID, "BGL", "#0B5345"),
                new PatientTask(Device.ID, "Document", "#424949")
            };

            _context.PatientTasks.AddRange(patientBasicTasks);

            await _context.SaveChangesAsync();

            // PatientTask task1 = _context.PatientTasks.FirstOrDefault(pt => pt.TaskName == "Task1");
            // PatientTask task2 = _context.PatientTasks.FirstOrDefault(pt => pt.TaskName == "Task2");

            // var patientDisplayTasks = new PatientDisplayTask[]{
            //     new PatientDisplayTask(Device.ID, task1., "Task1", "#B05448", new TimeSpan(10, 0, 0), 111001101),
            //     new PatientDisplayTask(Device.ID, task2., "Task2", "#B05448", new TimeSpan(10, 0, 0), 111001111)
            // };

            // _context.PatientDisplayTasks.AddRange(patientDisplayTasks);

            // await _context.SaveChangesAsync();

            SettingsProfile settings = new SettingsProfile(Device.ID, 30, new TimeSpan(6, 0, 0), new TimeSpan(18, 0, 0), false);

            _context.SettingsProfiles.Add(settings);

            await _context.SaveChangesAsync();

            return new JsonResult("false");
        }

        return new JsonResult("true");
    }
}
