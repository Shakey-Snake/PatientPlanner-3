
namespace PatientPlanner.Models
{

    public class Patient
    {
        public int PatientID { get; set; }
        public int DeviceID { get; set; }
        public string RoomNumber { get; set; }

        public ICollection<PatientTask> PatientTasks { get; set; }

        public Patient(int DeviceID, string RoomNumber)
        {
            this.DeviceID = DeviceID;
            this.RoomNumber = RoomNumber;
        }

    }
}