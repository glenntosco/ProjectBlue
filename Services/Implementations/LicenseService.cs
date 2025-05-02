using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using P4LicensePortal.Data;
using P4LicensePortal.Models;
using P4LicensePortal.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace P4LicensePortal.Services.Implementations
{
    public class LicenseService : ILicenseService
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;
        private readonly IConfiguration _configuration;

        public LicenseService(AppDbContext context, IAuditLogService auditLogService, IConfiguration configuration)
        {
            _context = context;
            _auditLogService = auditLogService;
            _configuration = configuration;
        }

        public async Task<License> GenerateLicenseAsync(Guid tenantId, string productCode, int maxUsers, DateTime expiryDate, bool enableReports, bool enableApi, bool allowBrandingOverrides, bool isSandbox, string createdBy)
        {
            var tenant = await _context.Tenants.FindAsync(tenantId) 
                ?? throw new ArgumentException("Tenant not found");

            var license = new License
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductCode = productCode,
                MaxUsers = maxUsers,
                ExpiryDate = expiryDate,
                EnableReports = enableReports,
                EnableAPI = enableApi,
                AllowBrandingOverrides = allowBrandingOverrides,
                IsSandbox = isSandbox,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            // Create JSON representation for signing
            var licenseData = JsonSerializer.Serialize(new
            {
                license.Id,
                license.TenantId,
                license.ProductCode,
                license.MaxUsers,
                license.ExpiryDate,
                license.EnableReports,
                license.EnableAPI,
                license.AllowBrandingOverrides,
                license.IsSandbox
            });

            // Sign the license with Ed25519
            license.Signature = SignData(licenseData);

            // Encrypt the license
            license.EncryptedLicense = await EncryptLicenseAsync(license);

            // Save to database
            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();

            // Log the action
            await _auditLogService.LogActionAsync("License", license.Id.ToString(), "Created", createdBy);

            return license;
        }

        public async Task<License> GetLicenseByIdAsync(Guid licenseId)
        {
            return await _context.Licenses
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(l => l.Id == licenseId);
        }

        public async Task<IEnumerable<License>> GetLicensesByTenantIdAsync(Guid tenantId)
        {
            return await _context.Licenses
                .Where(l => l.TenantId == tenantId)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<License>> GetLicensesByPartnerIdAsync(Guid partnerId)
        {
            return await _context.Licenses
                .Include(l => l.Tenant)
                .Where(l => l.Tenant.PartnerId == partnerId)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }

        public async Task<License> RevokeLicenseAsync(Guid licenseId, string revokedBy)
        {
            var license = await _context.Licenses.FindAsync(licenseId)
                ?? throw new ArgumentException("License not found");

            license.IsActive = false;
            license.RevokedDate = DateTime.UtcNow;
            license.RevokedBy = revokedBy;

            _context.Licenses.Update(license);
            await _context.SaveChangesAsync();

            // Log the action
            await _auditLogService.LogActionAsync("License", license.Id.ToString(), "Revoked", revokedBy);

            return license;
        }

        public async Task<bool> ValidateLicenseAsync(Guid licenseId)
        {
            var license = await _context.Licenses.FindAsync(licenseId)
                ?? throw new ArgumentException("License not found");

            if (!license.IsActive || license.IsExpired)
                return false;

            // Recreate the license data for signature verification
            var licenseData = JsonSerializer.Serialize(new
            {
                license.Id,
                license.TenantId,
                license.ProductCode,
                license.MaxUsers,
                license.ExpiryDate,
                license.EnableReports,
                license.EnableAPI,
                license.AllowBrandingOverrides,
                license.IsSandbox
            });

            // Verify the signature
            return VerifySignature(licenseData, license.Signature);
        }

        public async Task<string> DecryptLicenseAsync(License license)
        {
            // In a real implementation, you would decrypt using AES-256-GCM
            // For simplicity, this is just a placeholder implementation
            await Task.CompletedTask; // To make this async
            
            try
            {
                var aesKey = _configuration["CryptoSettings:AesKey"];
                // Actual decryption logic would go here
                
                return "Decrypted license data";
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> EncryptLicenseAsync(License license)
        {
            // In a real implementation, you would encrypt using AES-256-GCM
            // For simplicity, this is just a placeholder implementation
            await Task.CompletedTask; // To make this async
            
            try
            {
                var aesKey = _configuration["CryptoSettings:AesKey"];
                // Actual encryption logic would go here
                
                return "Encrypted license data";
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string SignData(string data)
        {
            // In a real implementation, you would sign using Ed25519
            // For simplicity, this is just a placeholder implementation
            
            var privateKey = _configuration["CryptoSettings:Ed25519PrivateKey"];
            // Actual signature logic would go here
            
            return Convert.ToBase64String(Encoding.UTF8.GetBytes("Simulated Ed25519 signature"));
        }

        private bool VerifySignature(string data, string signature)
        {
            // In a real implementation, you would verify using Ed25519
            // For simplicity, this is just a placeholder implementation
            
            var publicKey = _configuration["CryptoSettings:Ed25519PublicKey"];
            // Actual verification logic would go here
            
            return true;
        }
    }
}
