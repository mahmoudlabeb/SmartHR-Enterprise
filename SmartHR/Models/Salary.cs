using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHR.Models
{
    public class Salary
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "الموظف مطلوب")]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [Required]
        public int Month { get; set; } = DateTime.Now.Month;

        [Required]
        public int Year { get; set; } = DateTime.Now.Year;

        // المستحقات
        [Required]
        public decimal BasicSalary { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal OtherAllowances { get; set; }

        // الاستقطاعات
        public decimal TaxDeduction { get; set; }
        public decimal AbsenceDeduction { get; set; }
        public decimal OtherDeductions { get; set; }

        // الصافي (يمكن حسابه برمجياً، ولكن يفضل حفظه كقيمة ثابتة للأرشفة)
        public decimal NetSalary { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Paid

        public DateTime? PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}