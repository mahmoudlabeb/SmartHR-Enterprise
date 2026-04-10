namespace SmartHR.Models
{
    public class PaySlip : BaseEntity
    {
        public int SalaryId { get; set; }
        public virtual Salary? Salary { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime PaymentDate { get; set; }
        public bool IsPaid { get; set; } = false;
    }
}
