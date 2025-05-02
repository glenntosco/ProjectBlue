using P4LicensePortal.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P4LicensePortal.Services.Interfaces
{
    public interface ILicenseService
    {
        Task<License> GenerateLicenseAsync(Guid tenantId, string productCode, int maxUsers, DateTime expiryDate, bool enableReports, bool enableApi, bool allowBrandingOverrides, bool isSandbox, string createdBy);
        
        Task<License> GetLicenseByIdAsync(Guid licenseId);
        
        Task<IEnumerable<License>> GetLicensesByTenantIdAsync(Guid tenantId);
        
        Task<IEnumerable<License>> GetLicensesByPartnerIdAsync(Guid partnerId);
        
        Task<License> RevokeLicenseAsync(Guid licenseId, string revokedBy);
        
        Task<bool> ValidateLicenseAsync(Guid licenseId);
        
        Task<string> DecryptLicenseAsync(License license);
        
        Task<string> EncryptLicenseAsync(License license);
    }
}
