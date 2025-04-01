namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate
{
    /// <summary>
    /// OrderItem 表示订单中的某个商品条目，继承自基础 Entity 类
    /// </summary>
    public class OrderItem : Entity
    {
        // DDD 设计的建议：使用私有字段更好封装数据
        // 以下字段均为订单项内部状态，不直接公开
        private string _productName;    // 商品名称
        private string _pictureUrl;     // 商品图片 URL
        private decimal _unitPrice;     // 单价
        private decimal _discount;      // 折扣
        private int _units;             // 数量

        // 商品标识，公开只读
        public int ProductId { get; private set; }

        // EF Core 需要一个空的构造函数为只读字段初始化
        protected OrderItem() { }

        /// <summary>
        /// 构造函数，初始化一个新的订单项对象
        /// </summary>
        /// <param name="productId">商品标识</param>
        /// <param name="productName">商品名称</param>
        /// <param name="unitPrice">商品单价</param>
        /// <param name="discount">折扣</param>
        /// <param name="PictureUrl">商品图片 URL</param>
        /// <param name="units">购买数量，默认为 1</param>
        public OrderItem(int productId, string productName, decimal unitPrice, decimal discount, string PictureUrl, int units = 1)
        {
            // 单位不能为空，必须大于 0
            if (units <= 0)
            {
                throw new OrderingDomainException("Invalid number of units");
            }

            // 校验：总价格必须大于或等于折扣
            if ((unitPrice * units) < discount)
            {
                throw new OrderingDomainException("The total of order item is lower than applied discount");
            }

            // 初始化只读属性和私有字段
            ProductId = productId;
            _productName = productName;
            _unitPrice = unitPrice;
            _discount = discount;
            _units = units;
            _pictureUrl = PictureUrl;
        }

        /// <summary>
        /// 返回图片的 URI
        /// </summary>
        public string GetPictureUri() => _pictureUrl;

        /// <summary>
        /// 获取当前的折扣值
        /// </summary>
        public decimal GetCurrentDiscount() => _discount;

        /// <summary>
        /// 获取当前的订单项数量
        /// </summary>
        public int GetUnits() => _units;

        /// <summary>
        /// 获取商品的单价
        /// </summary>
        public decimal GetUnitPrice() => _unitPrice;

        /// <summary>
        /// 获取订单项的商品名称
        /// </summary>
        public string GetOrderItemProductName() => _productName;

        /// <summary>
        /// 设置新的折扣值，必须满足折扣不为负
        /// </summary>
        /// <param name="discount">新的折扣金额</param>
        public void SetNewDiscount(decimal discount)
        {
            if (discount < 0)
            {
                throw new OrderingDomainException("Discount is not valid");
            }
            _discount = discount;
        }

        /// <summary>
        /// 增加订单项数量，数量必须大于或等于 0
        /// </summary>
        /// <param name="units">需要增加的数量</param>
        public void AddUnits(int units)
        {
            if (units < 0)
            {
                throw new OrderingDomainException("Invalid units");
            }
            _units += units;
        }
    }
}
