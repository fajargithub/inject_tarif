using InjectServiceWorker.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectServiceWorker
{
    public class ApplicationDbContext : DbContext
    {
        public virtual DbSet<BatchModel> BatchResponses { get; set; }
        public DbSet<ServiceRateModel>? TBL_RATE { get; set; }
        public DbSet<ServiceRateProviderModel>? TBL_RATE_PROVIDER { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BatchModel>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<ServiceRateModel>(entity =>
            {
                entity.ToTable("TBL_RATE"); // Maps to the tbl_bank table
                entity.HasNoKey(); // Specifies that this entity is keyless
            });

            modelBuilder.Entity<ServiceRateProviderModel>(entity =>
            {
                entity.ToTable("TBL_RATE_PROVIDER"); // Maps to the tbl_bank table
                entity.HasNoKey(); // Specifies that this entity is keyless
            });


        }
    }
}
