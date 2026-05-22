using Microsoft.EntityFrameworkCore;
using BanquetHall.Models;

namespace BanquetHall.Data;

public class BanquetHallDbContext : DbContext
{
    public BanquetHallDbContext(DbContextOptions<BanquetHallDbContext> options) : base(options) { }

    public DbSet<Guest> Guests { get; set; }
    public DbSet<GuestFunction> GuestFunctions { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<FollowUp> FollowUps { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<FunctionName> FunctionNames { get; set; }
    public DbSet<FunctionHall> FunctionHalls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Guest → Functions
        modelBuilder.Entity<Guest>()
            .HasMany(g => g.Functions)
            .WithOne(f => f.Guest)
            .HasForeignKey(f => f.GuestId);

        // Guest → Payments
        modelBuilder.Entity<Guest>()
            .HasMany(g => g.Payments)
            .WithOne(p => p.Guest)
            .HasForeignKey(p => p.GuestId);

        // Guest → FollowUps
        modelBuilder.Entity<Guest>()
            .HasMany(g => g.FollowUps)
            .WithOne(f => f.Guest)
            .HasForeignKey(f => f.GuestId);

        // GuestFunction → Payments
        modelBuilder.Entity<GuestFunction>()
            .HasMany(gf => gf.Payments)
            .WithOne(p => p.Function)
            .HasForeignKey(p => p.FunctionId);

        // GuestFunction → FunctionName
        modelBuilder.Entity<GuestFunction>()
            .HasOne(gf => gf.FunctionName)
            .WithMany()
            .HasForeignKey(gf => gf.FunctionNameId);

        // GuestFunction → FunctionHall
        modelBuilder.Entity<GuestFunction>()
            .HasOne(gf => gf.FunctionHall)
            .WithMany()
            .HasForeignKey(gf => gf.FunctionHallId);

        // GuestFunction → AssignedManager
        modelBuilder.Entity<GuestFunction>()
            .HasOne(gf => gf.AssignedManager)
            .WithMany()
            .HasForeignKey(gf => gf.AssignedManagerId);

        // User → FollowUps
        modelBuilder.Entity<User>()
            .HasMany(u => u.AssignedFollowUps)
            .WithOne(f => f.Manager)
            .HasForeignKey(f => f.ManagerId);

        // ActivityLog → User
        modelBuilder.Entity<ActivityLog>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId);
    }
}
