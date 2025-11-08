using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceOrders.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!; // Владелец заказа

        public List<OrderItem> Items { get; set; } = new();

        [Required]
        public string Status { get; set; } = "created"; // created, in_progress, done, cancelled

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Product { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [ForeignKey("Order")]
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
