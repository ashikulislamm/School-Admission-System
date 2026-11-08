using Admission.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Admission.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ClassEntity> Classes => Set<ClassEntity>();

    public DbSet<Hold> Holds => Set<Hold>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClassEntity>(e =>
        {
            e.ToTable("classes", t => t.HasCheckConstraint("ck_classes_total_seats_positive", "total_seats > 0"));
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.TotalSeats).HasColumnName("total_seats");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Hold>(e =>
        {
            e.ToTable("holds", t => t.HasCheckConstraint(
                "ck_holds_status",
                "status IN ('HELD','CONFIRMED','EXPIRED','CANCELLED')"));
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ClassId).HasColumnName("class_id");
            e.Property(x => x.StudentId).HasColumnName("student_id").HasMaxLength(17).IsFixedLength().IsRequired();
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(16).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").ValueGeneratedOnAdd();
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            e.Property(x => x.ConfirmedAt).HasColumnName("confirmed_at");

            e.HasOne(x => x.Class)
                .WithMany(c => c.Holds)
                .HasForeignKey(x => x.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // Backstop for rule 3: one active/confirmed application per student per class.
            e.HasIndex(x => new { x.ClassId, x.StudentId })
                .IsUnique()
                .HasDatabaseName("ux_holds_active")
                .HasFilter("status IN ('HELD','CONFIRMED')");

            e.HasIndex(x => new { x.ClassId, x.Status, x.ExpiresAt }).HasDatabaseName("ix_holds_class_status");
            e.HasIndex(x => new { x.StudentId, x.CreatedAt }).HasDatabaseName("ix_holds_student");
        });
    }
}
