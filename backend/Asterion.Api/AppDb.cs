using Microsoft.EntityFrameworkCore;
namespace Asterion.Api;
public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketAudit> Audits => Set<TicketAudit>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Ticket>(e => {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(160).IsRequired();
            e.Property(x => x.Owner).HasMaxLength(80).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Version).IsRowVersion();
            e.HasIndex(x => x.CreatedAt);
        });
        model.Entity<TicketAudit>(e => {
            e.HasKey(x => x.Id);
            e.Property(x => x.Action).HasMaxLength(100).IsRequired();
            e.Property(x => x.Actor).HasMaxLength(80).IsRequired();
            e.HasOne<Ticket>().WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.TicketId, x.CreatedAt });
        });
    }
}
