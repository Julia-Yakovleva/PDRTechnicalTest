using Microsoft.EntityFrameworkCore;
using PDR.PatientBooking.Data.Models;

namespace PDR.PatientBooking.Data
{
    public class PatientBookingContext : DbContext
    {
        public PatientBookingContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsCancelled);
        }

        public DbSet<Order> Order { get; set; }
        public DbSet<Patient> Patient { get; set; }
        public DbSet<Doctor> Doctor { get; set; }
        public DbSet<Clinic> Clinic { get; set; }
    }
}
