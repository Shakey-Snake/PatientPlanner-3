namespace PatientPlanner.Models
{
    public class Device
    {
        public int ID { get; set; }
        public string Token { get; set; }
        public ICollection<Patient> Patients { get; set; }
        public SettingsProfile SettingsProfile { get; set; }

        public Device(string Token)
        {
            this.Token = Token;
        }
    }
}

