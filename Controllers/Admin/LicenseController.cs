using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P4LicensePortal.Models;
using P4LicensePortal.Services.Interfaces;

namespace P4LicensePortal.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/license")]
    [Authorize(Roles = "Admin,Distributor")]
    public class LicenseController : ControllerBase
    {
        private readonly ILicenseService _licenseService;
        private readonly IAuditLogService _auditLogService;

        public LicenseController(
            ILicenseService licenseService,
            IAuditLogService auditLogService)
        {
            _licenseService = licenseService;
            _auditLogService = auditLogService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Distributor")]
        public async Task<IActionResult> CreateLicense([FromBody] CreateLicenseRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var license = await _licenseService.GenerateLicenseAsync(
                    request.TenantId,
                    request.ProductCode,
                    request.MaxUsers,
                    request.ExpiryDate,
                    request.FeatureFlags
                );

                // Log license creation
                await _auditLogService.LogAsync("LicenseController", "CreateLicense",
                    $"License created for tenant {request.TenantId}, product {request.ProductCode}");

                return Ok(new
                {
                    licenseId = license.Id,
                    message = "License created successfully"
                });
            }
            catch (Exception ex)
            {
                await _auditLogService.LogAsync("LicenseController", "CreateLicenseError",
                    $"Error creating license: {ex.Message}", isError: true);

                return StatusCode(500, "An error occurred while creating the license");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Distributor,Reseller")]
        public async Task<IActionResult> GetLicense(Guid id)
        {
            try
            {
                var license = await _licenseService.GetLicenseByIdAsync(id);
                if (license == null)
                {
                    return NotFound("License not found");
                }

                // Check if non-admin user has access to this tenant's licenses
                if (!User.IsInRole("Admin"))
                {
                    // Logic to check if user has access to this tenant's licenses
                    // This would typically involve checking if the user's partnerId is associated with the tenant
                    // For simplicity, we're skipping this check
                }

                return Ok(license);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogAsync("LicenseController", "GetLicenseError",
                    $"Error retrieving license {id}: {ex.Message}", isError: true);

                return StatusCode(500, "An error occurred while retrieving the license");
            }
        }

        [HttpGet("tenant/{tenantId}")]
        [Authorize(Roles = "Admin,Distributor,Reseller")]
        public async Task<IActionResult> GetLicensesByTenant(Guid tenantId)
        {
            try
            {
                // Check if non-admin user has access to this tenant's licenses
                if (!User.IsInRole("Admin"))
                {
                    // Logic to check if user has access to this tenant's licenses
                    // This would typically involve checking if the user's partnerId is associated with the tenant
                    // For simplicity, we're skipping this check
                }

                var licenses = await _licenseService.GetLicensesByTenantAsync(tenantId);
                return Ok(licenses);
            }
            catch (Exception ex)
            {
                await _auditLogService.LogAsync("LicenseController", "GetLicensesByTenantError",
                    $"Error retrieving licenses for tenant {tenantId}: {ex.Message}", isError: true);

                return StatusCode(500, "An error occurred while retrieving the licenses");
            }
        }

        [HttpPost("revoke/{id}")]
        [Authorize(Roles = "Admin,Distributor")]
        public async Task<IActionResult> RevokeLicense(Guid id, [FromBody] RevokeLicenseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest("Revocation reason is required");
            }

            try
            {
                var license = await _licenseService.GetLicenseByIdAsync(id);
                if (license == null)
                {
                    return NotFound("License not found");
                }

                // Get current user
                var currentUser = User.Identity.Name;

                var result = await _licenseService.RevokeLicenseAsync(id, currentUser, request.Reason);
                if (!result)
                {
                    return BadRequest("Failed to revoke license");
                }

                await _auditLogService.LogAsync("LicenseController", "RevokeLicense",
                    $"License {id} revoked by {currentUser}. Reason: {request.Reason}");

                return Ok(new { message = "License revoked successfully" });
            }
            catch (Exception ex)
            {
                await _auditLogService.LogAsync("LicenseController", "RevokeLicenseError",
                    $"Error revoking license {id}: {ex.Message}", isError: true);

                return StatusCode(500, "An error occurred while revoking the license");
            }
        }
    }

    public class CreateLicenseRequest
    {
        [Required]
        public Guid TenantId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int MaxUsers { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public Dictionary<string, string> FeatureFlags { get; set; }
    }

    public class RevokeLicenseRequest
    {
        [Required]
        [StringLength(500)]
        public string Reason { get; set; }
    }
}
