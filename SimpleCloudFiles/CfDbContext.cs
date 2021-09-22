using Microsoft.EntityFrameworkCore;
using SimpleCloudFiles.Models;

namespace SimpleCloudFiles
{
	public class CfDbContext: DbContext
    {
        public CfDbContext(DbContextOptions<CfDbContext> options) : base(options) { }

        public DbSet<CfFile> CfFiles { get; set; }
        public DbSet<Dir> Dirs { get; set; }
        public DbSet<Account> Accounts { get; set; }
	}
}
