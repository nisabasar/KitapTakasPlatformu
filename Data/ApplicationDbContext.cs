using KitapTakasPlatformu.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KitapTakasPlatformu.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<ExchangeRequest> ExchangeRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ExchangeRequest>()
                .HasOne(e => e.Requester)
                .WithMany()
                .HasForeignKey(e => e.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExchangeRequest>()
                .HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExchangeRequest>()
                .HasOne(e => e.RequestedBook)
                .WithMany()
                .HasForeignKey(e => e.RequestedBookId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExchangeRequest>()
                .HasOne(e => e.OfferedBook)
                .WithMany()
                .HasForeignKey(e => e.OfferedBookId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}