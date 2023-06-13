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
        public SelectList SettingsTimes { get; set; }
        public SelectList SettingsIntervalMinutes { get; set; }
        public SelectList Options { get; set; }
        public List<string> displayTimesList { get; set; } = new();
        public SelectList SettingsIntervalMinutesTask { get; set; }
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

                        if (Patients.Count() > 0)
                        {
                            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionCurrentPatient)))
                            {
                                CurrentPatient = _context.Patients.FirstOrDefault(p => p.PatientID == Int32.Parse(HttpContext.Session.GetString(SessionCurrentPatient)));
                            }
                            else
                            {
                                CurrentPatient = Patients[0];
                            }

                            PatientID = CurrentPatient.PatientID;

                            taskList.AddRange(await _context.PatientDisplayTasks.Where(t => t.PatientID == CurrentPatient.PatientID).ToListAsync());
                        }
                    }

                    baseTaskList.AddRange(await _context.PatientTasks.Where(t => t.DeviceID == device.ID).ToListAsync());

                    Options = new SelectList(baseTaskList, nameof(PatientTask.PatientTaskID), nameof(PatientTask.TaskName));

                    // NOTE: not the most beautiful answer
                    SettingsTimes = new SelectList(
                            new List<SelectListItem>{
                            new SelectListItem { Text = "00:00", Value = "00:00"},
                            new SelectListItem { Text = "01:00", Value = "01:00"},
                            new SelectListItem { Text = "02:00", Value = "02:00"},
                            new SelectListItem { Text = "03:00", Value = "03:00"},
                            new SelectListItem { Text = "04:00", Value = "04:00"},
                            new SelectListItem { Text = "05:00", Value = "05:00"},
                            new SelectListItem { Text = "06:00", Value = "06:00"},
                            new SelectListItem { Text = "07:00", Value = "07:00"},
                            new SelectListItem { Text = "08:00", Value = "08:00"},
                            new SelectListItem { Text = "09:00", Value = "09:00"},
                            new SelectListItem { Text = "10:00", Value = "10:00"},
                            new SelectListItem { Text = "11:00", Value = "11:00"},
                            new SelectListItem { Text = "12:00", Value = "12:00"},
                            new SelectListItem { Text = "13:00", Value = "13:00"},
                            new SelectListItem { Text = "14:00", Value = "14:00"},
                            new SelectListItem { Text = "15:00", Value = "15:00"},
                            new SelectListItem { Text = "16:00", Value = "16:00"},
                            new SelectListItem { Text = "17:00", Value = "17:00"},
                            new SelectListItem { Text = "18:00", Value = "18:00"},
                            new SelectListItem { Text = "19:00", Value = "19:00"},
                            new SelectListItem { Text = "20:00", Value = "20:00"},
                            new SelectListItem { Text = "21:00", Value = "21:00"},
                            new SelectListItem { Text = "22:00", Value = "22:00"},
                            new SelectListItem { Text = "23:00", Value = "23:00"},
                            }, "Value", "Text"
                        );

                    SettingsIntervalMinutes = new SelectList(
                        new List<SelectListItem>{
                            new SelectListItem { Text = "00:05", Value = "5"},
                            new SelectListItem { Text = "00:10", Value = "10"},
                            new SelectListItem { Text = "00:15", Value = "15"},
                            new SelectListItem { Text = "00:30", Value = "30"},
                            new SelectListItem { Text = "1:00", Value = "60"},
                            new SelectListItem { Text = "2:00", Value = "120"},
                            new SelectListItem { Text = "3:00", Value = "180"},
                            new SelectListItem { Text = "4:00", Value = "240"},
                        }, "Value", "Text"
                    );

                    //Get the settings profile
                    settingsProfile = _context.SettingsProfiles.FirstOrDefault(s => s.DeviceID == device.ID);

                    //Calculate the time period

                    times = new List<TimeSpan>();

                    if (settingsProfile.StartTime > settingsProfile.EndTime)
                    {
                        var startTime = settingsProfile.StartTime;
                        var tempTime = new TimeSpan(24, 0, 0);
                        while (startTime < tempTime)
                        {
                            times.Add(startTime);
                            startTime = startTime.Add(new TimeSpan(0, settingsProfile.Interval, 0));
                        }

                        tempTime = new TimeSpan(0, 0, 0);
                        while (tempTime < settingsProfile.EndTime.Add(new TimeSpan(0, settingsProfile.Interval, 0)))
                        {
                            times.Add(tempTime);
                            tempTime = tempTime.Add(new TimeSpan(0, settingsProfile.Interval, 0));
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

                    SettingsIntervalMinutesTask = new SelectList(SettingsIntervalMinutes.Where(s => Int32.Parse(s.Value) >= settingsProfile.Interval), "Value", "Text");

                    // Create a list of intervals using current interval


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
                string[] splitEndTime = endTime.Split(":");
                TimeSpan newEndTime = new TimeSpan(int.Parse(splitEndTime[0]), int.Parse(splitEndTime[1]), 0);

                TimeSpan intervalSpan = new TimeSpan(0, int.Parse(interval), 0);

                // TODO: Add special case for if the startTime is greater than endTime
                List<PatientDisplayTask> newPTs = new List<PatientDisplayTask>();
                PatientDisplayTask newPT = new PatientDisplayTask(patientID, taskID, taskName, taskColour, newStartTime, groupNum);
                newPTs.Add(newPT);


                // TODO: Refactor the code for less temp variables to be used
                if (newStartTime > newEndTime)
                {
                    var tempTime = new TimeSpan(24, 0, 0);
                    while (newStartTime.Add(intervalSpan) < tempTime)
                    {
                        newStartTime = newStartTime.Add(intervalSpan);
                        newPT = new PatientDisplayTask(patientID, taskID, taskName, taskColour, newStartTime, groupNum);
                        newPTs.Add(newPT);
                    }

                    tempTime = new TimeSpan(0, 0, 0);

                    newStartTime = newStartTime.Add(intervalSpan);
                    newPT = new PatientDisplayTask(patientID, taskID, taskName, taskColour, tempTime, groupNum);
                    newPTs.Add(newPT);

                    while (tempTime.Add(intervalSpan) < newEndTime)
                    {
                        tempTime = tempTime.Add(intervalSpan);
                        newPT = new PatientDisplayTask(patientID, taskID, taskName, taskColour, tempTime, groupNum);
                        newPTs.Add(newPT);
                    }

                    //TODO: repeated code can be refactored
                    if (tempTime.Add(intervalSpan) == newEndTime)
                    {
                        tempTime = tempTime.Add(intervalSpan);
                        newPT = new PatientDisplayTask(patientID, taskID, taskName, taskColour, tempTime, groupNum);
                        newPTs.Add(newPT);
                    }
                }
                else
                {
                    while (newStartTime.Add(intervalSpan) < newEndTime)
                    {
                        newStartTime = newStartTime.Add(intervalSpan);
                        newPT = new PatientDisplayTask(patientID, taskID, taskName, taskColour, newStartTime, groupNum);
                        newPTs.Add(newPT);
                    }

                    //TODO: repeated code can be refactored
                    if (newStartTime.Add(intervalSpan) == newEndTime)
                    {
                        newStartTime = newStartTime.Add(intervalSpan);
                        newPT = new PatientDisplayTask(patientID, taskID, taskName, taskColour, newStartTime, groupNum);
                        newPTs.Add(newPT);
                    }
                }
                _context.PatientDisplayTasks.AddRange(newPTs);
            }
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

                    // sets the current patient session to the most recently added patient
                    Patient recentPatient = _context.Patients.OrderBy(p => p.PatientID).LastOrDefault(p => p.DeviceID == device.ID);
                    HttpContext.Session.SetString(SessionCurrentPatient, recentPatient.PatientID.ToString());
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

        public async Task<IActionResult> OnPostDeleteBaseTask(int taskid)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
            {
                // find the task, then remove it in the base task list
                PatientTask deleteTask = _context.PatientTasks.FirstOrDefault(p => p.PatientTaskID == taskid);
                _context.PatientTasks.Remove(deleteTask);

                // find all display tasks and remove them
                List<PatientDisplayTask> deleteDisplayTasks = new List<PatientDisplayTask>();
                deleteDisplayTasks.AddRange(await _context.PatientDisplayTasks.Where(pt => pt.PatientTaskID == deleteTask.PatientTaskID).ToListAsync());
                _context.PatientDisplayTasks.RemoveRange(deleteDisplayTasks);

                await _context.SaveChangesAsync();
            }
            return new JsonResult("false");
        }

        public async Task<IActionResult> OnPostAddBaseTask(string taskName, string taskColour)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
            {
                var p256dh = HttpContext.Session.GetString(SessionEndPoint);

                if (_context.Devices != null)
                {
                    Device device = _context.Devices.FirstOrDefault(device => device.PushP256DH == p256dh);
                    Patient patient = _context.Patients.FirstOrDefault(p => p.DeviceID == device.ID);

                    PatientTask task = new PatientTask(patient.PatientID, taskName, taskColour);

                    _context.PatientTasks.Add(task);
                    await _context.SaveChangesAsync();
                }
            }
            return new JsonResult("false");
        }

        // sets the start and end time
        public async Task<IActionResult> OnPostSetTimes(string startTime, string endTime)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
            {
                var p256dh = HttpContext.Session.GetString(SessionEndPoint);
                if (_context.Devices != null)
                {

                    Device device = _context.Devices.FirstOrDefault(device => device.PushP256DH == p256dh);
                    SettingsProfile settings = _context.SettingsProfiles.FirstOrDefault(s => s.DeviceID == device.ID);

                    string[] splitStartTime = startTime.Split(":");
                    string[] splitEndTime = endTime.Split(":");

                    settings.StartTime = new TimeSpan(Int32.Parse(splitStartTime[0]), Int32.Parse(splitStartTime[1]), 0);
                    settings.EndTime = new TimeSpan(Int32.Parse(splitEndTime[0]), Int32.Parse(splitEndTime[1]), 0);

                    await _context.SaveChangesAsync();
                }
            }
            Console.WriteLine("refresh");
            return new JsonResult("false");
        }

        public async Task<IActionResult> OnPostSetInterval(string interval)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
            {
                var p256dh = HttpContext.Session.GetString(SessionEndPoint);
                if (_context.Devices != null)
                {

                    Device device = _context.Devices.FirstOrDefault(device => device.PushP256DH == p256dh);
                    SettingsProfile settings = _context.SettingsProfiles.FirstOrDefault(s => s.DeviceID == device.ID);

                    settings.Interval = Int32.Parse(interval);

                    await _context.SaveChangesAsync();
                }
            }
            Console.WriteLine("refresh");
            return new JsonResult("false");
        }

        public async Task<IActionResult> OnPostSavePatient(int patientID, string patientRN)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
            {
                Patient patient = _context.Patients.FirstOrDefault(p => p.PatientID == patientID);
                // NOTE: entity becomes tracked therefore no need to use update query
                patient.RoomNumber = patientRN;

                await _context.SaveChangesAsync();
            }
            return new JsonResult("false");
        }

        public async Task<IActionResult> OnPostDeletePatient(int patientID)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionEndPoint)))
            {
                Patient patient = _context.Patients.FirstOrDefault(p => p.PatientID == patientID);
                // NOTE: entity becomes tracked therefore no need to use update query
                if (patient != null)
                {
                    _context.Remove(patient);
                    await _context.SaveChangesAsync();
                }
            }
            return new JsonResult("false");
        }
    }
}