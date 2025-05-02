using P4LicensePortal.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P4LicensePortal.Services.Interfaces
{
    /// <summary>
    /// Service for license generation, validation, and management
    /// </summary>
    public interface ILicenseService
    {
        /// <summary>
        /// Generate a new license for a tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="productCode">Product code</param>
        /// <param name="maxUsers">Maximum allowed users</param>
        /// <param name="expiryDate">License expiration date</param>
        /// <param name="featureFlags">Optional feature flags</param>
        /// <returns>Generated license object</returns>
        Task<License> GenerateLicenseAsync(Guid tenantId, string productCode, int maxUsers, 
            DateTime expiryDate, Dictionary<string, string> featureFlags = null);
        
        /// <summary>
        /// Validate a license
        /// </summary>
        /// <param name="licenseData">License data to validate</param>
        /// <returns>True if license is valid, false otherwise</returns>
        Task<bool> ValidateLicenseAsync(string licenseData);
        
        /// <summary>
        /// Get a license by ID
        /// </summary>
        /// <param name="licenseId">License identifier</param>
        /// <returns>License if found, null otherwise</returns>
        Task<License> GetLicenseByIdAsync(Guid licenseId);
        
        /// <summary>
        /// Get licenses for a tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Collection of licenses for the tenant</returns>
        Task<IEnumerable<License>> GetLicensesByTenantAsync(Guid tenantId);
        
        /// <summary>
        /// Revoke a license
        /// </summary>
        /// <param name="licenseId">License identifier</param>
        /// <param name="revokedBy">User who revoked the license</param>
        /// <param name="reason">Reason for revocation</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> RevokeLicenseAsync(Guid licenseId, string revokedBy, string reason);
    }
}
