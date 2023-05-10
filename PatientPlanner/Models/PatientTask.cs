namespace PatientPlanner.Models
{
    public class PatientTask
    {
        public int PatientTaskID { get; set; }
        public string TaskName { get; set; }
        public string TaskColour { get; set; }
        public int PatientID { get; set; }

        public Patient Patient { get; set; }

        public PatientTask(int PatientID, string TaskName, string TaskColour)
        {
            this.PatientID = PatientID;
            this.TaskName = TaskName;
            this.TaskColour = TaskColour;
        }
    }
}