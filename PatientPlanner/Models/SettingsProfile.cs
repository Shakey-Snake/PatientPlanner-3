namespace PatientPlanner.Models
{

    public class SettingsProfile
    {
        public int SettingsProfileID { get; set; }
        public int DeviceID { get; set; }
        public int Interval { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool EnabledNotification { get; set; }

        public Device Device { get; set; }

        public SettingsProfile(int DeviceID, int Interval, TimeSpan StartTime, TimeSpan EndTime, bool EnabledNotification)
        {
            this.DeviceID = DeviceID;
            this.Interval = Interval;
            this.StartTime = StartTime;
            this.EndTime = EndTime;
            this.EnabledNotification = EnabledNotification;
        }

    }
}

