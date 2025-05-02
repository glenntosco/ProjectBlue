using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace P4LicensePortal.Models
{
    public class Tenant
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(63)]
        public string Subdomain { get; set; }

        [Required]
        public Guid PartnerId { get; set; }

        [Required, MaxLength(250)]
        public string ConnectionString { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(36)]
        public string CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string Region { get; set; } = "us";

        [MaxLength(50)]
        public string Language { get; set; } = "en-US";

        [MaxLength(255)]
        public string ProductVersion { get; set; }

        // Navigation properties
        public virtual Partner Partner { get; set; }
        public virtual ICollection<License> Licenses { get; set; }
    }
}
