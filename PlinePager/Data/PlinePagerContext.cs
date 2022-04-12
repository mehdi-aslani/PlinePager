using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlinePager.Models;
using PlinePager.Models.Users;

namespace PlinePager.Data
{
    public sealed class PlinePagerContext : IdentityDbContext<TblUser>
    {
        private readonly IHostingEnvironment _hostingEnv;

        public PlinePagerContext(DbContextOptions<PlinePagerContext> options,
            IHostingEnvironment hostingEnv)
            : base(options)
        {
            _hostingEnv = hostingEnv ?? throw new ArgumentNullException(nameof(hostingEnv));
            if (!_hostingEnv.IsDevelopment())
                Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            TblArea.OnModelCreating(builder);
            TblAgent.OnModelCreating(builder);
            TblSound.OnModelCreating(builder);
            TblSchedule.OnModelCreating(builder);
            TblAzan.OnModelCreating(builder);
            base.OnModelCreating(builder);
        }

        public DbSet<PlinePager.Models.TblAgent> TblAgents { get; set; }
        public DbSet<PlinePager.Models.TblArea> TblAreas { get; set; }
        public DbSet<TblSound> TblSounds { get; set; }
        public DbSet<PlinePager.Models.TblSchedule> TblSchedules { get; set; }
        public DbSet<PlinePager.Models.TblAzan> TblAzans { get; set; }
    }
}