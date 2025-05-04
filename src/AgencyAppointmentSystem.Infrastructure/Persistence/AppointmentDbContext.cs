using AgencyAppointmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Infrastructure.Persistence
{
    public class AppointmentDbContext : DbContext
    {
        public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Token> Tokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            });

            // Configure Appointment entity
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AppointmentDate).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
                entity.Property(e => e.Status).IsRequired();

                // Configure relationships
                entity.HasOne(a => a.Customer)
                      .WithMany(c => c.Appointments)
                      .HasForeignKey(a => a.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Token)
                      .WithOne(t => t.Appointment)
                      .HasForeignKey<Appointment>(a => a.TokenId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Token entity
            modelBuilder.Entity<Token>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokenNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.IssueDate).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
