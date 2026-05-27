using Microsoft.EntityFrameworkCore;
using DocLocker.Core.Models;

namespace DocLocker.API.Data
{
    public class DocLockerDbContext : DbContext
    {
        public DocLockerDbContext(DbContextOptions<DocLockerDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectManager> ProjectManagers => Set<ProjectManager>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

        public DbSet<DocumentRequest> DocumentRequests => Set<DocumentRequest>();
        public DbSet<DocumentRequestStatus> DocumentRequestStatuses => Set<DocumentRequestStatus>();
        public DbSet<DocumentRequestStatusHistory> DocumentRequestStatusHistories => Set<DocumentRequestStatusHistory>();

        public DbSet<Document> Documents => Set<Document>();
        public DbSet<DocumentReview> DocumentReviews => Set<DocumentReview>();
        // Configure entity mappings and seed data.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make email unique for users.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configure composite keys for project managers.
            modelBuilder.Entity<ProjectManager>()
                .HasKey(pm => new { pm.ProjectId, pm.ManagerId });

            // Configure composite keys for project members.
            modelBuilder.Entity<ProjectMember>()
                .HasKey(pm => new { pm.ProjectId, pm.MemberId });

            // Configure admin ownership for projects.
            modelBuilder.Entity<Project>()
                .HasOne(p => p.CreatedByAdmin)
                .WithMany(u => u.CreatedProjects)
                .HasForeignKey(p => p.CreatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure member document request relation.
            modelBuilder.Entity<DocumentRequest>()
                .HasOne(r => r.Member)
                .WithMany(u => u.MemberDocumentRequests)
                .HasForeignKey(r => r.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure manager document request relation.
            modelBuilder.Entity<DocumentRequest>()
                .HasOne(r => r.RequestedByManager)
                .WithMany(u => u.ManagerDocumentRequests)
                .HasForeignKey(r => r.RequestedByManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure manager document review relation.
            modelBuilder.Entity<DocumentReview>()
                .HasOne(r => r.Manager)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure document request relation.
            modelBuilder.Entity<Document>()
                .HasOne(d => d.DocumentRequest)
                .WithMany(r => r.Documents)
                .HasForeignKey(d => d.DocumentRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure uploader relation for documents.
            modelBuilder.Entity<Document>()
                .HasOne(d => d.UploadedByUser)
                .WithMany(u => u.UploadedDocuments)
                .HasForeignKey(d => d.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure document review relation.
            modelBuilder.Entity<DocumentReview>()
                .HasOne(r => r.Document)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure request status relation.
            modelBuilder.Entity<DocumentRequest>()
                .HasOne(r => r.Status)
                .WithMany(s => s.Requests)
                .HasForeignKey(r => r.DocumentRequestStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure request status history relations.
            modelBuilder.Entity<DocumentRequestStatusHistory>()
                .HasOne(h => h.DocumentRequest)
                .WithMany(r => r.StatusHistory)
                .HasForeignKey(h => h.DocumentRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure status in history.
            modelBuilder.Entity<DocumentRequestStatusHistory>()
                .HasOne(h => h.Status)
                .WithMany()
                .HasForeignKey(h => h.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure user who changed status.
            modelBuilder.Entity<DocumentRequestStatusHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index latest document per request.
            modelBuilder.Entity<Document>()
                .HasIndex(d => new { d.DocumentRequestId, d.IsLatest });

            // Seed default roles.
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, Name = "Admin" },
                new Role { RoleId = 2, Name = "Manager" },
                new Role { RoleId = 3, Name = "Member" }
            );

            // Seed request status values.
            modelBuilder.Entity<DocumentRequestStatus>().HasData(
                new DocumentRequestStatus { DocumentRequestStatusId = 1, Name = "Pending" },
                new DocumentRequestStatus { DocumentRequestStatusId = 2, Name = "Submitted" },
                new DocumentRequestStatus { DocumentRequestStatusId = 3, Name = "Approved" },
                new DocumentRequestStatus { DocumentRequestStatusId = 4, Name = "Rejected" },
                new DocumentRequestStatus { DocumentRequestStatusId = 5, Name = "RevisionRequested" }
            );
        }
    }
}