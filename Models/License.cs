using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace P4LicensePortal.Models
{
    public class License
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required, MaxLength(50)]
        public string ProductCode { get; set; }

        [Required]
        public int MaxUsers { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(36)]
        public string CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? RevokedDate { get; set; }

        [MaxLength(36)]
        public string RevokedBy { get; set; }

        [Required]
        public string Signature { get; set; }

        [Required]
        public string EncryptedLicense { get; set; }

        // Feature flags
        public bool EnableReports { get; set; }
        public bool EnableAPI { get; set; }
        public bool AllowBrandingOverrides { get; set; }
        public bool IsSandbox { get; set; }

        // Navigation properties
        public virtual Tenant Tenant { get; set; }

        [NotMapped]
        [JsonIgnore]
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;

        [NotMapped]
        [JsonIgnore]
        public bool IsValid => IsActive && !IsExpired;
    }
}
