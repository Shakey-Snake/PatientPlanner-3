namespace PatientPlanner.Models
{
    public class PatientTask
    {
        public int PatientTaskID { get; set; }
        public int PatientID { get; set; }
        public string TaskName { get; set; }
        public TimeSpan DueTime { get; set; }
        public string TaskColour { get; set; }
        public int ChildPatientID { get; set; }
        public bool BasicTask { get; set; }

        public Patient Patient { get; set; }

        public PatientTask(int PatientID, string TaskName, TimeSpan DueTime, string TaskColour)
        {
            this.PatientID = PatientID;
            this.TaskName = TaskName;
            this.DueTime = DueTime;
            this.TaskColour = TaskColour;
        }
    }
}