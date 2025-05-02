using System;
using System.Threading;
using System.Threading.Tasks;

namespace P4LicensePortal.Services.Interfaces
{
    /// <summary>
    /// Service for database backup operations
    /// </summary>
    public interface IBackupService
    {
        /// <summary>
        /// Create a BACPAC backup of the master database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if backup was successful, false otherwise</returns>
        Task<bool> CreateMasterDatabaseBackupAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Schedule automatic backups at specified intervals
        /// </summary>
        /// <param name="intervalHours">Interval in hours between backups</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the background operation</returns>
        Task ScheduleAutomaticBackupsAsync(int intervalHours = 6, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get a list of available backups
        /// </summary>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>Collection of backup metadata</returns>
        Task<IEnumerable<BackupMetadata>> GetAvailableBackupsAsync(int maxResults = 20);
        
        /// <summary>
        /// Get a download URL for a specific backup
        /// </summary>
        /// <param name="backupId">Backup identifier</param>
        /// <param name="expiryMinutes">URL expiry time in minutes</param>
        /// <returns>Temporary URL to download the backup</returns>
        Task<string> GetBackupDownloadUrlAsync(string backupId, int expiryMinutes = 60);
    }
    
    /// <summary>
    /// Metadata for a database backup
    /// </summary>
    public class BackupMetadata
    {
        /// <summary>
        /// Unique identifier for the backup
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Name of the database that was backed up
        /// </summary>
        public string DatabaseName { get; set; }
        
        /// <summary>
        /// Date and time when the backup was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Size of the backup in bytes
        /// </summary>
        public long SizeInBytes { get; set; }
        
        /// <summary>
        /// Status of the backup
        /// </summary>
        public string Status { get; set; }
    }
}
