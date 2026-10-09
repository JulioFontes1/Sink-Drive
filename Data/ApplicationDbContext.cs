namespace SinkDrive.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SinkDrive.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}
    public DbSet<GoogleAccount> GoogleAccounts => Set<GoogleAccount>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<GoogleAccount>(entity =>
        {
            entity.HasIndex(account => account.GoogleId).IsUnique();
            entity.HasOne(account => account.User).WithMany().HasForeignKey(account => account.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(account => account.GoogleId).IsRequired();
            entity.Property(account => account.Email).IsRequired();
        });
    }
}