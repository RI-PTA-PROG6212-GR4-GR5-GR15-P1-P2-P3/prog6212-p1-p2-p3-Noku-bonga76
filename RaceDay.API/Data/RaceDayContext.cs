using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayContext : DbContext
    {
        public RaceDayContext(DbContextOptions<RaceDayContext> options)
      : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();
        public DbSet<EventImage> EventImages => Set<EventImage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserID);
                entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.Email).HasMaxLength(100).IsRequired();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(u => u.Role).HasMaxLength(20).IsRequired();
                entity.Property(u => u.Phone).HasMaxLength(20);
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.EventDate).HasColumnType("date");
                entity.Property(e => e.Location).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Distance).HasPrecision(6, 2);
                entity.Property(e => e.EventType).HasMaxLength(50).IsRequired();

                entity.HasOne(e => e.Organiser)
                    .WithMany(u => u.OrganisedEvents)
                    .HasForeignKey(e => e.OrganiserID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.CategoryName).HasMaxLength(100).IsRequired();
                entity.Property(c => c.Description).HasMaxLength(255);

                entity.HasOne(c => c.Event)
                    .WithMany(e => e.Categories)
                    .HasForeignKey(c => c.EventID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.Property(e => e.EnrolmentDate)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Confirmed")
                    .IsRequired();

                entity.HasOne(e => e.Participant)
                    .WithMany(u => u.Enrolments)
                    .HasForeignKey(e => e.ParticipantID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Event)
                    .WithMany(e => e.Enrolments)
                    .HasForeignKey(e => e.EventID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Enrolments)
                    .HasForeignKey(e => e.CategoryID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Result>(entity =>
            {
                entity.Property(r => r.FinishTime).HasColumnType("time");
                entity.HasIndex(r => r.EnrolmentID).IsUnique();

                entity.HasOne(r => r.Enrolment)
                    .WithOne(e => e.Result)
                    .HasForeignKey<Result>(r => r.EnrolmentID)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<EventImage>(entity =>
            {
                // Explicitly define the primary key
                entity.HasKey(i => i.ImageID);

                entity.Property(i => i.ImageURL)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(i => i.UploadedDate)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(i => i.Event)
                    .WithMany(e => e.EventImages)
                    .HasForeignKey(i => i.EventID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

       