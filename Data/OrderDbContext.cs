using MauricioAguilar.OrderService.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;


namespace MauricioAguilar.Product.Service.Data
{
    public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();

        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OutboxMessage>()
            .ToTable("OutboxMessages");

            modelBuilder.Entity<Order>()
                .HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId);

            modelBuilder.Entity<Order>()
                .OwnsOne(x => x.ShippingAddress);

            modelBuilder.Entity<Order>()
                .OwnsOne(x => x.BillingAddress);

            modelBuilder.HasDefaultSchema("ord");
        }
    }
}
