using System;
using System.ComponentModel.DataAnnotations;

namespace P4LicensePortal.Models
{
    public class Certification
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(250)]
        public string Description { get; set; }

        public bool RequiresExam { get; set; }

        public bool RequiresRenewal { get; set; }

        [MaxLength(50)]
        public string Tier { get; set; } // Basic, Professional, Enterprise

        public int RenewalPeriodMonths { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [MaxLength(255)]
        public string LogoUrl { get; set; }
    }
}
