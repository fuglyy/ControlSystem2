namespace ServiceOrders.DTOs{
    public class UpdateOrderRequest
    {
        public string Status { get; set; } = null!; // created, in_progress, done, cancelled
    }
}