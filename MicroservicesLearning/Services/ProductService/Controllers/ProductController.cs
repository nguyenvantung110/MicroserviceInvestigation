using Microsoft.AspNetCore.Mvc;

namespace ProductService.Controllers;

public class Product
{
    public string ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private static readonly List<Product> _products = new()
    {
        new Product { ProductId = "P001", Name = "Laptop", Price = 1000 },
        new Product { ProductId = "P002", Name = "Smartphone", Price = 500 },
        new Product { ProductId = "P003", Name = "Tablet", Price = 300 }
    };

    [HttpGet]
    public IActionResult GetProducts()
    {
        return Ok(_products);
    }

    [HttpPost]
    public IActionResult AddProduct([FromBody] Product product)
    {
        _products.Add(product);
        return Ok(_products);
    }
}