namespace Asterion.Api;
public enum TicketStatus { Open, InProgress, Completed }
public sealed class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Owner { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public byte[] Version { get; set; } = [];
}
public sealed class TicketAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TicketId { get; set; }
    public required string Action { get; set; }
    public required string Actor { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
public sealed record CreateTicket(string Title, string Owner);
public sealed record ChangeStatus(TicketStatus Status, string Actor, string Version);
public static class TicketRules
{
    public static bool IsAllowed(TicketStatus from, TicketStatus to) =>
        (from, to) is (TicketStatus.Open, TicketStatus.InProgress)
        or (TicketStatus.InProgress, TicketStatus.Completed);
}
