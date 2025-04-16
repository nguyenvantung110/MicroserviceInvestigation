using Microsoft.AspNetCore.Mvc;

namespace LB_Product.Controllers
{
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
        new Product { ProductId = "LB_P001", Name = "LB_Laptop", Price = 1000 },
        new Product { ProductId = "LB_P002", Name = "LB_Smartphone", Price = 500 },
        new Product { ProductId = "LB_P003", Name = "LB_Tablet", Price = 300 }
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
}
