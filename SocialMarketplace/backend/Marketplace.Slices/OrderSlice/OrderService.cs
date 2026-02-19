using Microsoft.Extensions.Logging;
using Marketplace.Core.Infrastructure;
using Marketplace.Database.Enums;

namespace Marketplace.Slices.OrderSlice;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(Guid id, Guid userId);
    Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetMyOrdersAsync(Guid userId, int page, int pageSize, string? status = null);
    Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetMySalesAsync(Guid userId, int page, int pageSize, string? status = null);
    Task<Guid> CreateAsync(CreateOrderDto dto, Guid buyerId);
    Task<bool> UpdateStatusAsync(Guid id, Guid userId, OrderStatus status, string? notes = null);
    Task<bool> CancelAsync(Guid id, Guid userId, string? reason = null);
    Task<IEnumerable<OrderItemDto>> GetItemsAsync(Guid orderId, Guid userId);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IJobQueue _jobQueue;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository repository, IJobQueue jobQueue, ILogger<OrderService> logger)
    {
        _repository = repository;
        _jobQueue = jobQueue;
        _logger = logger;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null) return null;
        if (order.BuyerId != userId && order.SellerId != userId)
            return null;
        return order;
    }

    public async Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetMyOrdersAsync(Guid userId, int page, int pageSize, string? status = null)
    {
        return await _repository.GetByBuyerAsync(userId, page, pageSize, status);
    }

    public async Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetMySalesAsync(Guid userId, int page, int pageSize, string? status = null)
    {
        return await _repository.GetBySellerAsync(userId, page, pageSize, status);
    }

    public async Task<Guid> CreateAsync(CreateOrderDto dto, Guid buyerId)
    {
        var orderId = await _repository.CreateAsync(dto, buyerId);

        await _jobQueue.EnqueueAsync("notifications", new Dictionary<string, string>
        {
            ["Type"] = "new_order",
            ["OrderId"] = orderId.ToString(),
            ["BuyerId"] = buyerId.ToString(),
            ["SellerId"] = dto.SellerId?.ToString() ?? ""
        });

        _logger.LogInformation("Order created: {OrderId} by buyer {BuyerId}", orderId, buyerId);
        return orderId;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, Guid userId, OrderStatus status, string? notes = null)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null) return false;
        if (order.SellerId != userId && order.BuyerId != userId) return false;

        var result = await _repository.UpdateStatusAsync(id, status, notes);
        if (result)
        {
            await _jobQueue.EnqueueAsync("notifications", new Dictionary<string, string>
            {
                ["Type"] = "order_status_changed",
                ["OrderId"] = id.ToString(),
                ["NewStatus"] = status.ToString()
            });
        }
        return result;
    }

    public async Task<bool> CancelAsync(Guid id, Guid userId, string? reason = null)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null || order.BuyerId != userId) return false;
        if (order.Status > (int)OrderStatus.Confirmed) return false;

        return await _repository.CancelAsync(id, userId, reason);
    }

    public async Task<IEnumerable<OrderItemDto>> GetItemsAsync(Guid orderId, Guid userId)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order == null || (order.BuyerId != userId && order.SellerId != userId))
            return [];

        return await _repository.GetItemsAsync(orderId);
    }
}
