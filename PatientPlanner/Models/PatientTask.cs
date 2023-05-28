namespace PatientPlanner.Models
{
    public class PatientTask
    {
        public int PatientTaskID { get; set; }
        public string TaskName { get; set; }
        public string TaskColour { get; set; }
        public int DeviceID { get; set; }

        public Device Device { get; set; }

        public PatientTask(int DeviceID, string TaskName, string TaskColour)
        {
            this.DeviceID = DeviceID;
            this.TaskName = TaskName;
            this.TaskColour = TaskColour;
        }

        public PatientTask(int PatientTaskID, int DeviceID, string TaskName, string TaskColour)
        {
            this.PatientTaskID = PatientTaskID;
            this.DeviceID = DeviceID;
            this.TaskName = TaskName;
            this.TaskColour = TaskColour;
        }
    }
}