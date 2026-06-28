using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>
/// End-to-end tests driving the Sales endpoints over HTTP against a real database, covering
/// the parts only the running pipeline exercises: JWT authorization, request validation, the
/// exception-to-status-code middleware and the full object mapping.
/// </summary>
[Trait("Category", "Functional")]
public sealed class SalesFunctionalTests : IClassFixture<FunctionalApiFactory>
{
    private readonly FunctionalApiFactory _factory;

    public SalesFunctionalTests(FunctionalApiFactory factory) => _factory = factory;

    [SkippableFact]
    public async Task CreateSale_AppliesQuantityDiscounts_AndReturnsCreated()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/sales", BuildSalePayload());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var data = await ReadDataAsync(response);
        data["totalAmount"]!.GetValue<decimal>().Should().Be(550m);
        var items = data["items"]!.AsArray();
        items[0]!["total"]!.GetValue<decimal>().Should().Be(450m);
        items[1]!["total"]!.GetValue<decimal>().Should().Be(100m);
    }

    [SkippableFact]
    public async Task GetSale_ReturnsThePersistedSale()
    {
        var client = await CreateAuthenticatedClientAsync();
        var saleId = await CreateSaleAsync(client);

        var response = await client.GetAsync($"/api/sales/{saleId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await ReadDataAsync(response);
        data["id"]!.GetValue<Guid>().Should().Be(saleId);
        data["totalAmount"]!.GetValue<decimal>().Should().Be(550m);
    }

    [SkippableFact]
    public async Task CancelSaleItem_RecalculatesTotal_AndMarksItemCancelled()
    {
        var client = await CreateAuthenticatedClientAsync();
        var created = await ReadDataAsync(await client.PostAsJsonAsync("/api/sales", BuildSalePayload()));
        var saleId = created["id"]!.GetValue<Guid>();
        var itemId = created["items"]!.AsArray()[1]!["id"]!.GetValue<Guid>();

        var response = await client.PostAsync($"/api/sales/{saleId}/items/{itemId}/cancel", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await ReadDataAsync(response);
        data["totalAmount"]!.GetValue<decimal>().Should().Be(450m);
        data["items"]!.AsArray()
            .First(i => i!["id"]!.GetValue<Guid>() == itemId)!["isCancelled"]!.GetValue<bool>()
            .Should().BeTrue();
    }

    [SkippableFact]
    public async Task CreateSale_WithQuantityAboveLimit_ReturnsBadRequest()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = BuildSalePayload(quantity: 21);

        var response = await client.PostAsJsonAsync("/api/sales", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [SkippableFact]
    public async Task GetSale_ThatDoesNotExist_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/sales/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [SkippableFact]
    public async Task Sales_WithoutToken_ReturnUnauthorized()
    {
        Skip.IfNot(_factory.DockerAvailable, "Docker is not available; skipping functional tests.");

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/sales");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<Guid> CreateSaleAsync(HttpClient client)
    {
        var data = await ReadDataAsync(await client.PostAsJsonAsync("/api/sales", BuildSalePayload()));
        return data["id"]!.GetValue<Guid>();
    }

    private static object BuildSalePayload(int quantity = 5) => new
    {
        saleNumber = $"FUNC-{Guid.NewGuid():N}",
        saleDate = DateTime.UtcNow,
        customerId = Guid.NewGuid(),
        customerName = "Customer",
        branchId = Guid.NewGuid(),
        branchName = "Branch",
        items = new[]
        {
            new { productId = Guid.NewGuid(), productName = "P1", quantity, unitPrice = 100m },
            new { productId = Guid.NewGuid(), productName = "P2", quantity = 2, unitPrice = 50m }
        }
    };

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        Skip.IfNot(_factory.DockerAvailable, "Docker is not available; skipping functional tests.");

        var client = _factory.CreateClient();
        var email = $"user_{Guid.NewGuid():N}@test.com";
        const string password = "Passw0rd!";

        var registration = await client.PostAsJsonAsync("/api/users", new
        {
            username = "FunctionalUser",
            email,
            phone = "+5511987654321",
            password
        });
        registration.StatusCode.Should().Be(HttpStatusCode.Created);

        var authentication = await client.PostAsJsonAsync("/api/auth", new { email, password });
        authentication.EnsureSuccessStatusCode();

        var token = (await ReadDataAsync(authentication))["token"]!.GetValue<string>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<JsonNode> ReadDataAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonNode>();
        return body!["data"]!;
    }
}
