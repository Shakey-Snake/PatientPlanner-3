using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public List<PatientTask> baseTaskList { get; set; } = new();
        public SelectList Options { get; set; }
        public List<string> displayTimesList { get; set; } = new();
        public List<int> intervalList { get; set; } = new();
        public List<PatientDisplayTask> taskList { get; set; } = new List<PatientDisplayTask>();
        public int PatientID = 0;

        public async Task OnGetAsync()
        {
            // Check for a session state, then get the device. Otherwise have automatic reminder for the user to refresh the page
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
            {
                var p256dh = HttpContext.Session.GetString(SessionEndPoint);
                if (_context.Devices != null)
                {
                    Device device = _context.Devices.FirstOrDefault(device => device.PushP256DH == p256dh);

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

                    taskList.AddRange(await _context.PatientDisplayTasks.Where(t => t.PatientID == CurrentPatient.PatientID).ToListAsync());

                    baseTaskList.AddRange(await _context.PatientTasks.Where(t => t.DeviceID == device.ID).ToListAsync());

                    Options = new SelectList(baseTaskList, nameof(PatientTask.PatientTaskID), nameof(PatientTask.TaskName));

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
                    // Create a list of intervals using current interval
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

        public async Task<IActionResult> OnPostAddTaskAsync(int patientID, int taskID, string startTime, string endTime, string interval)
        {
            string taskColour = "";
            string taskName = "";
            string[] splitStartTime = startTime.Split(":");
            TimeSpan newStartTime = new TimeSpan(int.Parse(splitStartTime[0]), int.Parse(splitStartTime[1]), 0);

            Random rnd = new Random();
            int groupNum = rnd.Next(1000000, 9999999);

            var task = _context.PatientDisplayTasks.FirstOrDefault(s => s.GroupNumber == groupNum);

            while (task != null)
            {
                groupNum = rnd.Next(1000000, 9999999);

                task = _context.PatientDisplayTasks.FirstOrDefault(s => s.GroupNumber == groupNum);
            }

            // Get the device id from the passed in patientID

            int deviceID = (_context.Patients.FirstOrDefault(p => p.PatientID == patientID)).DeviceID;

            // Get the task colour by finding the task from basic task table

            PatientTask baseTask = _context.PatientTasks.FirstOrDefault(s => s.PatientTaskID == taskID && s.DeviceID == deviceID);

            taskColour = baseTask.TaskColour;

            taskName = baseTask.TaskName;

            // Create multiple tasks with the same group number

            if (interval != null || endTime != null)
            {
                List<PatientDisplayTask> newPTs = new List<PatientDisplayTask>();
                PatientDisplayTask newPT = new PatientDisplayTask(patientID, taskID, taskName, taskColour, newStartTime, groupNum);
                newPTs.Add(newPT);

                string[] splitEndTime = endTime.Split(":");
                TimeSpan newEndTime = new TimeSpan(int.Parse(splitEndTime[0]), int.Parse(splitEndTime[1]), 0);

                TimeSpan intervalSpan = new TimeSpan(0, int.Parse(interval), 0);

                //TODO: Needs some adjustments for tasks that end on the right intervals
                while (newStartTime.Add(intervalSpan) < newEndTime)
                {
                    newStartTime = newStartTime.Add(intervalSpan);
                    newPT = new PatientDisplayTask(patientID, taskID, taskName, taskColour, newStartTime, groupNum);
                    newPTs.Add(newPT);
                }
                // TODO CONT: Possibly add to list here

                _context.PatientDisplayTasks.AddRange(newPTs);
            }
            // Create one single instance of the task
            else
            {
                PatientDisplayTask newPT = new PatientDisplayTask(patientID, taskID, taskName, taskColour, newStartTime, groupNum);

                _context.PatientDisplayTasks.Add(newPT);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Get");
        }

        public async Task<IActionResult> OnPostAddPatient(string roomNumber)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
            {
                var p256dh = HttpContext.Session.GetString(SessionEndPoint);
                if (_context.Devices != null)
                {
                    Device device = _context.Devices.FirstOrDefault(device => device.PushP256DH == p256dh);
                    Patient pateint = new Patient(device.ID, roomNumber);

                    _context.Patients.Add(pateint);
                    await _context.SaveChangesAsync();
                }
            }
            Console.WriteLine("refresh");
            return new JsonResult("false");
        }

        public async Task<IActionResult> OnPostDeleteTask(string taskid)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
            {

                var p256dh = HttpContext.Session.GetString(SessionEndPoint);
                // check if the task belongs to the session for security
                PatientDisplayTask task = _context.PatientDisplayTasks.FirstOrDefault(t => t.PatientDisplayTaskID == Int32.Parse(taskid));

                if (task != null)
                {

                    Patient patient = _context.Patients.FirstOrDefault(p => p.PatientID == task.PatientID);
                    Device device = _context.Devices.FirstOrDefault(device => device.PushP256DH == p256dh);

                    if (patient != null && device != null && patient.DeviceID == device.ID)
                    {
                        // task exists for the current device therefore remove it.
                        _context.PatientDisplayTasks.Remove(task);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            Console.WriteLine("refresh");
            return new JsonResult("false");
        }

        public async Task<IActionResult> OnPostSaveBaseTask(int taskid, string taskColour, string taskName)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
            {
                // create the task, then remove the task in the database and re add it with the new attributes
                PatientTask updateTask = _context.PatientTasks.FirstOrDefault(p => p.PatientTaskID == taskid);
                // NOTE: entity becomes tracked therefore no need to use update query
                updateTask.TaskColour = taskColour;
                updateTask.TaskName = taskName;

                // change all tasks in the display task table
                List<PatientDisplayTask> changeDisplayTasks = new List<PatientDisplayTask>();
                changeDisplayTasks.AddRange(await _context.PatientDisplayTasks.Where(pt => pt.PatientTaskID == updateTask.PatientTaskID).ToListAsync());

                changeDisplayTasks.Select(pt => { pt.TaskName = taskName; pt.TaskColour = taskColour; return pt; }).ToList();

                _context.PatientDisplayTasks.UpdateRange(changeDisplayTasks);

                await _context.SaveChangesAsync();
            }
            return new JsonResult("false");
        }
    }
}
