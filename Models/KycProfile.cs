using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P4LicensePortal.Models
{
    public class KycProfile
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PartnerId { get; set; }

        [Required, MaxLength(100)]
        public string CompanyName { get; set; }

        [Required, MaxLength(250)]
        public string LegalAddress { get; set; }

        [Required, MaxLength(50)]
        public string TaxId { get; set; }

        [MaxLength(100)]
        public string RegistrationNumber { get; set; }

        [Required, MaxLength(100)]
        public string ContactName { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string ContactEmail { get; set; }

        [MaxLength(20)]
        public string ContactPhone { get; set; }

        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime? VerificationDate { get; set; }

        [MaxLength(36)]
        public string VerifiedBy { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [MaxLength(500)]
        public string RejectionReason { get; set; }

        [MaxLength(255)]
        public string DocumentUrl1 { get; set; }

        [MaxLength(255)]
        public string DocumentUrl2 { get; set; }

        [MaxLength(255)]
        public string DocumentUrl3 { get; set; }

        // Navigation property
        public virtual Partner Partner { get; set; }

        [NotMapped]
        public bool IsExpired => ExpiryDate.HasValue && DateTime.UtcNow > ExpiryDate;

        [NotMapped]
        public bool IsVerified => Status == "Approved" && !IsExpired;
    }
}
