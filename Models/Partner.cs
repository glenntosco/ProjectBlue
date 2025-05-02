using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace P4LicensePortal.Models
{
    public class Partner
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(250)]
        public string CompanyAddress { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(100)]
        public string PartnerType { get; set; } // "Distributor", "Reseller"

        public Guid? DistributorId { get; set; } // Only for Resellers

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Tenant> Tenants { get; set; }
        public virtual ICollection<KycProfile> KycProfiles { get; set; }
        public virtual ICollection<PartnerCertification> Certifications { get; set; }
        public virtual PartnerBranding Branding { get; set; }

        // Self-referencing relationship for Distributor-Reseller
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Partner Distributor { get; set; }
        
        public virtual ICollection<Partner> Resellers { get; set; }
    }

    public class PartnerBranding
    {
        [Key]
        public Guid PartnerId { get; set; }
        
        [MaxLength(255)]
        public string LogoUrl { get; set; }
        
        [MaxLength(50)]
        public string PrimaryColor { get; set; }
        
        [MaxLength(50)]
        public string AccentColor { get; set; }
        
        public string EmailFooterHtml_EN { get; set; }
        
        public string EmailFooterHtml_ES { get; set; }
        
        [MaxLength(255)]
        public string FaviconUrl { get; set; }
        
        [MaxLength(20)]
        public string DefaultTheme { get; set; } = "light"; // "light" or "dark"

        // Navigation property
        public virtual Partner Partner { get; set; }
    }

    public class PartnerCertification
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid PartnerId { get; set; }
        
        [Required, MaxLength(100)]
        public string CertificationType { get; set; }
        
        [Required]
        public DateTime AwardedDate { get; set; }
        
        [Required]
        public DateTime ExpiryDate { get; set; }
        
        [MaxLength(255)]
        public string CertificationUrl { get; set; }
        
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual Partner Partner { get; set; }
    }
}
