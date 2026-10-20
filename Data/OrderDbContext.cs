using MauricioAguilar.OrderService.Entities;
using Microsoft.EntityFrameworkCore;


namespace MauricioAguilar.OrderService.Data
{
    public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();

        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OutboxMessage>()
            .ToTable("OutboxMessages");

            modelBuilder.Entity<InboxMessage>(entity =>
            {
                entity.ToTable("InboxMessages");

                entity.HasKey(x => x.MessageId);

                entity.Property(x => x.MessageType)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.CorrelationId)
                    .HasMaxLength(50);

                entity.HasIndex(x => x.ProcessedAtUtc);
            });

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
