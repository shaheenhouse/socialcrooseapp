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
            SELECT o.id, o.buyer_id as BuyerId, o.store_id as StoreId, o.seller_id as SellerId,
                   o.order_number as OrderNumber, o.status, o.order_type as OrderType,
                   o.subtotal, o.discount_amount as DiscountAmount, o.discount_code as DiscountCode,
                   o.tax_amount as TaxAmount, o.shipping_amount as ShippingAmount,
                   o.service_fee as ServiceFee, o.total_amount as TotalAmount, o.currency,
                   o.platform_commission as PlatformCommission, o.seller_earnings as SellerEarnings,
                   o.shipping_name as ShippingName, o.shipping_address as ShippingAddress,
                   o.shipping_city as ShippingCity, o.shipping_country as ShippingCountry,
                   o.tracking_number as TrackingNumber, o.tracking_url as TrackingUrl,
                   o.shipped_at as ShippedAt, o.delivered_at as DeliveredAt,
                   o.notes, o.cancelled_at as CancelledAt, o.cancellation_reason as CancellationReason,
                   o.completed_at as CompletedAt, o.created_at as CreatedAt,
                   b.first_name || ' ' || b.last_name as BuyerName, b.avatar_url as BuyerAvatarUrl,
                   sl.first_name || ' ' || sl.last_name as SellerName,
                   st.name as StoreName
            FROM orders o
            JOIN users b ON o.buyer_id = b.id
            LEFT JOIN users sl ON o.seller_id = sl.id
            LEFT JOIN stores st ON o.store_id = st.id
            WHERE o.id = @Id AND o.is_deleted = false
            """;

        return await connection.QuerySingleOrDefaultAsync<OrderDto>(sql, new { Id = id });
    }

    public async Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetByBuyerAsync(Guid buyerId, int page, int pageSize, string? status = null)
    {
        return await GetOrdersAsync("o.buyer_id = @UserId", buyerId, page, pageSize, status);
    }

    public async Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetBySellerAsync(Guid sellerId, int page, int pageSize, string? status = null)
    {
        return await GetOrdersAsync("o.seller_id = @UserId", sellerId, page, pageSize, status);
    }

    public async Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetByStoreAsync(Guid storeId, int page, int pageSize, string? status = null)
    {
        return await GetOrdersAsync("o.store_id = @UserId", storeId, page, pageSize, status);
    }

    private async Task<(IEnumerable<OrderListDto> Orders, int TotalCount)> GetOrdersAsync(string userFilter, Guid userId, int page, int pageSize, string? status)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = $"WHERE {userFilter} AND o.is_deleted = false";
        if (!string.IsNullOrEmpty(status))
            whereClause += " AND o.status = @Status::integer";

        var countSql = $"SELECT COUNT(*) FROM orders o {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { UserId = userId, Status = status });

        var sql = $"""
            SELECT o.id, o.order_number as OrderNumber, o.status, o.order_type as OrderType,
                   o.total_amount as TotalAmount, o.currency, o.created_at as CreatedAt,
                   o.shipped_at as ShippedAt, o.delivered_at as DeliveredAt,
                   (SELECT COUNT(*) FROM order_items oi WHERE oi.order_id = o.id AND oi.is_deleted = false) as ItemCount,
                   COALESCE(st.name, sl.first_name || ' ' || sl.last_name) as SellerOrStoreName,
                   b.first_name || ' ' || b.last_name as BuyerName
            FROM orders o
            JOIN users b ON o.buyer_id = b.id
            LEFT JOIN users sl ON o.seller_id = sl.id
            LEFT JOIN stores st ON o.store_id = st.id
            {whereClause}
            ORDER BY o.created_at DESC
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
            INSERT INTO orders (id, buyer_id, store_id, seller_id, order_number, status, order_type,
                               subtotal, discount_amount, tax_amount, shipping_amount, service_fee,
                               total_amount, currency, notes,
                               shipping_name, shipping_address, shipping_city, shipping_country, shipping_postal_code,
                               created_at, is_deleted)
            VALUES (@Id, @BuyerId, @StoreId, @SellerId, @OrderNumber, @Status, @OrderType,
                   @Subtotal, @DiscountAmount, @TaxAmount, @ShippingAmount, @ServiceFee,
                   @TotalAmount, @Currency, @Notes,
                   @ShippingName, @ShippingAddress, @ShippingCity, @ShippingCountry, @ShippingPostalCode,
                   NOW(), false)
            RETURNING id
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
                    INSERT INTO order_items (id, order_id, product_id, service_id, item_type, name,
                                           quantity, unit_price, total_price, currency, image_url, created_at, is_deleted)
                    VALUES (@Id, @OrderId, @ProductId, @ServiceId, @ItemType, @Name,
                           @Quantity, @UnitPrice, @TotalPrice, @Currency, @ImageUrl, NOW(), false)
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
            OrderStatus.Shipped => ", shipped_at = NOW()",
            OrderStatus.Delivered => ", delivered_at = NOW()",
            OrderStatus.Completed => ", completed_at = NOW()",
            _ => ""
        };

        var sql = $"""
            UPDATE orders SET status = @Status, internal_notes = COALESCE(@Notes, internal_notes),
                   updated_at = NOW(){extraUpdates}
            WHERE id = @Id AND is_deleted = false
            """;

        return await connection.ExecuteAsync(sql, new { Id = id, Status = (int)status, Notes = notes }) > 0;
    }

    public async Task<bool> CancelAsync(Guid id, Guid cancelledBy, string? reason = null)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        const string sql = """
            UPDATE orders SET status = @Status, cancelled_at = NOW(), cancelled_by = @CancelledBy,
                   cancellation_reason = @Reason, updated_at = NOW()
            WHERE id = @Id AND is_deleted = false AND status IN (0, 1)
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
            SELECT oi.id, oi.product_id as ProductId, oi.service_id as ServiceId,
                   oi.item_type as ItemType, oi.name, oi.sku, oi.quantity,
                   oi.unit_price as UnitPrice, oi.discount_amount as DiscountAmount,
                   oi.total_price as TotalPrice, oi.currency, oi.image_url as ImageUrl,
                   oi.is_delivered as IsDelivered, oi.is_refunded as IsRefunded
            FROM order_items oi
            WHERE oi.order_id = @OrderId AND oi.is_deleted = false
            ORDER BY oi.created_at
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
