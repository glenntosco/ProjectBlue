using P4LicensePortal.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P4LicensePortal.Services.Interfaces
{
    public interface IPartnerService
    {
        Task<Partner> CreatePartnerAsync(Partner partner);
        
        Task<Partner> GetPartnerByIdAsync(Guid partnerId);
        
        Task<IEnumerable<Partner>> GetAllDistributorsAsync();
        
        Task<IEnumerable<Partner>> GetResellersByDistributorIdAsync(Guid distributorId);
        
        Task<Partner> UpdatePartnerAsync(Partner partner);
        
        Task<Partner> DeactivatePartnerAsync(Guid partnerId, string deactivatedBy);
        
        Task<PartnerBranding> UpdateBrandingAsync(PartnerBranding branding);
        
        Task<PartnerBranding> GetBrandingByPartnerIdAsync(Guid partnerId);
    }
}
