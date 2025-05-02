using Microsoft.EntityFrameworkCore;
using P4LicensePortal.Data;
using P4LicensePortal.Models;
using P4LicensePortal.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P4LicensePortal.Services.Implementations
{
    public class PartnerService : IPartnerService
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public PartnerService(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        public async Task<Partner> CreatePartnerAsync(Partner partner)
        {
            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();

            // Create default branding
            var branding = new PartnerBranding
            {
                PartnerId = partner.Id,
                PrimaryColor = "#1976d2",
                AccentColor = "#ff4081",
                DefaultTheme = "light"
            };

            _context.PartnerBrandings.Add(branding);
            await _context.SaveChangesAsync();

            // Log the action
            await _auditLogService.LogActionAsync("Partner", partner.Id.ToString(), "Created", "System");

            return partner;
        }

        public async Task<Partner> GetPartnerByIdAsync(Guid partnerId)
        {
            return await _context.Partners
                .Include(p => p.Branding)
                .Include(p => p.KycProfiles)
                .Include(p => p.Certifications)
                .FirstOrDefaultAsync(p => p.Id == partnerId);
        }

        public async Task<IEnumerable<Partner>> GetAllDistributorsAsync()
        {
            return await _context.Partners
                .Where(p => p.PartnerType == "Distributor" && p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Partner>> GetResellersByDistributorIdAsync(Guid distributorId)
        {
            return await _context.Partners
                .Where(p => p.DistributorId == distributorId && p.IsActive)
                .ToListAsync();
        }

        public async Task<Partner> UpdatePartnerAsync(Partner partner)
        {
            _context.Partners.Update(partner);
            await _context.SaveChangesAsync();

            // Log the action
            await _auditLogService.LogActionAsync("Partner", partner.Id.ToString(), "Updated", "System");

            return partner;
        }

        public async Task<Partner> DeactivatePartnerAsync(Guid partnerId, string deactivatedBy)
        {
            var partner = await _context.Partners.FindAsync(partnerId)
                ?? throw new ArgumentException("Partner not found");

            partner.IsActive = false;
            
            _context.Partners.Update(partner);
            await _context.SaveChangesAsync();

            // Log the action
            await _auditLogService.LogActionAsync("Partner", partner.Id.ToString(), "Deactivated", deactivatedBy);

            return partner;
        }

        public async Task<PartnerBranding> UpdateBrandingAsync(PartnerBranding branding)
        {
            var existingBranding = await _context.PartnerBrandings.FindAsync(branding.PartnerId);
            
            if (existingBranding == null)
            {
                _context.PartnerBrandings.Add(branding);
            }
            else
            {
                _context.Entry(existingBranding).CurrentValues.SetValues(branding);
            }
            
            await _context.SaveChangesAsync();

            // Log the action
            await _auditLogService.LogActionAsync("PartnerBranding", branding.PartnerId.ToString(), "Updated", "System");

            return branding;
        }

        public async Task<PartnerBranding> GetBrandingByPartnerIdAsync(Guid partnerId)
        {
            return await _context.PartnerBrandings.FindAsync(partnerId);
        }
    }
}
