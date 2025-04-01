namespace Microsoft.eShopOnContainers.Services.Identity.API.Models
{
    // 应用程序用户类，继承自IdentityUser
    // 可以在此类中添加用户的配置文件数据属性
    public class ApplicationUser : IdentityUser
    {
        // 信用卡号码，必填属性
        [Required]
        public string CardNumber { get; set; }

        // 安全码，必填属性
        [Required]
        public string SecurityNumber { get; set; }

        // 信用卡有效期，必填属性
        // 正则表达式验证：格式必须为MM/YY，例如 "08/23"
        [Required]
        [RegularExpression(@"(0[1-9]|1[0-2])\/[0-9]{2}", ErrorMessage = "Expiration should match a valid MM/YY value")]
        public string Expiration { get; set; }

        // 持卡人姓名，必填属性
        [Required]
        public string CardHolderName { get; set; }

        // 信用卡类型，以整型表示，可选属性
        public int CardType { get; set; }

        // 街道地址，必填属性
        [Required]
        public string Street { get; set; }

        // 城市，必填属性
        [Required]
        public string City { get; set; }

        // 州，必填属性
        [Required]
        public string State { get; set; }

        // 国家，必填属性
        [Required]
        public string Country { get; set; }

        // 邮政编码，必填属性
        [Required]
        public string ZipCode { get; set; }

        // 用户名字，必填属性
        [Required]
        public string Name { get; set; }

        // 用户姓氏，必填属性
        [Required]
        public string LastName { get; set; }
    }
}
