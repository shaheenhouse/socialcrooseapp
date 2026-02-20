using Dapper;
using Marketplace.Core.Infrastructure;
using Marketplace.Database.Enums;

namespace Marketplace.Slices.OrderSlice;

public interface IOrderRepository
{
    Task<OrderDto?> GetByIdAsync(Guid id);
    Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetByBuyerAsync(Guid buyerId, int page, int pageSize, string? status = null);
    Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetBySellerAsync(Guid sellerId, int page, int pageSize, string? status = null);
    Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetByStoreAsync(Guid storeId, int page, int pageSize, string? status = null);
    Task<Guid> CreateAsync(CreateOrderDto dto, Guid buyerId);
    Task<bool> UpdateStatusAsync(Guid id, OrderStatus status, string? notes = null);
    Task<bool> CancelAsync(Guid id, Guid cancelledBy, string? reason = null);
    Task<IEnumerable<OrderItemDto>> GetItemsAsync(Guid orderId);
}

public class OrderRepository : IOrderRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public OrderRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT o."Id", o."BuyerId", o."StoreId", o."SellerId",
                   o."OrderNumber", o."Status", o."OrderType",
                   o."Subtotal", o."DiscountAmount", o."DiscountCode",
                   o."TaxAmount", o."ShippingAmount",
                   o."ServiceFee", o."TotalAmount", o."Currency",
                   o."PlatformCommission", o."SellerEarnings",
                   o."ShippingName", o."ShippingAddress",
                   o."ShippingCity", o."ShippingCountry",
                   o."TrackingNumber", o."TrackingUrl",
                   o."ShippedAt", o."DeliveredAt",
                   o."Notes", o."CancelledAt", o."CancellationReason",
                   o."CompletedAt", o."CreatedAt",
                   b."FirstName" || ' ' || b."LastName" as BuyerName, b."AvatarUrl" as BuyerAvatarUrl,
                   sl."FirstName" || ' ' || sl."LastName" as SellerName,
                   st."Name" as StoreName
            FROM orders o
            JOIN users b ON o."BuyerId" = b."Id"
            LEFT JOIN users sl ON o."SellerId" = sl."Id"
            LEFT JOIN stores st ON o."StoreId" = st."Id"
            WHERE o."Id" = @Id AND o."IsDeleted" = false
            """;

        return await connection.QuerySingleOrDefaultAsync<OrderDto>(sql, new { Id = id });
    }

    public async Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetByBuyerAsync(Guid buyerId, int page, int pageSize, string? status = null)
    {
        return await GetOrdersAsync("o.\"BuyerId\" = @UserId", buyerId, page, pageSize, status);
    }

    public async Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetBySellerAsync(Guid sellerId, int page, int pageSize, string? status = null)
    {
        return await GetOrdersAsync("o.\"SellerId\" = @UserId", sellerId, page, pageSize, status);
    }

    public async Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetByStoreAsync(Guid storeId, int page, int pageSize, string? status = null)
    {
        return await GetOrdersAsync("o.\"StoreId\" = @UserId", storeId, page, pageSize, status);
    }

    private async Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetOrdersAsync(string userFilter, Guid userId, int page, int pageSize, string? status)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = $"WHERE {userFilter} AND o.\"IsDeleted\" = false";
        if (!string.IsNullOrEmpty(status))
            whereClause += " AND o.\"Status\" = @Status::integer";

        var countSql = $"SELECT COUNT(*) FROM orders o {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { UserId = userId, Status = status });

        var sql = $"""
            SELECT o."Id", o."OrderNumber", o."Status", o."OrderType",
                   o."TotalAmount", o."Currency", o."CreatedAt",
                   o."ShippedAt", o."DeliveredAt",
                   (SELECT COUNT(*) FROM order_items oi WHERE oi."OrderId" = o."Id" AND oi."IsDeleted" = false) as ItemCount,
                   COALESCE(st."Name", sl."FirstName" || ' ' || sl."LastName") as SellerOrStoreName,
                   b."FirstName" || ' ' || b."LastName" as BuyerName
            FROM orders o
            JOIN users b ON o."BuyerId" = b."Id"
            LEFT JOIN users sl ON o."SellerId" = sl."Id"
            LEFT JOIN stores st ON o."StoreId" = st."Id"
            {whereClause}
            ORDER BY o."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var orders = await connection.QueryAsync<OrderListDto>(sql, new
        {
            UserId = userId,
            Status = status,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });

        return (orders, totalCount);
    }

    public async Task<Guid> CreateAsync(CreateOrderDto dto, Guid buyerId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        const string orderSql = """
            INSERT INTO orders ("Id", "BuyerId", "StoreId", "SellerId", "OrderNumber", "Status", "OrderType",
                               "Subtotal", "DiscountAmount", "TaxAmount", "ShippingAmount", "ServiceFee",
                               "TotalAmount", "Currency", "Notes",
                               "ShippingName", "ShippingAddress", "ShippingCity", "ShippingCountry", "ShippingPostalCode",
                               "IsGift", "CreatedAt", "IsDeleted")
            VALUES (@Id, @BuyerId, @StoreId, @SellerId, @OrderNumber, @Status, @OrderType,
                   @Subtotal, @DiscountAmount, @TaxAmount, @ShippingAmount, @ServiceFee,
                   @TotalAmount, @Currency, @Notes,
                   @ShippingName, @ShippingAddress, @ShippingCity, @ShippingCountry, @ShippingPostalCode,
                   false, NOW(), false)
            RETURNING "Id"
            """;

        var orderId = await connection.ExecuteScalarAsync<Guid>(orderSql, new
        {
            Id = id,
            BuyerId = buyerId,
            dto.StoreId,
            dto.SellerId,
            OrderNumber = orderNumber,
            Status = (int)OrderStatus.Pending,
            OrderType = dto.OrderType ?? "Product",
            dto.Subtotal,
            dto.DiscountAmount,
            dto.TaxAmount,
            dto.ShippingAmount,
            dto.ServiceFee,
            dto.TotalAmount,
            Currency = dto.Currency ?? "USD",
            dto.Notes,
            dto.ShippingName,
            dto.ShippingAddress,
            dto.ShippingCity,
            dto.ShippingCountry,
            dto.ShippingPostalCode
        });

        if (dto.Items != null)
        {
            foreach (var item in dto.Items)
            {
                await connection.ExecuteAsync("""
                    INSERT INTO order_items ("Id", "OrderId", "ProductId", "ServiceId", "ItemType", "Name",
                                           "Quantity", "UnitPrice", "TotalPrice", "Currency", "ImageUrl",
                                           "IsDelivered", "IsRefunded", "CreatedAt", "IsDeleted")
                    VALUES (@Id, @OrderId, @ProductId, @ServiceId, @ItemType, @Name,
                           @Quantity, @UnitPrice, @TotalPrice, @Currency, @ImageUrl,
                           false, false, NOW(), false)
                    """, new
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    item.ProductId,
                    item.ServiceId,
                    ItemType = item.ItemType ?? "Product",
                    item.Name,
                    item.Quantity,
                    item.UnitPrice,
                    TotalPrice = item.UnitPrice * item.Quantity,
                    Currency = dto.Currency ?? "USD",
                    item.ImageUrl
                });
            }
        }

        return orderId;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, OrderStatus status, string? notes = null)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var extraUpdates = status switch
        {
            OrderStatus.Shipped => ", \"ShippedAt\" = NOW()",
            OrderStatus.Delivered => ", \"DeliveredAt\" = NOW()",
            OrderStatus.Completed => ", \"CompletedAt\" = NOW()",
            _ => ""
        };

        var sql = $"""
            UPDATE orders SET "Status" = @Status, "InternalNotes" = COALESCE(@Notes, "InternalNotes"),
                   "UpdatedAt" = NOW(){extraUpdates}
            WHERE "Id" = @Id AND "IsDeleted" = false
            """;

        return await connection.ExecuteAsync(sql, new { Id = id, Status = (int)status, Notes = notes }) > 0;
    }

    public async Task<bool> CancelAsync(Guid id, Guid cancelledBy, string? reason = null)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        const string sql = """
            UPDATE orders SET "Status" = @Status, "CancelledAt" = NOW(), "CancelledBy" = @CancelledBy,
                   "CancellationReason" = @Reason, "UpdatedAt" = NOW()
            WHERE "Id" = @Id AND "IsDeleted" = false AND "Status" IN (0, 1)
            """;

        return await connection.ExecuteAsync(sql, new
        {
            Id = id,
            Status = (int)OrderStatus.Cancelled,
            CancelledBy = cancelledBy,
            Reason = reason
        }) > 0;
    }

    public async Task<IEnumerable<OrderItemDto>> GetItemsAsync(Guid orderId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT oi."Id", oi."ProductId", oi."ServiceId",
                   oi."ItemType", oi."Name", oi."Sku", oi."Quantity",
                   oi."UnitPrice", oi."DiscountAmount",
                   oi."TotalPrice", oi."Currency", oi."ImageUrl",
                   oi."IsDelivered", oi."IsRefunded"
            FROM order_items oi
            WHERE oi."OrderId" = @OrderId AND oi."IsDeleted" = false
            ORDER BY oi."CreatedAt"
            """;

        return await connection.QueryAsync<OrderItemDto>(sql, new { OrderId = orderId });
    }
}

// DTOs
public record OrderDto
{
    public Guid Id { get; init; }
    public Guid BuyerId { get; init; }
    public Guid? StoreId { get; init; }
    public Guid? SellerId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public int Status { get; init; }
    public string OrderType { get; init; } = "Product";
    public decimal Subtotal { get; init; }
    public decimal? DiscountAmount { get; init; }
    public string? DiscountCode { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal ShippingAmount { get; init; }
    public decimal? ServiceFee { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal? PlatformCommission { get; init; }
    public decimal? SellerEarnings { get; init; }
    public string? ShippingName { get; init; }
    public string? ShippingAddress { get; init; }
    public string? ShippingCity { get; init; }
    public string? ShippingCountry { get; init; }
    public string? TrackingNumber { get; init; }
    public string? TrackingUrl { get; init; }
    public DateTime? ShippedAt { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public string? Notes { get; init; }
    public DateTime? CancelledAt { get; init; }
    public string? CancellationReason { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? BuyerName { get; init; }
    public string? BuyerAvatarUrl { get; init; }
    public string? SellerName { get; init; }
    public string? StoreName { get; init; }
}

public record OrderListDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public int Status { get; init; }
    public string OrderType { get; init; } = "Product";
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTime CreatedAt { get; init; }
    public DateTime? ShippedAt { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public int ItemCount { get; init; }
    public string? SellerOrStoreName { get; init; }
    public string? BuyerName { get; init; }
}

public record CreateOrderDto
{
    public Guid? StoreId { get; init; }
    public Guid? SellerId { get; init; }
    public string? OrderType { get; init; } = "Product";
    public decimal Subtotal { get; init; }
    public decimal? DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal ShippingAmount { get; init; }
    public decimal? ServiceFee { get; init; }
    public decimal TotalAmount { get; init; }
    public string? Currency { get; init; } = "USD";
    public string? Notes { get; init; }
    public string? ShippingName { get; init; }
    public string? ShippingAddress { get; init; }
    public string? ShippingCity { get; init; }
    public string? ShippingCountry { get; init; }
    public string? ShippingPostalCode { get; init; }
    public IEnumerable<CreateOrderItemDto>? Items { get; init; }
}

public record CreateOrderItemDto
{
    public Guid? ProductId { get; init; }
    public Guid? ServiceId { get; init; }
    public string? ItemType { get; init; } = "Product";
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; } = 1;
    public decimal UnitPrice { get; init; }
    public string? ImageUrl { get; init; }
}

public record OrderItemDto
{
    public Guid Id { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? ServiceId { get; init; }
    public string ItemType { get; init; } = "Product";
    public string Name { get; init; } = string.Empty;
    public string? Sku { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal? DiscountAmount { get; init; }
    public decimal TotalPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public string? ImageUrl { get; init; }
    public bool IsDelivered { get; init; }
    public bool IsRefunded { get; init; }
}
