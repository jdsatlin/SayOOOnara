using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SayOOOnara.Persistance
{
	public class OooContext : DbContext
	{
		public DbSet<User> Users { get; set; }

		public DbSet<OooPeriod> Periods { get; set;}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Data Source=SayOOOnara.db");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
		}

	}
}
