using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using P4LicensePortal.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace P4LicensePortal.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(string userId, string role, Guid? tenantId = null, IDictionary<string, bool> featureFlags = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };

            if (tenantId.HasValue)
            {
                claims.Add(new Claim("TenantId", tenantId.Value.ToString()));
            }

            // Add feature flags as claims
            if (featureFlags != null)
            {
                foreach (var flag in featureFlags)
                {
                    claims.Add(new Claim($"Feature.{flag.Key}", flag.Value.ToString()));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:ExpiryInMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Generate token from license
        public string GenerateTokenFromLicense(License license, string userId, string role)
        {
            var featureFlags = new Dictionary<string, bool>
            {
                { "EnableReports", license.EnableReports },
                { "EnableAPI", license.EnableAPI },
                { "AllowBrandingOverrides", license.AllowBrandingOverrides },
                { "IsSandbox", license.IsSandbox }
            };

            return GenerateToken(userId, role, license.TenantId, featureFlags);
        }
    }
}
