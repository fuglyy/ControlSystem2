using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceOrders.Data;
using ServiceOrders.Models;
using ServiceOrders.DTOs;
using System.Security.Claims;

namespace ServiceOrders.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Создание заказа ---
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Status = "created",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = model.Items.Select(i => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    Product = i.Product,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            order.TotalAmount = order.Items.Sum(i => i.Price * i.Quantity);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        // --- Получение заказа по ID ---
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            if (order.UserId != userId) return Forbid();

            return Ok(order);
        }

        // --- Список заказов текущего пользователя ---
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(orders);
        }

        // --- Обновление статуса ---
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] UpdateOrderRequest model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            if (order.UserId != userId) return Forbid();

            order.Status = model.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(order);
        }

        // --- Отмена заказа ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            if (order.UserId != userId) return Forbid();

            order.Status = "cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(order);
        }
    }
}
