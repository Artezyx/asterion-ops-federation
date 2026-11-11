using System.Text.Json.Serialization;
using Asterion.Api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(opts => opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddWorkflowSecurity(builder.Configuration);
builder.Services.AddCors(opts => opts.AddPolicy("local-ui", policy => policy
    .WithOrigins("http://localhost:4200", "http://localhost:4201")
    .AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddDbContext<AppDb>(opts => opts.UseSqlServer(
    builder.Configuration.GetConnectionString("Main") ?? throw new InvalidOperationException("ConnectionStrings:Main missing")));
var app = builder.Build();
app.UseExceptionHandler();
app.UseCors("local-ui");
app.UseAuthentication();
app.UseAuthorization();
// Only safe for this disposable local demo. Replace with EF migrations in production.
using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<AppDb>().Database.EnsureCreatedAsync();
app.MapGet("/health", () => Results.Ok(new { status = "ready" }));
var api = app.MapGroup("/api/v1").RequireAuthorization();
api.MapGet("/tickets", async (AppDb db, int? page, int? pageSize, CancellationToken ct) => {
    var p = Math.Clamp(page ?? 1, 1, 100000);
    var s = Math.Clamp(pageSize ?? 20, 1, 100);
    var query = db.Tickets.AsNoTracking();
    var total = await query.CountAsync(ct);
    var items = await query.OrderByDescending(t => t.CreatedAt).Skip((p-1)*s).Take(s)
        .Select(t => new { t.Id, t.Title, t.Owner, t.Status, t.CreatedAt, Version = Convert.ToBase64String(t.Version) })
        .ToListAsync(ct);
    return Results.Ok(new { items, total, page = p, pageSize = s });
}).RequireAuthorization("tickets.read");
api.MapPost("/tickets", async (CreateTicket input, AppDb db, System.Security.Claims.ClaimsPrincipal principal, CancellationToken ct) => {
    if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Length > 160 ||
        string.IsNullOrWhiteSpace(input.Owner) || input.Owner.Length > 80)
        return Results.ValidationProblem(new Dictionary<string,string[]> { ["ticket"] = ["Valid title and owner required."] });
    var actor = principal.FindFirst("sub")?.Value;
    if (string.IsNullOrWhiteSpace(actor) || actor.Length > 80) return Results.Unauthorized();
    var ticket = new Ticket { Title = input.Title.Trim(), Owner = input.Owner.Trim() };
    db.Tickets.Add(ticket);
    db.Audits.Add(new TicketAudit { TicketId = ticket.Id, Action = "Created", Actor = actor });
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/v1/tickets/{ticket.Id}", new {ticket.Id, ticket.Title, ticket.Owner, ticket.Status, Version = Convert.ToBase64String(ticket.Version)});
}).RequireAuthorization("tickets.write");
api.MapPatch("/tickets/{id:guid}/status", async (Guid id, ChangeStatus input, AppDb db, System.Security.Claims.ClaimsPrincipal principal, CancellationToken ct) => {
    var ticket = await db.Tickets.FindAsync([id], ct);
    if (ticket is null) return Results.NotFound();
    var actor = principal.FindFirst("sub")?.Value;
    if (string.IsNullOrWhiteSpace(actor) || actor.Length > 80) return Results.Unauthorized();
    byte[] version;
    try { version = Convert.FromBase64String(input.Version); }
    catch (FormatException) { return Results.BadRequest(new {error = "Invalid version token."}); }
    if (!version.SequenceEqual(ticket.Version)) return Results.Conflict(new { error = "Ticket was modified; reload it." });
    if (!TicketRules.IsAllowed(ticket.Status, input.Status))
        return Results.Conflict(new { error = "Invalid status transition." });
    ticket.Status = input.Status;
    db.Audits.Add(new TicketAudit { TicketId = id, Action = $"Status:{input.Status}", Actor = actor });
    try { await db.SaveChangesAsync(ct); }
    catch (DbUpdateConcurrencyException) { return Results.Conflict(new {error="Concurrent update"}); }
    return Results.Ok(new { ticket.Id, ticket.Status, Version = Convert.ToBase64String(ticket.Version) });
}).RequireAuthorization("tickets.write");
api.MapGet("/tickets/{id:guid}/history", async (Guid id, AppDb db, CancellationToken ct) => {
    if (!await db.Tickets.AnyAsync(t => t.Id == id, ct)) return Results.NotFound();
    return Results.Ok(await db.Audits.AsNoTracking().Where(a => a.TicketId == id)
        .OrderBy(a => a.CreatedAt).ToListAsync(ct));
}).RequireAuthorization("tickets.read");
app.Run();
public partial class Program { }
