namespace SmartHR.Models
{
    public class Training : BaseEntity
    {
        public string TrainingType { get; set; } = string.Empty;
        public decimal TrainingCost { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TrainerId { get; set; }
        public virtual Trainer? Trainer { get; set; }

        public virtual ICollection<Trainee> Trainees { get; set; } = new List<Trainee>();
    }
}
