using P4LicensePortal.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace P4LicensePortal.Services
{
    public class LicenseApiService
    {
        private readonly HttpClient _httpClient;

        public LicenseApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Retrieves all licenses from the API
        /// </summary>
        public async Task<IEnumerable<License>> GetLicensesAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<License>>("api/license");
            }
            catch (Exception)
            {
                // Log the exception
                throw;
            }
        }

        /// <summary>
        /// Gets a specific license by ID
        /// </summary>
        public async Task<License> GetLicenseByIdAsync(Guid licenseId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<License>($"api/license/{licenseId}");
            }
            catch (Exception)
            {
                // Log the exception
                throw;
            }
        }

        /// <summary>
        /// Creates a new license
        /// </summary>
        public async Task<License> CreateLicenseAsync(License license)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/license", license);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<License>();
            }
            catch (Exception)
            {
                // Log the exception
                throw;
            }
        }

        /// <summary>
        /// Revokes (deletes) a license by ID
        /// </summary>
        public async Task RevokeLicenseAsync(Guid licenseId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/license/{licenseId}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                // Log the exception
                throw;
            }
        }
    }
}
