using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlinePager.Models;
using PlinePager.Models.Users;

namespace PlinePager.Data
{
    public class PlinePagerContext : IdentityDbContext<TblUser>
    {
        public PlinePagerContext (DbContextOptions<PlinePagerContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            TblArea.OnModelCreating(builder);
            TblAgent.OnModelCreating(builder);
            base.OnModelCreating(builder);
        }

        public DbSet<PlinePager.Models.TblAgent> tblAgents { get; set; }

        public DbSet<PlinePager.Models.TblArea> TblAreas { get; set; }
    }
}
