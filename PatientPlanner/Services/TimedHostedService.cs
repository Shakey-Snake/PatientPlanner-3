using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PatientPlanner.Models;
using PatientPlanner.Data;

namespace PatientPlanner.Services;
public class TimedHostedService : IHostedService, IDisposable
{
    public class Payload
    {
        public string title { get; set; }
        public string message { get; set; }
    }
    private int executionCount = 0;
    private readonly IDbContextFactory<TimetableContext> _contextFactory;
    private readonly ILogger<TimedHostedService> _logger;
    private readonly IConfiguration _configuration;
    private Timer? _alarmTimer = null;
    private Timer? _reminderTimer = null;
    private TimeSpan localTime = DateTime.UtcNow.TimeOfDay;

    public TimedHostedService(IDbContextFactory<TimetableContext> contextFactory, ILogger<TimedHostedService> logger, IConfiguration configuration)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        //Every 5 minutes
        // sync to every 5 minutes

        _alarmTimer = new Timer(SendAlarmNotification, null, TimeSpan.Zero,
            TimeSpan.FromMinutes(1));

        _reminderTimer = new Timer(SendReminderNotification, null, TimeSpan.Zero,
            TimeSpan.FromMinutes(10));

        return Task.CompletedTask;
    }

    private void SendNotification(string type)
    {
        // Get all devices, for each device get the list of patient tasks that have their time no greater than 2 hours ago.
        // send one reminder every 5 minutes, that reminds the nurse of the most recent task they have yet to complete.
        // example format: You have not seen patient [roomNumber] for [taskname] aswell as patient [roomNumber], [roomNumber] and others.
        using (TimetableContext dbContext = _contextFactory.CreateDbContext())
        {
            var deviceList = dbContext.Devices.ToList();
            List<int> filteredDeviceIDs = new List<int>();

            foreach (Device device in deviceList)
            {
                filteredDeviceIDs.Add(dbContext.SettingsProfiles.Where(s => s.DeviceID == device.ID && s.EnabledNotification == true).Select(s => s.DeviceID).FirstOrDefault());
            }

            deviceList = deviceList.Where(d => filteredDeviceIDs.Contains(d.ID)).ToList();

            foreach (Device device in deviceList)
            {
                var settings = dbContext.SettingsProfiles.FirstOrDefault(s => s.DeviceID == device.ID);
                var patients = dbContext.Patients.Where(p => p.DeviceID == device.ID).ToList();

                localTime = DateTime.UtcNow.TimeOfDay;
                _logger.LogInformation("local time: {localTime}", localTime);

                var adjustedTime = new TimeSpan(0, 0, 0);

                _logger.LogInformation("TimezoneDiff {TimezoneDiff}", settings.TimezoneDiff);

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

                _logger.LogInformation("adjustedTime: {adjustedTime}", adjustedTime.Negate());
                if (type == " Alarm!")
                {
                    foreach (Patient patient in patients)
                    {
                        List<PatientDisplayTask> taskList = new List<PatientDisplayTask>();
                        var pdt = dbContext.PatientDisplayTasks.ToList().Where(t => t.PatientID == patient.PatientID && t.DueTime.Hours == adjustedTime.Hours && t.DueTime.Minutes == adjustedTime.Minutes && t.Completed == false);
                        taskList.AddRange(pdt);

                        if (taskList.Count != 0)
                        {
                            //Create the message string using the first elements for now
                            string message = "Patient " + patients.Find(p => p.PatientID == taskList[0].PatientID).RoomNumber +
                                " has " + taskList[0].TaskName + " due now at " + taskList[0].DueTime.ToString(@"hh\:mm");

                            var payload = new Payload
                            {
                                title = "A patient requires attention",
                                message = message
                            };

                            string payloadJsonString = JsonSerializer.Serialize(payload);
                            NotificationService.Send(device, payloadJsonString, _configuration);
                        }
                    }
                }
                else
                {
                    foreach (Patient patient in patients)
                    {
                        List<PatientDisplayTask> taskList = new List<PatientDisplayTask>();
                        // check for diff of 2 hours
                        var pdt = dbContext.PatientDisplayTasks.ToList().Where(t => t.PatientID == patient.PatientID && t.DueTime >= adjustedTime.Subtract(new TimeSpan(2, 0, 0)) && t.DueTime < adjustedTime && t.Completed == false).ToList();
                        taskList.AddRange(pdt);

                        _logger.LogInformation("adjustedTime: {adjustedTime}", adjustedTime.Subtract(new TimeSpan(2, 0, 0)));
                        _logger.LogInformation("adjustedTime: {adjustedTime}", adjustedTime);

                        // check for night shift reminders
                        // EX: time is 1, minus 2 is 23, therefore it would be larger so it needs to check for night shifts aswell as regular
                        if (adjustedTime.Subtract(new TimeSpan(2, 0, 0)) > adjustedTime)
                        {
                            _logger.LogInformation("nightshift");
                            // find the diff from now to 0
                            var diff = new TimeSpan(0, 0, 0).Subtract(adjustedTime);
                            // gives a value between 0 and 2, use this to find the upper limit of times
                            var upperTimeSpan = new TimeSpan(0, 0, 0).Subtract(diff);
                            pdt = dbContext.PatientDisplayTasks.Where(t => t.PatientID == patient.PatientID && t.DueTime >= upperTimeSpan).ToList();
                            taskList.AddRange(pdt);
                        }

                        if (taskList.Count != 0)
                        {
                            //Create the message string using the first elements for now
                            // TODO: test this

                            string message = "Patient " + patients.Find(p => p.PatientID == taskList[0].PatientID).RoomNumber +
                                " was due " + taskList[0].TaskName + " " + adjustedTime.Subtract(taskList[0].DueTime).ToString(@"hh\:mm") + " hours ago at " + taskList[0].DueTime;

                            var payload = new Payload
                            {
                                title = "It looks like you missed a patient",
                                message = message
                            };

                            string payloadJsonString = JsonSerializer.Serialize(payload);
                            NotificationService.Send(device, payloadJsonString, _configuration);
                        }
                    }


                }

                // get the patient tasks

                // get the tasks that match the current device, and are before the current time, but greater than 2 hours ago
                // NOTE: could make 2 read calls for a reminder and for an alarm.
            }
        }
    }

    private void SendAlarmNotification(object? state)
    {
        SendNotification(" Alarm!");
        var count = Interlocked.Increment(ref executionCount);

        _logger.LogInformation(
            "Alarm being sent, Timed Hosted Service is working. Count: {Count}", count);
    }

    private void SendReminderNotification(object? state)
    {
        SendNotification(" Reminder!");

        var count = Interlocked.Increment(ref executionCount);

        _logger.LogInformation(
            "Reminder being sent, Timed Hosted Service is working. Count: {Count}", count);
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _alarmTimer?.Change(Timeout.Infinite, 0);
        _reminderTimer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _alarmTimer?.Dispose();
        _reminderTimer?.Dispose();
    }
}