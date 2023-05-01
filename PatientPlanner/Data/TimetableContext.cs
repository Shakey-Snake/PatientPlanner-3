using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PatientPlanner.Models;

namespace PatientPlanner.Data
{
    public class TimetableContext : DbContext
    {
        public TimetableContext(DbContextOptions<TimetableContext> options)
            : base(options)
        {
        }

        public DbSet<PatientPlanner.Models.Device> Devices { get; set; }
        public DbSet<PatientPlanner.Models.Patient> Patients { get; set; }
        public DbSet<PatientPlanner.Models.PatientTask> PatientTasks { get; set; }
        public DbSet<PatientPlanner.Models.SettingsProfile> SettingsProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>().ToTable("Device");
            modelBuilder.Entity<Patient>().ToTable("Patient");
            modelBuilder.Entity<PatientTask>().ToTable("PatientTask");
            modelBuilder.Entity<SettingsProfile>().ToTable("SettingsProfile");
        }

    }
}
