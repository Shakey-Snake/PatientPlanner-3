namespace PatientPlanner.Models
{
    public class PatientBaseTask : PatientTask
    {
        // A boolean value for the add task form for default tasks
        public bool BasicTask { get; set; }

        public PatientBaseTask(int PatientID, string TaskName, string TaskColour, bool BasicTask)
        {
            this.PatientID = PatientID;
            this.TaskName = TaskName;
            this.TaskColour = TaskColour;
            this.BasicTask = BasicTask;
        }
    }
}