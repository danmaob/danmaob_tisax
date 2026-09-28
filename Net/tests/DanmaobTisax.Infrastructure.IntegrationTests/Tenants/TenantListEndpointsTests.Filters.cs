using System.Net;
using DanmaobTisax.Domain.Tenants;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Tenants;

public partial class TenantListEndpointsTests
{
    [Fact]
    public async Task ListTenants_FilteredByStatus_ReturnsOnlyTenantsInThatStatus()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var marker = NewMarker();
        var active = await _factory.CreateTenantAsync(client, marker + "-Active");
        var suspended = await _factory.CreateTenantAsync(client, marker + "-Suspended");
        var deactivated = await _factory.CreateTenantAsync(client, marker + "-Deactivated");
        var suspendResponse = await client.PostAsync("/api/v1/platform/tenants/" + suspended.Id + "/suspend", null);
        Assert.Equal(HttpStatusCode.OK, suspendResponse.StatusCode);
        var deactivateResponse = await client.PostAsync("/api/v1/platform/tenants/" + deactivated.Id + "/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        var suspendedPage = await GetTenantsAsync(client, "name=" + marker + "&status=Suspended");
        var suspendedItem = Assert.Single(suspendedPage.Items);
        Assert.Equal(suspended.Id, suspendedItem.Id);
        Assert.Equal("Suspended", suspendedItem.Status);
        var deactivatedPage = await GetTenantsAsync(client, "name=" + marker + "&status=Deactivated");
        var deactivatedItem = Assert.Single(deactivatedPage.Items);
        Assert.Equal(deactivated.Id, deactivatedItem.Id);
        var activePage = await GetTenantsAsync(client, "name=" + marker + "&status=Active");
        var activeItem = Assert.Single(activePage.Items);
        Assert.Equal(active.Id, activeItem.Id);
    }

    [Fact]
    public async Task ListTenants_FilteredByName_IgnoresCaseAndSurroundingSpaces()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var marker = NewMarker();
        await _factory.CreateTenantAsync(client, marker + "-Alpha");
        await _factory.CreateTenantAsync(client, marker + "-Bravo");
        var search = "  " + marker.ToUpperInvariant() + "-ALPHA  ";
        var page = await GetTenantsAsync(client, "name=" + Uri.EscapeDataString(search));
        var item = Assert.Single(page.Items);
        Assert.Equal(marker + "-Alpha", item.Name);
    }

    [Fact]
    public async Task ListTenants_WithPreexistingTenantsInDifferentStatuses_ReturnsStoredStatuses()
    {
        var marker = NewMarker();
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var activeTenant = new Tenant(marker + "-A-Active");
        var suspendedTenant = new Tenant(marker + "-B-Suspended");
        suspendedTenant.Suspend();
        var deactivatedTenant = new Tenant(marker + "-C-Deactivated");
        deactivatedTenant.Deactivate();
        context.Tenants.Add(activeTenant);
        context.Tenants.Add(suspendedTenant);
        context.Tenants.Add(deactivatedTenant);
        await context.SaveChangesAsync();
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var page = await GetTenantsAsync(client, "name=" + marker);
        Assert.Equal(3, page.TotalCount);
        var statuses = page.Items.Select(t => t.Status).ToList();
        Assert.Equal(new[] { "Active", "Suspended", "Deactivated" }, statuses);
    }

    [Fact]
    public async Task ListTenants_WithUnknownStatus_ReturnsBadRequest()
    {
        var login = await _factory.CreatePlatformAdministratorAndLoginAsync();
        var client = _factory.CreateClientWithToken(login.AccessToken);
        var response = await client.GetAsync("/api/v1/platform/tenants?status=Unknown");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
