using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TicketStore>();
builder.Services.AddCors(options => options.AddPolicy("frontend", policy =>
    policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
        .AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
app.UseCors("frontend");
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/tickets", (TicketStore store) => Results.Ok(store.All()));
app.MapPost("/api/tickets", (CreateTicket request, TicketStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
        return Results.BadRequest(new { error = "Title is required" });
    var result = store.Create(request.Title.Trim());
    return Results.Created($"/api/tickets/{result.Id}", result);
});
app.MapPatch("/api/tickets/{id:guid}/status", (Guid id, ChangeStatus request, TicketStore store) =>
{
    var outcome = store.Change(id, request);
    return outcome switch
    {
        StatusChange.Missing => Results.NotFound(),
        StatusChange.Conflict => Results.Conflict(new { error = "Version mismatch" }),
        StatusChange.Invalid => Results.BadRequest(new { error = "Unsupported status" }),
        _ => Results.Ok(store.Get(id))
    };
});
app.Run();

public record CreateTicket(string Title);
public record ChangeStatus(string Status, int ExpectedVersion);
public record Ticket(Guid Id, string Title, string Status, int Version, DateTimeOffset UpdatedAt);
public enum StatusChange { Success, Missing, Conflict, Invalid }

public sealed class TicketStore
{
    private readonly ConcurrentDictionary<Guid, Ticket> _tickets = new();
    private readonly object _sync = new();
    public IReadOnlyList<Ticket> All() => _tickets.Values.OrderBy(t => t.Title).ToList();
    public Ticket? Get(Guid id) => _tickets.GetValueOrDefault(id);
    public Ticket Create(string title)
    {
        var value = new Ticket(Guid.NewGuid(), title, "open", 0, DateTimeOffset.UtcNow);
        _tickets[value.Id] = value;
        return value;
    }
    public StatusChange Change(Guid id, ChangeStatus request)
    {
        lock (_sync)
        {
            if (!_tickets.TryGetValue(id, out var ticket)) return StatusChange.Missing;
            if (request.ExpectedVersion != ticket.Version) return StatusChange.Conflict;
            if (request.Status is not ("open" or "in_progress" or "done")) return StatusChange.Invalid;
            _tickets[id] = ticket with
            {
                Status = request.Status, Version = ticket.Version + 1,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            return StatusChange.Success;
        }
    }
}
