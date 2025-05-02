using Microsoft.EntityFrameworkCore;
using P4LicensePortal.Models;
using System;

namespace P4LicensePortal.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<License> Licenses { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<PartnerBranding> PartnerBrandings { get; set; }
        public DbSet<PartnerCertification> PartnerCertifications { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<KycProfile> KycProfiles { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<License>()
                .HasOne(l => l.Tenant)
                .WithMany(t => t.Licenses)
                .HasForeignKey(l => l.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tenant>()
                .HasOne(t => t.Partner)
                .WithMany(p => p.Tenants)
                .HasForeignKey(t => t.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<KycProfile>()
                .HasOne(k => k.Partner
