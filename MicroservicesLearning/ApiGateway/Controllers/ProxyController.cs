using ApiGateway.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using ApiGateway.Services;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProxyController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly RabbitMqPublisher _publisher;

    public ProxyController(RabbitMqPublisher publisher, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _publisher = publisher;
    }

    [HttpGet("service-products")]
    public async Task<IActionResult> GetFromServiceA()
    {
        var response = await _httpClient.GetStringAsync("http://localhost:5236/product");
        return Ok(response);
    }

    [HttpGet("service-order")]
    public async Task<IActionResult> GetFromServiceB()
    {
        var response = await _httpClient.GetStringAsync("http://localhost:5090/order");
        return Ok(response);
    }

    // Create order by Order Service
    [HttpPost("service-orders")]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(order),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("http://localhost:5090/order", content);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            return Ok(responseData);
        }
        else
        {
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }

    [HttpPost("create-order")]
    public IActionResult CreateOrder1([FromBody] Order order)
    {
        _publisher.PublishOrder(order);
        return Ok("Order is processing");
    }
}