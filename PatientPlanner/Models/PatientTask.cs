namespace PatientPlanner.Models
{
    public abstract class PatientTask
    {
        public int PatientTaskID { get; set; }
        public string TaskName { get; set; }
        public string TaskColour { get; set; }
        public int PatientID { get; set; }

        public Patient Patient { get; set; }
    }
}