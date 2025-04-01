namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Queries;

// 实现 IOrderQueries 接口，负责处理订单相关的查询操作
public class OrderQueries : IOrderQueries
{
    // 数据库连接字符串
    private string _connectionString = string.Empty;

    // 构造函数，必须传入有效的数据库连接字符串，否则抛出异常
    public OrderQueries(string constr)
    {
        // 如果传入的连接字符串为空或仅为空白字符串，则抛出 ArgumentNullException 异常
        _connectionString = !string.IsNullOrWhiteSpace(constr)
            ? constr
            : throw new ArgumentNullException(nameof(constr));
    }

    // 根据订单ID获取订单详情
    public async Task<Order> GetOrderAsync(int id)
    {
        // 使用数据库连接进行查询
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        // 执行 SQL 查询，查询订单基本信息、地址、状态以及订单项信息
        var result = await connection.QueryAsync<dynamic>(
            @"select o.[Id] as ordernumber, o.OrderDate as date, o.Description as description,
                         o.Address_City as city, o.Address_Country as country, o.Address_State as state, o.Address_Street as street, o.Address_ZipCode as zipcode,
                         os.Name as status, 
                         oi.ProductName as productname, oi.Units as units, oi.UnitPrice as unitprice, oi.PictureUrl as pictureurl
                  FROM ordering.Orders o
                  LEFT JOIN ordering.Orderitems oi ON o.Id = oi.orderid 
                  LEFT JOIN ordering.orderstatus os on o.OrderStatusId = os.Id
                  WHERE o.Id=@id",
            new { id }
        );

        // 若查询结果为空，则抛出 KeyNotFoundException 异常
        if (result.AsList().Count == 0)
            throw new KeyNotFoundException();

        // 调用 MapOrderItems 方法将查询结果映射成 Order 对象
        return MapOrderItems(result);
    }

    // 根据用户ID获取该用户的订单概览列表
    public async Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(Guid userId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        // 执行 SQL 查询，查询订单编号、日期、状态及订单总金额
        return await connection.QueryAsync<OrderSummary>(
            @"SELECT o.[Id] as ordernumber, o.[OrderDate] as [date], os.[Name] as [status], 
                         SUM(oi.units * oi.unitprice) as total
                  FROM [ordering].[Orders] o
                  LEFT JOIN [ordering].[orderitems] oi ON o.Id = oi.orderid 
                  LEFT JOIN [ordering].[orderstatus] os on o.OrderStatusId = os.Id                     
                  LEFT JOIN [ordering].[buyers] ob on o.BuyerId = ob.Id
                  WHERE ob.IdentityGuid = @userId
                  GROUP BY o.[Id], o.[OrderDate], os.[Name] 
                  ORDER BY o.[Id]",
            new { userId }
        );
    }

    // 获取所有卡类型信息
    public async Task<IEnumerable<CardType>> GetCardTypesAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        // 执行 SQL 查询，从 ordering.cardtypes 表中读取所有卡类型
        return await connection.QueryAsync<CardType>("SELECT * FROM ordering.cardtypes");
    }

    // 内部辅助方法：将查询返回的动态结果映射为 Order 对象，并组装订单项列表及总金额
    private Order MapOrderItems(dynamic result)
    {
        // 创建 Order 对象，并初始化基本属性
        var order = new Order
        {
            ordernumber = result[0].ordernumber,
            date = result[0].date,
            status = result[0].status,
            description = result[0].description,
            street = result[0].street,
            city = result[0].city,
            zipcode = result[0].zipcode,
            country = result[0].country,
            orderitems = new List<Orderitem>(),
            total = 0
        };

        // 遍历所有返回的记录，为每个订单项创建 Orderitem 对象，并累加订单总金额
        foreach (dynamic item in result)
        {
            var orderitem = new Orderitem
            {
                productname = item.productname,
                units = item.units,
                unitprice = (double)item.unitprice,
                pictureurl = item.pictureurl
            };

            // 累加该订单项的金额到总金额
            order.total += item.units * item.unitprice;
            order.orderitems.Add(orderitem);
        }

        return order;
    }
}
