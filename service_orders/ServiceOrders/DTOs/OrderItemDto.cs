public class OrderItemDto
{
    public string Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}