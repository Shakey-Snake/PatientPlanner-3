namespace PatientPlanner.Models
{
    public class PatientDisplayTask
    {
        public int PatientDisplayTaskID { get; set; }
        public string TaskName { get; set; }
        public string TaskColour { get; set; }
        public TimeSpan DueTime { get; set; }
        public int GroupNumber { get; set; }
        public int PatientID { get; set; }
        public int PatientTaskID { get; set; }
        public bool Completed { get; set; }
        public Patient Patient { get; set; }

        public PatientDisplayTask(int PatientID, int PatientTaskID, string TaskName, string TaskColour, TimeSpan DueTime, int GroupNumber, bool Completed)
        {
            this.PatientID = PatientID;
            this.PatientTaskID = PatientTaskID;
            this.TaskName = TaskName;
            this.DueTime = DueTime;
            this.TaskColour = TaskColour;
            this.GroupNumber = GroupNumber;
            this.Completed = Completed;
        }
    }
}