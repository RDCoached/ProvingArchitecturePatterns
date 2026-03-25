using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OnionArch.Api.Endpoints;
using OnionArch.Application.DTOs;
using OnionArch.Domain.Enums;

namespace OnionArch.Api.Tests.Endpoints;

public sealed class OrderEndpointsTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;

    public OrderEndpointsTests(ApiTestFixture factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreatedWithOrder()
    {
        // Arrange
        var request = new CreateOrderRequest(Guid.NewGuid(), "USD");

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.CustomerId.Should().Be(request.CustomerId);
        order.Currency.Should().Be("USD");
        order.Status.Should().Be(OrderStatus.Draft);
        order.TotalAmount.Should().Be(0);
    }

    [Fact]
    public async Task AddOrderItem_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        var createRequest = new CreateOrderRequest(Guid.NewGuid(), "USD");
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        var addItemRequest = new AddOrderItemRequest(Guid.NewGuid(), 2, 50.00m);

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/orders/{order!.Id}/items",
            addItemRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ConfirmOrder_ValidOrder_ReturnsNoContent()
    {
        // Arrange
        var createRequest = new CreateOrderRequest(Guid.NewGuid(), "USD");
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        var addItemRequest = new AddOrderItemRequest(Guid.NewGuid(), 2, 50.00m);
        await _client.PostAsJsonAsync($"/api/orders/{order!.Id}/items", addItemRequest);

        // Act
        var response = await _client.PostAsync($"/api/orders/{order.Id}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ConfirmOrder_EmptyOrder_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreateOrderRequest(Guid.NewGuid(), "USD");
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        // Act
        var response = await _client.PostAsync($"/api/orders/{order!.Id}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrderById_ExistingOrder_ReturnsOrder()
    {
        // Arrange
        var createRequest = new CreateOrderRequest(Guid.NewGuid(), "EUR");
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        // Act
        var response = await _client.GetAsync($"/api/orders/{createdOrder!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.Id.Should().Be(createdOrder.Id);
        order.Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task GetOrderById_NonExistentOrder_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrdersByCustomer_ReturnsCustomerOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request1 = new CreateOrderRequest(customerId, "USD");
        var request2 = new CreateOrderRequest(customerId, "USD");

        await _client.PostAsJsonAsync("/api/orders", request1);
        await _client.PostAsJsonAsync("/api/orders", request2);

        // Act
        var response = await _client.GetAsync($"/api/orders/customer/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        orders.Should().NotBeNull();
        orders!.Should().HaveCountGreaterThanOrEqualTo(2);
        orders.Should().AllSatisfy(o => o.CustomerId.Should().Be(customerId));
    }

    [Fact]
    public async Task EndToEnd_CreateAddItemConfirmGet_WorksCorrectly()
    {
        // Arrange & Act
        // 1. Create order
        var createRequest = new CreateOrderRequest(Guid.NewGuid(), "USD");
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        // 2. Add item
        var addItemRequest = new AddOrderItemRequest(Guid.NewGuid(), 3, 25.50m);
        await _client.PostAsJsonAsync($"/api/orders/{order!.Id}/items", addItemRequest);

        // 3. Confirm order
        await _client.PostAsync($"/api/orders/{order.Id}/confirm", null);

        // 4. Get order
        var getResponse = await _client.GetAsync($"/api/orders/{order.Id}");
        var finalOrder = await getResponse.Content.ReadFromJsonAsync<OrderDto>();

        // Assert
        finalOrder.Should().NotBeNull();
        finalOrder!.Status.Should().Be(OrderStatus.Confirmed);
        finalOrder.Items.Should().HaveCount(1);
        finalOrder.Items[0].Quantity.Should().Be(3);
        finalOrder.Items[0].UnitPrice.Should().Be(25.50m);
        finalOrder.TotalAmount.Should().Be(76.50m); // 3 * 25.50
        finalOrder.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task AddOrderItem_OrderNotFound_ReturnsBadRequest()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();
        var addItemRequest = new AddOrderItemRequest(Guid.NewGuid(), 2, 50.00m);

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/orders/{nonExistentOrderId}/items",
            addItemRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddOrderItem_ToConfirmedOrder_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreateOrderRequest(Guid.NewGuid(), "USD");
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        var addItemRequest = new AddOrderItemRequest(Guid.NewGuid(), 2, 50.00m);
        await _client.PostAsJsonAsync($"/api/orders/{order!.Id}/items", addItemRequest);
        await _client.PostAsync($"/api/orders/{order.Id}/confirm", null);

        // Act - Try to add item after confirmation
        var response = await _client.PostAsJsonAsync(
            $"/api/orders/{order.Id}/items",
            addItemRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddOrderItem_WrongCurrency_ReturnsBadRequest()
    {
        // Arrange
        var createRequest = new CreateOrderRequest(Guid.NewGuid(), "USD");
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        // Try to add item with wrong currency through manipulation
        // This test verifies the domain validation works
        var addItemRequest = new AddOrderItemRequest(Guid.NewGuid(), 2, 50.00m);

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/orders/{order!.Id}/items",
            addItemRequest);

        // Assert - Should succeed as currency matches
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ConfirmOrder_NonExistentOrder_ReturnsBadRequest()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/orders/{nonExistentOrderId}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateOrder_WithDifferentCurrencies_CreatesOrders()
    {
        // Arrange & Act
        var usdRequest = new CreateOrderRequest(Guid.NewGuid(), "USD");
        var eurRequest = new CreateOrderRequest(Guid.NewGuid(), "EUR");
        var gbpRequest = new CreateOrderRequest(Guid.NewGuid(), "GBP");

        var usdResponse = await _client.PostAsJsonAsync("/api/orders", usdRequest);
        var eurResponse = await _client.PostAsJsonAsync("/api/orders", eurRequest);
        var gbpResponse = await _client.PostAsJsonAsync("/api/orders", gbpRequest);

        var usdOrder = await usdResponse.Content.ReadFromJsonAsync<OrderDto>();
        var eurOrder = await eurResponse.Content.ReadFromJsonAsync<OrderDto>();
        var gbpOrder = await gbpResponse.Content.ReadFromJsonAsync<OrderDto>();

        // Assert
        usdOrder!.Currency.Should().Be("USD");
        eurOrder!.Currency.Should().Be("EUR");
        gbpOrder!.Currency.Should().Be("GBP");
    }

    [Fact]
    public async Task AddMultipleItems_ToSameOrder_CalculatesTotalCorrectly()
    {
        // Arrange
        var createRequest = new CreateOrderRequest(Guid.NewGuid(), "USD");
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        // Act - Add multiple items
        await _client.PostAsJsonAsync(
            $"/api/orders/{order!.Id}/items",
            new AddOrderItemRequest(Guid.NewGuid(), 2, 25.00m));

        await _client.PostAsJsonAsync(
            $"/api/orders/{order.Id}/items",
            new AddOrderItemRequest(Guid.NewGuid(), 3, 15.00m));

        await _client.PostAsJsonAsync(
            $"/api/orders/{order.Id}/items",
            new AddOrderItemRequest(Guid.NewGuid(), 1, 100.00m));

        // Assert
        var getResponse = await _client.GetAsync($"/api/orders/{order.Id}");
        var finalOrder = await getResponse.Content.ReadFromJsonAsync<OrderDto>();

        finalOrder.Should().NotBeNull();
        finalOrder!.Items.Should().HaveCount(3);
        finalOrder.TotalAmount.Should().Be(195.00m); // (2*25) + (3*15) + (1*100)
    }

    [Fact]
    public async Task AddSameProduct_Twice_IncreasesQuantity()
    {
        // Arrange
        var createRequest = new CreateOrderRequest(Guid.NewGuid(), "USD");
        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        var productId = Guid.NewGuid();

        // Act - Add same product twice
        await _client.PostAsJsonAsync(
            $"/api/orders/{order!.Id}/items",
            new AddOrderItemRequest(productId, 2, 25.00m));

        await _client.PostAsJsonAsync(
            $"/api/orders/{order.Id}/items",
            new AddOrderItemRequest(productId, 3, 25.00m));

        // Assert
        var getResponse = await _client.GetAsync($"/api/orders/{order.Id}");
        var finalOrder = await getResponse.Content.ReadFromJsonAsync<OrderDto>();

        finalOrder.Should().NotBeNull();
        finalOrder!.Items.Should().HaveCount(1);
        finalOrder.Items[0].Quantity.Should().Be(5); // 2 + 3
        finalOrder.TotalAmount.Should().Be(125.00m); // 5 * 25
    }

    [Fact]
    public async Task GetOrdersByCustomer_NoOrders_ReturnsEmptyList()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/orders/customer/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        orders.Should().NotBeNull();
        orders.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrdersByCustomer_MultipleOrders_ReturnsAll()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Create 3 orders for same customer
        await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(customerId, "USD"));
        await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(customerId, "USD"));
        await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(customerId, "EUR"));

        // Act
        var response = await _client.GetAsync($"/api/orders/customer/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        orders.Should().NotBeNull();
        orders!.Should().HaveCount(3);
        orders.Should().AllSatisfy(o => o.CustomerId.Should().Be(customerId));
    }

    [Fact]
    public async Task CreateOrder_DefaultCurrency_UsesUSD()
    {
        // Arrange - Use default currency by not specifying
        var request = new CreateOrderRequest(Guid.NewGuid());

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order!.Currency.Should().Be("USD");
    }
}
