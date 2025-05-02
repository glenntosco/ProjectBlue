using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using P4LicensePortal.Data;
using P4LicensePortal.Models;
using P4LicensePortal.Services.Interfaces;

namespace P4LicensePortal.Services.Implementations
{
    public class LicenseService : ILicenseService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IAuditLogService _auditLogService;
        private readonly IConfiguration _configuration;
        private readonly byte[] _encryptionKey;

        public LicenseService(
            ApplicationDbContext dbContext,
            IAuditLogService auditLogService,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _auditLogService = auditLogService;
            _configuration = configuration;

            // Get encryption key from configuration
            string keyBase64 = _configuration["License:EncryptionKey"];
            _encryptionKey = Convert.FromBase64String(keyBase64);
        }

        public async Task<License> GenerateLicenseAsync(Guid tenantId, string productCode, int maxUsers,
            DateTime expiryDate, Dictionary<string, string> featureFlags = null)
        {
            // Create license data object
            var licenseData = new
            {
                TenantId = tenantId,
                ProductCode = productCode,
                MaxUsers = maxUsers,
                ExpiryDate = expiryDate,
                IssuedDate = DateTime.UtcNow,
                FeatureFlags = featureFlags ?? new Dictionary<string, string>()
            };

            // Serialize license data to JSON
            var licenseJson = JsonSerializer.Serialize(licenseData);

            // Sign the license data using Ed25519
            var signature = SignData(Encoding.UTF8.GetBytes(licenseJson));

            // Encrypt the license data + signature
            var encryptedData = EncryptData(Encoding.UTF8.GetBytes(licenseJson + "." + Convert.ToBase64String(signature)));

            // Create license record
            var license = new License
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ProductCode = productCode,
                MaxUsers = maxUsers,
                IssuedDate = DateTime.UtcNow,
                ExpiryDate = expiryDate,
                EncryptedData = Convert.ToBase64String(encryptedData),
                Status = "Active",
                FeatureFlags = JsonSerializer.Serialize(featureFlags ?? new Dictionary<string, string>())
            };

            // Save to database
            _dbContext.Licenses.Add(license);
            await _dbContext.SaveChangesAsync();

            // Log license generation
            await _auditLogService.LogAsync("LicenseService", "LicenseGeneration",
                $"Generated license for Tenant {tenantId}, Product {productCode}");

            return license;
        }

        public async Task<bool> ValidateLicenseAsync(string licenseData)
        {
            try
            {
                // Decrypt license data
                var encryptedBytes = Convert.FromBase64String(licenseData);
                var decryptedBytes = DecryptData(encryptedBytes);
                var decryptedText = Encoding.UTF8.GetString(decryptedBytes);

                // Split data and signature
                var parts = decryptedText.Split('.');
                if (parts.Length != 2)
                    return false;

                var jsonData = parts[0];
                var signature = Convert.FromBase64String(parts[1]);

                // Verify signature
                var isValid = VerifySignature(Encoding.UTF8.GetBytes(jsonData), signature);
                if (!isValid)
                    return false;

                // Deserialize license data
                var license = JsonSerializer.Deserialize<JsonElement>(jsonData);

                // Check if license is expired
                var expiryDate = license.GetProperty("ExpiryDate").GetDateTime();
                if (DateTime.UtcNow > expiryDate)
                    return false;

                await _auditLogService.LogAsync("LicenseService", "LicenseValidation",
                    $"Validated license for Tenant {license.GetProperty("TenantId").GetString()}");

                return true;
            }
            catch (Exception ex)
            {
                await _auditLogService.LogAsync("LicenseService", "LicenseValidation",
                    $"License validation failed: {ex.Message}", isError: true);
                return false;
            }
        }

        public async Task<License> GetLicenseByIdAsync(Guid licenseId)
        {
            return await _dbContext.Licenses
                .FirstOrDefaultAsync(l => l.Id == licenseId);
        }

        public async Task<IEnumerable<License>> GetLicensesByTenantAsync(Guid tenantId)
        {
            return await _dbContext.Licenses
                .Where(l => l.TenantId == tenantId)
                .OrderByDescending(l => l.IssuedDate)
                .ToListAsync();
        }

        public async Task<bool> RevokeLicenseAsync(Guid licenseId, string revokedBy, string reason)
        {
            var license = await _dbContext.Licenses.FindAsync(licenseId);
            if (license == null)
                return false;

            license.Status = "Revoked";
            license.RevokedDate = DateTime.UtcNow;
            license.RevokedBy = revokedBy;
            license.RevocationReason = reason;

            await _dbContext.SaveChangesAsync();

            await _auditLogService.LogAsync("LicenseService", "LicenseRevocation",
                $"License {licenseId} revoked by {revokedBy}. Reason: {reason}");

            return true;
        }

        #region Cryptographic Operations

        private byte[] SignData(byte[] data)
        {
            // Load the private key from configuration
            string privateKeyBase64 = _configuration["License:Ed25519PrivateKey"];
            var privateKey = Convert.FromBase64String(privateKeyBase64);

            using var algorithm = new Ed25519();
            return algorithm.SignData(data, privateKey);
        }

        private bool VerifySignature(byte[] data, byte[] signature)
        {
            // Load the public key from configuration
            string publicKeyBase64 = _configuration["License:Ed25519PublicKey"];
            var publicKey = Convert.FromBase64String(publicKeyBase64);

            using var algorithm = new Ed25519();
            return algorithm.VerifyData(data, signature, publicKey);
        }

        private byte[] EncryptData(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.Mode = CipherMode.GCM;
            aes.Padding = PaddingMode.PKCS7;

            // Generate a random IV for each encryption
            aes.GenerateIV();
            var iv = aes.IV;

            using var encryptor = aes.CreateEncryptor();
            var encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);

            // Combine IV and encrypted data
            var result = new byte[iv.Length + encryptedData.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encryptedData, 0, result, iv.Length, encryptedData.Length);

            return result;
        }

        private byte[] DecryptData(byte[] encryptedData)
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.Mode = CipherMode.GCM;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV from the encrypted data
            var ivLength = aes.BlockSize / 8;
            var iv = new byte[ivLength];
            Buffer.BlockCopy(encryptedData, 0, iv, 0, ivLength);
            aes.IV = iv;

            // Extract actual encrypted data
            var dataLength = encryptedData.Length - ivLength;
            var data = new byte[dataLength];
            Buffer.BlockCopy(encryptedData, ivLength, data, 0, dataLength);

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(data, 0, data.Length);
        }

        #endregion
    }
}
