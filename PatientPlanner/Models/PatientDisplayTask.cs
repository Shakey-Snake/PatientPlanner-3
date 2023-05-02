namespace PatientPlanner.Models
{
    public class PatientDisplayTask : PatientTask
    {
        public TimeSpan DueTime { get; set; }
        public int ChildPatientID { get; set; }

        public PatientDisplayTask(int PatientID, string TaskName, TimeSpan DueTime, string TaskColour, bool BasicTask)
        {
            this.PatientID = PatientID;
            this.TaskName = TaskName;
            this.TaskColour = TaskColour;
        }
    }
}