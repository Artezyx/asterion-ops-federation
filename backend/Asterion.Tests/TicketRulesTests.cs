using Asterion.Api;
using Xunit;
public sealed class TicketRulesTests
{
    [Theory]
    [InlineData(TicketStatus.Open, TicketStatus.InProgress, true)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Completed, true)]
    [InlineData(TicketStatus.Open, TicketStatus.Completed, false)]
    [InlineData(TicketStatus.Completed, TicketStatus.Open, false)]
    [InlineData(TicketStatus.Open, TicketStatus.Open, false)]
    public void EnforcesWorkflowTransitions(TicketStatus from, TicketStatus to, bool allowed) =>
        Assert.Equal(allowed, TicketRules.IsAllowed(from, to));
}
