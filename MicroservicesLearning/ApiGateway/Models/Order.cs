namespace ApiGateway.Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
