using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using P4LicensePortal.Services.Interfaces;

namespace P4LicensePortal.Services.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuditLogService _auditLogService;
        private readonly string _jwtKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        public JwtService(
            IConfiguration configuration, 
            IAuditLogService auditLogService)
        {
            _configuration = configuration;
            _auditLogService = auditLogService;
            
            // Load settings from configuration
            _jwtKey = _configuration["JWT:Key"];
            _issuer = _configuration["JWT:Issuer"];
            _audience = _configuration["JWT:Audience"];
            _expiryMinutes = int.Parse(_configuration["JWT:ExpiryMinutes"] ?? "60");
            
            if (string.IsNullOrEmpty(_jwtKey))
                throw new ArgumentNullException(nameof(_jwtKey), "JWT Key is not configured");
        }

        public async Task<string> GenerateToken(string userId, string tenantId, string role, Dictionary<string, string> featureFlags = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);
            
            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("TenantId", tenantId),
                new Claim(ClaimTypes.Role, role)
            };
            
            // Add feature flags as claims
            if (featureFlags != null)
            {
                foreach (var flag in featureFlags)
                {
                    claims.Add(new Claim($"Feature:{flag.Key}", flag.Value));
                }
            }
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            // Audit log token creation
            await _auditLogService.LogAsync("JwtService", "TokenGeneration", 
                $"Generated token for User {userId}, Tenant {tenantId}, Role {role}");
            
            return tokenString;
        }

        public async Task<ClaimsPrincipal> ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtKey);
                
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (Exception ex)
            {
                await _auditLogService.LogAsync("JwtService", "TokenValidation", 
                    $"Token validation failed: {ex.Message}", isError: true);
                return null;
            }
        }

        public async Task<string> RefreshToken(string existingToken)
        {
            var principal = await ValidateToken(existingToken);
            if (principal == null)
                return null;
                
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var tenantId = principal.FindFirstValue("TenantId");
            var role = principal.FindFirstValue(ClaimTypes.Role);
            
            // Extract feature flags
            var featureFlags = new Dictionary<string, string>();
            var featureClaims = principal.Claims.Where(c => c.Type.StartsWith("Feature:"));
            
            foreach (var claim in featureClaims)
            {
                var flagName = claim.Type.Substring(8); // Remove "Feature:" prefix
                featureFlags.Add(flagName, claim.Value);
            }
            
            // Generate new token
            return await GenerateToken(userId, tenantId, role, featureFlags);
        }
    }
}
