using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
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
        public DateTime IssuedDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required, MaxLength(8000)]
        public string EncryptedData { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } // Active, Revoked, Expired

        public DateTime? RevokedDate { get; set; }

        [MaxLength(100)]
        public string RevokedBy { get; set; }

        [MaxLength(500)]
        public string RevocationReason { get; set; }

        [MaxLength(4000)]
        public string FeatureFlags { get; set; }

        // Navigation properties
        public virtual Tenant Tenant { get; set; }

        [NotMapped]
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;

        [NotMapped]
        public bool IsActive => Status == "Active" && !IsExpired;

        [NotMapped]
        public Dictionary<string, string> FeatureFlagsDictionary
        {
            get
            {
                if (string.IsNullOrEmpty(FeatureFlags))
                    return new Dictionary<string, string>();

                try
                {
                    return JsonSerializer.Deserialize<Dictionary<string, string>>(FeatureFlags);
                }
                catch
                {
                    return new Dictionary<string, string>();
                }
            }
        }

        public bool HasFeature(string featureName)
        {
            return FeatureFlagsDictionary.ContainsKey(featureName) &&
                   FeatureFlagsDictionary[featureName].Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public int GetFeatureIntValue(string featureName, int defaultValue = 0)
        {
            if (!FeatureFlagsDictionary.ContainsKey(featureName))
                return defaultValue;

            if (int.TryParse(FeatureFlagsDictionary[featureName], out int value))
                return value;

            return defaultValue;
        }
    }
}
