using ImageGenerator.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace ImageGenerator.Repository.Identity
{
    public class AppIdentityDbContext : IdentityDbContext<AppUser>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> Options) : base(Options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensure to call base method first

            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(l => l.UserId);
            modelBuilder.Entity<List<string>>().HasNoKey();
        }
    }
}
