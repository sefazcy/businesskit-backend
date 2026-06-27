using BusinessKit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BusinessSettingsEntity = BusinessKit.Domain.Entities.BusinessSettings;

namespace BusinessKit.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<BusinessSettingsEntity> BusinessSettings { get; set; } = null!;
    public DbSet<BusinessService> BusinessServices { get; set; } = null!;
    public DbSet<ContactMessage> ContactMessages { get; set; } = null!;
    public DbSet<GalleryItem> GalleryItems { get; set; } = null!;
    public DbSet<BlogPost> BlogPosts { get; set; } = null!;
    public DbSet<StaffMember> StaffMembers { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<StaffWorkingHour> StaffWorkingHours { get; set; } = null!;
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<StockMovement> StockMovements { get; set; } = null!;
    public DbSet<ApartmentUnit> ApartmentUnits { get; set; } = null!;
    public DbSet<Resident> Residents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        StampEntities();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampEntities()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
