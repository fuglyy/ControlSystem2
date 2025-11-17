using ServiceOrders.Models;

public interface IOrderEventsService
{
    void OrderCreated(Order order);
    void OrderStatusUpdated(Order order);
}

public class OrderEventsService : IOrderEventsService
{
    public void OrderCreated(Order order)
    {
        Console.WriteLine($"[EVENT] OrderCreated: Id={order.Id}, User={order.UserId}, Total={order.TotalAmount}");
    }

    public void OrderStatusUpdated(Order order)
    {
        Console.WriteLine($"[EVENT] OrderStatusUpdated: Id={order.Id}, Status={order.Status}");
    }
}
