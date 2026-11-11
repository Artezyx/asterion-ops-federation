using System.Security.Claims;
using Asterion.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class WorkflowSecurityTests
{
    static IAuthorizationService Service()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddWorkflowSecurity(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string,string?> { ["Auth:SigningKey"]=new string('x',48) }).Build());
        return services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
    }

    [Theory]
    [InlineData("tickets.read","tickets.read",true)]
    [InlineData("tickets.read","tickets.write",true)]
    [InlineData("tickets.write","tickets.read",false)]
    [InlineData("tickets.write","tickets.write",true)]
    public async Task AppliesReadWriteScopes(string policy, string scope, bool allowed)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub","test-operator"),new Claim("scope",scope)],"Bearer"));
        Assert.Equal(allowed,(await Service().AuthorizeAsync(principal,policy)).Succeeded);
    }

    [Fact]
    public async Task AnonymousRequestsHaveNoPermissions() =>
        Assert.False((await Service().AuthorizeAsync(new ClaimsPrincipal(),"tickets.read")).Succeeded);

    [Fact]
    public void RequiresStrongSigningSecret() =>
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection()
            .AddWorkflowSecurity(new ConfigurationBuilder().Build()));
}
