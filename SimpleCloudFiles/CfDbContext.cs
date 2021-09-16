using SimpleCloudFiles.Models;
using Microsoft.EntityFrameworkCore;
using System;
using SimpleCloudFiles.Utils;

namespace SimpleCloudFiles
{
    public class CfDbContext: DbContext
    {
        public CfDbContext(DbContextOptions<CfDbContext> options) : base(options) { }

        public DbSet<CfFile> CfFiles { get; set; }
        public DbSet<Dir> Dirs { get; set; }
        public DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var accountId = Guid.NewGuid().ToString("N");
            modelBuilder.Entity<Account>().HasData(new Account
            {
                Id = accountId,
                Password = Md5Util.GetMD5("123456"),
                UserName = "admin",
                CreateTime = DateTime.Now,
            });

            modelBuilder.Entity<Dir>().HasData(new Dir
            {
                Id = Guid.NewGuid().ToString("N"),
                AccountId = accountId,
                CreateTime = DateTime.Now,
                DirId = "",
                Name = "首页"
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
