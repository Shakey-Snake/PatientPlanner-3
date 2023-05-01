namespace PatientPlanner.Models
{
    public class Device
    {
        public int ID { get; set; }
        public string PushEndpoint { get; set; }
        public string PushP256DH { get; set; }
        public string PushAuth { get; set; }

        public ICollection<Patient> Patients { get; set; }
        public SettingsProfile SettingsProfile { get; set; }

        public Device(string PushEndpoint, string PushP256DH, string PushAuth)
        {
            this.PushEndpoint = PushEndpoint;
            this.PushP256DH = PushP256DH;
            this.PushAuth = PushAuth;
        }
    }
}

