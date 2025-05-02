using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P4LicensePortal.Models;
using P4LicensePortal.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeBase.Controllers
{
    [ApiController]
    [Route("api/license")]
    [Authorize]
    public class LicenseController : ControllerBase
    {
        private readonly ILicenseService _licenseService;

        public LicenseController(ILicenseService licenseService)
        {
            _licenseService = licenseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<License>>> GetAllLicenses()
        {
            // This implementation assumes we need all licenses
            // In a real-world scenario, you might want to filter by tenant or user permissions
            var licenses = await _licenseService.GetLicensesByPartnerIdAsync(Guid.Empty);
            return Ok(licenses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<License>> GetLicenseById(Guid id)
        {
            var license = await _licenseService.GetLicenseByIdAsync(id);
            
            if (license == null)
            {
                return NotFound();
            }

            return Ok(license);
        }

        [HttpPost]
        public async Task<ActionResult<License>> CreateLicense([FromBody] License license)
        {
            if (license == null)
            {
                return BadRequest();
            }

            // Generate a new license with the provided details
            var createdLicense = await _licenseService.GenerateLicenseAsync(
                license.TenantId, 
                license.ProductCode, 
                license.MaxUsers, 
                license.ExpiryDate, 
                license.EnableReports, 
                license.EnableAPI, 
                license.AllowBrandingOverrides, 
                license.IsSandbox, 
                license.CreatedBy);

            return CreatedAtAction(nameof(GetLicenseById), new { id = createdLicense.Id }, createdLicense);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLicense(Guid id)
        {
            try
            {
                // Note: This actually revokes the license rather than deleting it
                // This is typically better for audit purposes
                string userId = User.Identity.Name; // Or extract from claims
                await _licenseService.RevokeLicenseAsync(id, userId);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}