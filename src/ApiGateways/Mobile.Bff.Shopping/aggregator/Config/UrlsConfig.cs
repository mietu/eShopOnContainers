namespace Microsoft.eShopOnContainers.Mobile.Shopping.HttpAggregator.Config;

/// <summary>
/// 用于存储各个服务的基础 URL 和生成 API 请求路径的操作类
/// </summary>
public class UrlsConfig
{
    /// <summary>
    /// 处理与目录（Catalog）相关的操作
    /// </summary>
    public class CatalogOperations
    {
        /// <summary>
        /// 生成获取指定 ID 商品的 API URL
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <returns>API URL 字符串</returns>
        public static string GetItemById(int id) => $"/api/v1/catalog/items/{id}";

        /// <summary>
        /// 生成获取多个指定 ID 商品的 API URL
        /// </summary>
        /// <param name="ids">商品 ID 集合</param>
        /// <returns>API URL 字符串</returns>
        public static string GetItemsById(IEnumerable<int> ids) => $"/api/v1/catalog/items?ids={string.Join(',', ids)}";
    }

    /// <summary>
    /// 处理与购物篮（Basket）相关的操作
    /// </summary>
    public class BasketOperations
    {
        /// <summary>
        /// 生成获取指定 ID 购物篮的 API URL
        /// </summary>
        /// <param name="id">购物篮 ID</param>
        /// <returns>API URL 字符串</returns>
        public static string GetItemById(string id) => $"/api/v1/basket/{id}";

        /// <summary>
        /// 生成更新购物篮的 API URL
        /// </summary>
        /// <returns>API URL 字符串</returns>
        public static string UpdateBasket() => "/api/v1/basket";
    }

    /// <summary>
    /// 处理与订单（Orders）相关的操作
    /// </summary>
    public class OrdersOperations
    {
        /// <summary>
        /// 生成获取订单草稿的 API URL
        /// </summary>
        /// <returns>API URL 字符串</returns>
        public static string GetOrderDraft() => "/api/v1/orders/draft";
    }

    // 以下成员为各服务的基础 URL 地址

    /// <summary>
    /// 购物篮服务的基础 URL
    /// </summary>
    public string Basket { get; set; }

    /// <summary>
    /// 目录服务的基础 URL
    /// </summary>
    public string Catalog { get; set; }

    /// <summary>
    /// 订单服务的基础 URL
    /// </summary>
    public string Orders { get; set; }

    /// <summary>
    /// gRPC 格式的购物篮服务的基础 URL
    /// </summary>
    public string GrpcBasket { get; set; }

    /// <summary>
    /// gRPC 格式的目录服务的基础 URL
    /// </summary>
    public string GrpcCatalog { get; set; }

    /// <summary>
    /// gRPC 格式的订单服务的基础 URL
    /// </summary>
    public string GrpcOrdering { get; set; }
}
