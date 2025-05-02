using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Azure.Management.Sql;
using Microsoft.Azure.Management.Sql.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Azure.Authentication;
using P4LicensePortal.Services.Interfaces;

namespace P4LicensePortal.Services.Implementations
{
    public class BackupService : IBackupService, IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<BackupService> _logger;
        private Timer _timer;
        
        private readonly string _connectionString;
        private readonly string _blobConnectionString;
        private readonly string _containerName;
        private readonly string _databaseName;
        
        // Azure management credentials
        private readonly string _subscriptionId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tenantId;
        private readonly string _resourceGroupName;
        private readonly string _serverName;

        public BackupService(
            IConfiguration configuration,
            IAuditLogService auditLogService,
            ILogger<BackupService> logger)
        {
            _configuration = configuration;
            _auditLogService = auditLogService;
            _logger = logger;
            
            // Load configuration values
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _blobConnectionString = _configuration["Backup:BlobConnectionString"];
            _containerName = _configuration["Backup:ContainerName"];
            _databaseName = _configuration["Backup:DatabaseName"] ?? "P4L_Master";
            
            _subscriptionId = _configuration["Azure:SubscriptionId"];
            _clientId = _configuration["Azure:ClientId"];
            _clientSecret = _configuration["Azure:ClientSecret"];
            _tenantId = _configuration["Azure:TenantId"];
            _resourceGroupName = _configuration["Azure:ResourceGroupName"];
            _serverName = _configuration["Azure:ServerName"];
        }

        public async Task<bool> CreateMasterDatabaseBackupAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Create a unique name for the backup
                var backupId = Guid.NewGuid().ToString();
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"{_databaseName}_{timestamp}_{backupId}.bacpac";
                
                _logger.LogInformation($"Starting backup of {_databaseName} to {backupFileName}");
                
                // Create SqlManagementClient
                var serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(
                    _tenantId, _clientId, _clientSecret);
                
                var sqlClient = new SqlManagementClient(serviceCreds)
                {
                    SubscriptionId = _subscriptionId
                };
                
                // Extract database and server names from connection string
                var builder = new SqlConnectionStringBuilder(_connectionString);
                var serverName = builder.DataSource.Split('.')[0];
                
                // Create export request
                var storageKey = await GetStorageKeyAsync();
                var exportRequest = new ExportDatabaseDefinition
                {
                    StorageKeyType = "StorageAccessKey",
                    StorageKey = storageKey,
                    StorageUri = $"https://{_configuration["Backup:StorageAccount"]}.blob.core.windows.net/{_containerName}/{backupFileName}",
                    AdministratorLogin = builder.UserID,
                    AdministratorLoginPassword = builder.Password,
                    AuthenticationType = "SQL"
                };
                
                // Start export operation
                await sqlClient.Databases.ExportAsync(
                    _resourceGroupName,
                    serverName,
                    _databaseName,
                    exportRequest,
                    cancellationToken);
                
                // Log successful backup
                await _auditLogService.LogAsync("BackupService", "DatabaseBackup", 
                    $"Created backup of {_databaseName} with ID {backupId}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating backup of {_databaseName}");
                await _auditLogService.LogAsync("BackupService", "DatabaseBackup", 
                    $"Backup failed: {ex.Message}", isError: true);
                return false;
            }
        }
        
        private async Task<string> GetStorageKeyAsync()
        {
            // In a real implementation, this would use Azure Management APIs
            // to get the storage account key. For simplicity, we'll use the
            // one from configuration.
            return _configuration["Backup:StorageKey"];
        }

        public async Task ScheduleAutomaticBackupsAsync(int intervalHours = 6, CancellationToken cancellationToken = default)
        {
            // Log schedule start
            _logger.LogInformation($"Scheduling automatic backups every {intervalHours} hours");
            await _auditLogService.LogAsync("BackupService", "ScheduleBackups", 
                $"Scheduled automatic backups every {intervalHours} hours");
            
            // Cancel existing timer if it exists
            _timer?.Change(Timeout.Infinite, 0);
            _timer?.Dispose();
            
            // Create new timer
            _timer = new Timer(
                async _ => await CreateMasterDatabaseBackupAsync(cancellationToken),
                null,
                TimeSpan.Zero, // Start immediately
                TimeSpan.FromHours(intervalHours) // Then run every intervalHours
            );
        }

        public async Task<IEnumerable<BackupMetadata>> GetAvailableBackupsAsync(int maxResults = 20)
        {
            try
            {
                // Create BlobServiceClient
                var blobServiceClient = new BlobServiceClient(_blobConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                
                // List blobs
                var blobs = containerClient.GetBlobsAsync();
                var results = new List<BackupMetadata>();
                
                await foreach (var blob in blobs)
                {
                    // Only include .bacpac files
                    if (!blob.Name.EndsWith(".bacpac")) continue;
                    
                    // Parse backup ID from filename (assuming format: dbname_timestamp_guid.bacpac)
                    var parts = blob.Name.Split('_');
                    if (parts.Length < 3) continue;
                    
                    var backupId = Path.GetFileNameWithoutExtension(parts[2]);
                    
                    results.Add(new BackupMetadata
                    {
                        Id = backupId,
                        DatabaseName = parts[0],
                        CreatedAt = blob.Properties.CreatedOn?.DateTime ?? DateTime.MinValue,
                        SizeInBytes = blob.Properties.ContentLength ?? 0,
                        Status = "Completed"
                    });
                    
                    if (results.Count >= maxResults)
                        break;
                }
                
                return results.OrderByDescending(b => b.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing available backups");
                await _auditLogService.LogAsync("BackupService", "ListBackups", 
                    $"Error listing backups: {ex.Message}", isError: true);
                return Array.Empty<BackupMetadata>();
            }
        }

        public async Task<string> GetBackupDownloadUrlAsync(string backupId, int expiryMinutes = 60)
        {
            try
            {
                // Find the blob with the specified backup ID
                var backups = await GetAvailableBackupsAsync();
                var backup = backups.FirstOrDefault(b => b.Id == backupId);
                if (backup == null)
                    return null;
                
                // Reconstruct the blob name
                string blobName = $"{backup.DatabaseName}_{backup.CreatedAt:yyyyMMdd_HHmmss}_{backup.Id}.bacpac";
                
                // Create BlobServiceClient
                var blobServiceClient = new BlobServiceClient(_blobConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                // Create SAS token with read permission
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerName,
                    BlobName = blobName,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                
                // Generate SAS URI
                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                
                // Log access
                await _auditLogService.LogAsync("BackupService", "BackupDownload", 
                    $"Generated download URL for backup {backupId}, expires in {expiryMinutes} minutes");
                
                return sasUri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating download URL for backup {backupId}");
                await _auditLogService.LogAsync("BackupService", "BackupDownload", 
                    $"Error generating download URL: {ex.Message}", isError: true);
                return null;
            }
        }

        #region IHostedService Implementation
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Schedule automatic backups
            await ScheduleAutomaticBackupsAsync(
                int.Parse(_configuration["Backup:IntervalHours"] ?? "6"), 
                cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            _timer?.Dispose();
            _timer = null;
            
            return Task.CompletedTask;
        }
        
        #endregion
    }
}
