using Quartz;
using PatientPlanner.Models;
using Microsoft.EntityFrameworkCore;
using PatientPlanner.Services;

public class SendNotifications : IJob
{
    private readonly PatientPlanner.Data.TimetableContext _context;
    public SendNotifications(PatientPlanner.Data.TimetableContext context)
    {
        _context = context;
    }
    public Task Execute(IJobExecutionContext context)
    {
        TimeSpan localTime = DateTime.UtcNow.TimeOfDay;
        // Job that sends notifications to the user in the form of an alarm or a reminder
        // It will first gather all of the devices which have notifications enabled.
        // Then it will get the tasks that will be alarmed.
        // Then it will get the tasks that will be reminded 
        // and the overlapped tasks from alarm will be removed if they are in the list

        // Get all of the devices that have notifications enabled
        List<Device> devices = _context.Devices.Where(d => (_context.SettingsProfiles.Where(s => s.DeviceID == d.ID && s.EnabledNotification == true)).Select(s => s.DeviceID).Contains(d.ID)).ToList();
        Console.Write(devices);

        foreach (Device device in devices)
        {
            var patients = _context.Patients.Where(p => p.DeviceID == device.ID).ToList();
            var settings = _context.SettingsProfiles.FirstOrDefault(s => s.DeviceID == device.ID);

            // localTime = DateTime.UtcNow.TimeOfDay;

            var adjustedTime = new TimeSpan(0, 0, 0);

            if (settings.TimezoneDiff < 0)
            {
                adjustedTime = localTime.Subtract(new TimeSpan(0, Math.Abs(settings.TimezoneDiff), 0));
                if (adjustedTime < TimeSpan.Zero)
                {
                    adjustedTime = new TimeSpan(24, 0, 0).Add(adjustedTime);
                }
            }
            else
            {
                adjustedTime = new TimeSpan(0, settings.TimezoneDiff, 0).Add(localTime);
            }

            List<PatientDisplayTask> alarmTaskList = new List<PatientDisplayTask>();
            List<PatientDisplayTask> reminderTaskList = new List<PatientDisplayTask>();

            foreach (Patient patient in patients)
            {
                var pdt = _context.PatientDisplayTasks.ToList().Where(t => t.PatientID == patient.PatientID && t.DueTime.Hours == adjustedTime.Hours && t.DueTime.Minutes == adjustedTime.Minutes && t.Completed == false);
                alarmTaskList.AddRange(pdt);

                if (alarmTaskList.FirstOrDefault(pt => pt.PatientID == patient.PatientID) != null)
                {
                    continue;
                }

                pdt = _context.PatientDisplayTasks.ToList().Where(t => t.PatientID == patient.PatientID && t.DueTime >= adjustedTime.Subtract(new TimeSpan(2, 0, 0)) && t.DueTime < adjustedTime && t.Completed == false).ToList();
                reminderTaskList.AddRange(pdt);

                // check for night shift reminders
                // EX: time is 1, minus 2 is 23, therefore it would be larger so it needs to check for night shifts aswell as regular
                if (adjustedTime.Subtract(new TimeSpan(2, 0, 0)) > adjustedTime)
                {
                    // find the diff from now to 0
                    var diff = new TimeSpan(0, 0, 0).Subtract(adjustedTime);
                    // gives a value between 0 and 2, use this to find the upper limit of times
                    var upperTimeSpan = new TimeSpan(0, 0, 0).Subtract(diff);
                    pdt = _context.PatientDisplayTasks.Where(t => t.PatientID == patient.PatientID && t.DueTime >= upperTimeSpan).ToList();
                    reminderTaskList.AddRange(pdt);
                }
            }

            if (alarmTaskList.Count != 0)
            {
                foreach (var task in alarmTaskList)
                {
                    var data = new Dictionary<string, string>()
                        {
                            { "title", "A patient requires attention" },
                            { "body", "Patient " + patients.Find(p => p.PatientID == task.PatientID).RoomNumber + " has " + task.TaskName + " due now at " + task.DueTime.ToString(@"hh\:mm")},
                            { "click_action", "https://localhost:7039/Index" },
                        };
                    NotificationService.Send(device, data);
                }
            }

            if (reminderTaskList.Count != 0)
            {
                foreach (var task in reminderTaskList)
                {
                    var data = new Dictionary<string, string>()
                        {
                            { "title", "It looks like you missed a patient" },
                            { "body", "Patient " + patients.Find(p => p.PatientID == task.PatientID).RoomNumber + " was due " + task.TaskName + " " + adjustedTime.Subtract(task.DueTime).ToString(@"hh\:mm") + " hours ago"},
                            { "click_action", "https://localhost:7039/Index" },
                        };
                    NotificationService.Send(device, data);
                }
            }
        }

        return Task.FromResult(true);
    }
}