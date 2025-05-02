using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace P4LicensePortal.Services.Interfaces
{
    /// <summary>
    /// Service for JWT token generation and validation
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generate a JWT token for the specified user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="role">User role (Admin, Distributor, Reseller, TenantUser)</param>
        /// <param name="featureFlags">Optional feature flags to include in token</param>
        /// <returns>JWT token string</returns>
        Task<string> GenerateToken(string userId, string tenantId, string role, Dictionary<string, string> featureFlags = null);
        
        /// <summary>
        /// Validate a JWT token and extract claims
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>ClaimsPrincipal if valid, null if invalid</returns>
        Task<ClaimsPrincipal> ValidateToken(string token);
        
        /// <summary>
        /// Refresh an existing token
        /// </summary>
        /// <param name="existingToken">Current valid token</param>
        /// <returns>New JWT token with refreshed expiration</returns>
        Task<string> RefreshToken(string existingToken);
    }
}
