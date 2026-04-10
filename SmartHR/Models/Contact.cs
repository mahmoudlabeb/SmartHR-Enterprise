// Models/Contact.cs
using System.ComponentModel.DataAnnotations;

namespace SmartHR.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        public string Name { get; set; } = string.Empty;

        public string? JobTitle { get; set; }

        public string? Company { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "صيغة البريد غير صحيحة")]
        public string? Email { get; set; }

        public string? ContactGroup { get; set; } // مثال: مورد، شريك، فريلانسر
    }
}