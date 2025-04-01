namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Config;

/// <summary>
/// UrlsConfig 类用于存储各个后端服务的 URL 配置和地址生成方法。
/// </summary>
public class UrlsConfig
{
    /// <summary>
    /// CatalogOperations 类封装了和目录服务相关的 URL 操作。
    /// </summary>
    public class CatalogOperations
    {
        /// <summary>
        /// 根据单个商品 id 获取商品详情（适用于通过 gRPC 调用 REST 接口，需通过 80 端口）。
        /// </summary>
        /// <param name="id">单个商品的 id</param>
        /// <returns>生成后的 URL 字符串</returns>
        public static string GetItemById(int id) =>
            $"/api/v1/catalog/items/{id}"; // 使用 80 端口默认配置

        /// <summary>
        /// 根据多个商品 id（以字符串形式传递）获取多个商品详情。
        /// 注意：此方法将字符串中的每个字符以逗号分隔连接。
        /// </summary>
        /// <param name="ids">商品 id 字符串</param>
        /// <returns>生成后的 URL 字符串</returns>
        public static string GetItemById(string ids) =>
            $"/api/v1/catalog/items/ids/{string.Join(',', ids)}";

        /// <summary>
        /// 根据整数集合中的商品 id 获取多个商品详情（适用于标准 REST 调用，通过 5000 端口）。
        /// </summary>
        /// <param name="ids">商品 id 的集合</param>
        /// <returns>生成后的 URL 字符串</returns>
        public static string GetItemsById(IEnumerable<int> ids) =>
            $":5000/api/v1/catalog/items?ids={string.Join(',', ids)}";
    }

    /// <summary>
    /// BasketOperations 类封装了和购物篮服务相关的 URL 操作。
    /// </summary>
    public class BasketOperations
    {
        /// <summary>
        /// 根据购物篮 id 获取购物篮信息。
        /// </summary>
        /// <param name="id">购物篮的 id</param>
        /// <returns>生成后的 URL 字符串</returns>
        public static string GetItemById(string id) =>
            $"/api/v1/basket/{id}";

        /// <summary>
        /// 更新购物篮的 URL。
        /// </summary>
        /// <returns>更新购物篮的 URL 字符串</returns>
        public static string UpdateBasket() =>
            "/api/v1/basket";
    }

    /// <summary>
    /// OrdersOperations 类封装了和订单服务相关的 URL 操作。
    /// </summary>
    public class OrdersOperations
    {
        /// <summary>
        /// 获取订单草稿的 URL。
        /// </summary>
        /// <returns>订单草稿 URL 字符串</returns>
        public static string GetOrderDraft() =>
            "/api/v1/orders/draft";
    }

    // 以下属性用于存储外部服务的基础 URL 地址
    /// <summary>
    /// Basket 服务基础地址
    /// </summary>
    public string Basket { get; set; }

    /// <summary>
    /// Catalog 服务基础地址
    /// </summary>
    public string Catalog { get; set; }

    /// <summary>
    /// Orders 服务基础地址
    /// </summary>
    public string Orders { get; set; }

    /// <summary>
    /// gRPC 方式调用 Basket 服务的地址
    /// </summary>
    public string GrpcBasket { get; set; }

    /// <summary>
    /// gRPC 方式调用 Catalog 服务的地址
    /// </summary>
    public string GrpcCatalog { get; set; }

    /// <summary>
    /// gRPC 方式调用 Ordering 服务的地址
    /// </summary>
    public string GrpcOrdering { get; set; }
}

