namespace WebMVC.Infrastructure;

/// <summary>
/// 静态工具类，用于构建各种API端点的URL
/// </summary>
public static class API
{
    /// <summary>
    /// 购买相关API端点
    /// </summary>
    public static class Purchase
    {
        /// <summary>
        /// 获取添加商品到购物篮的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <returns>完整的API端点URL</returns>
        public static string AddItemToBasket(string baseUri) => $"{baseUri}/basket/items";

        /// <summary>
        /// 获取更新购物篮中商品的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <returns>完整的API端点URL</returns>
        public static string UpdateBasketItem(string baseUri) => $"{baseUri}/basket/items";

        /// <summary>
        /// 获取订单草稿的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <param name="basketId">购物篮ID</param>
        /// <returns>完整的API端点URL</returns>
        public static string GetOrderDraft(string baseUri, string basketId) => $"{baseUri}/order/draft/{basketId}";
    }

    /// <summary>
    /// 购物篮相关API端点
    /// </summary>
    public static class Basket
    {
        /// <summary>
        /// 获取特定购物篮的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <param name="basketId">购物篮ID</param>
        /// <returns>完整的API端点URL</returns>
        public static string GetBasket(string baseUri, string basketId) => $"{baseUri}/{basketId}";

        /// <summary>
        /// 获取更新购物篮的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <returns>完整的API端点URL</returns>
        public static string UpdateBasket(string baseUri) => baseUri;

        /// <summary>
        /// 获取结算购物篮的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <returns>完整的API端点URL</returns>
        public static string CheckoutBasket(string baseUri) => $"{baseUri}/checkout";

        /// <summary>
        /// 获取清空购物篮的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <param name="basketId">购物篮ID</param>
        /// <returns>完整的API端点URL</returns>
        public static string CleanBasket(string baseUri, string basketId) => $"{baseUri}/{basketId}";
    }

    /// <summary>
    /// 订单相关API端点
    /// </summary>
    public static class Order
    {
        /// <summary>
        /// 获取特定订单的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <param name="orderId">订单ID</param>
        /// <returns>完整的API端点URL</returns>
        public static string GetOrder(string baseUri, string orderId)
        {
            return $"{baseUri}/{orderId}";
        }

        /// <summary>
        /// 获取所有我的订单的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <returns>完整的API端点URL</returns>
        public static string GetAllMyOrders(string baseUri)
        {
            return baseUri;
        }

        /// <summary>
        /// 获取添加新订单的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <returns>完整的API端点URL</returns>
        public static string AddNewOrder(string baseUri)
        {
            return $"{baseUri}/new";
        }

        /// <summary>
        /// 获取取消订单的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <returns>完整的API端点URL</returns>
        public static string CancelOrder(string baseUri)
        {
            return $"{baseUri}/cancel";
        }

        /// <summary>
        /// 获取发货订单的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <returns>完整的API端点URL</returns>
        public static string ShipOrder(string baseUri)
        {
            return $"{baseUri}/ship";
        }
    }

    /// <summary>
    /// 商品目录相关API端点
    /// </summary>
    public static class Catalog
    {
        /// <summary>
        /// 获取所有商品目录项的API端点，支持分页和过滤
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <param name="page">页码</param>
        /// <param name="take">每页数量</param>
        /// <param name="brand">品牌ID过滤</param>
        /// <param name="type">类型ID过滤</param>
        /// <returns>完整的API端点URL，包含过滤和分页参数</returns>
        public static string GetAllCatalogItems(string baseUri, int page, int take, int? brand, int? type)
        {
            var filterQs = "";

            if (type.HasValue)
            {
                var brandQs = (brand.HasValue) ? brand.Value.ToString() : string.Empty;
                filterQs = $"/type/{type.Value}/brand/{brandQs}";
            }
            else if (brand.HasValue)
            {
                var brandQs = (brand.HasValue) ? brand.Value.ToString() : string.Empty;
                filterQs = $"/type/all/brand/{brandQs}";
            }
            else
            {
                filterQs = string.Empty;
            }

            return $"{baseUri}items{filterQs}?pageIndex={page}&pageSize={take}";
        }

        /// <summary>
        /// 获取所有品牌的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <returns>完整的API端点URL</returns>
        public static string GetAllBrands(string baseUri)
        {
            return $"{baseUri}catalogBrands";
        }

        /// <summary>
        /// 获取所有类型的API端点
        /// </summary>
        /// <param name="baseUri">基础URI</param>
        /// <returns>完整的API端点URL</returns>
        public static string GetAllTypes(string baseUri)
        {
            return $"{baseUri}catalogTypes";
        }
    }
}
