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
        public DbSet<ServiceRateModel>? TBL_RATE { get; set; }
        public DbSet<ServiceRateProviderModel>? TBL_RATE_PROVIDER { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    }
}
