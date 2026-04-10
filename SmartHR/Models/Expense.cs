namespace SmartHR.Models
{
    public class Expense : BaseEntity
    {
        public string ItemName { get; set; } = string.Empty;
        public string PurchasedFrom { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved

        public int PurchasedByEmployeeId { get; set; }
        public virtual Employee? PurchasedByEmployee { get; set; }
    }
}
