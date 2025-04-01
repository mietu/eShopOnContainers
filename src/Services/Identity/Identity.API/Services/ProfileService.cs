namespace Microsoft.eShopOnContainers.Services.Identity.API.Services
{
    public class ProfileService : IProfileService
    {
        // 用户管理器，用于管理 ApplicationUser 实例
        private readonly UserManager<ApplicationUser> _userManager;

        // 构造函数，通过依赖注入获取 UserManager 实例
        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // 获取用户的 Profile 数据
        async public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // 确保 context.Subject 不为空，否则抛出异常
            var subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));

            // 根据“sub”声明获取用户标识
            var subjectId = subject.Claims.Where(x => x.Type == "sub").FirstOrDefault()?.Value;

            // 根据用户标识查找用户
            var user = await _userManager.FindByIdAsync(subjectId);
            if (user == null)
                throw new ArgumentException("Invalid subject identifier");

            // 从用户对象提取声明
            var claims = GetClaimsFromUser(user);
            // 将提取的声明赋值给 IssuedClaims
            context.IssuedClaims = claims.ToList();
        }

        // 判断用户是否处于活动状态
        async public Task IsActiveAsync(IsActiveContext context)
        {
            // 确保 context.Subject 不为空，否则抛出异常
            var subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));

            // 根据“sub”声明获取用户标识
            var subjectId = subject.Claims.Where(x => x.Type == "sub").FirstOrDefault()?.Value;
            // 查找用户
            var user = await _userManager.FindByIdAsync(subjectId);

            // 初始化为不活跃状态
            context.IsActive = false;

            if (user != null)
            {
                // 如果支持安全戳，则验证令牌中的安全戳与数据库中的是否一致
                if (_userManager.SupportsUserSecurityStamp)
                {
                    var security_stamp = subject.Claims.Where(c => c.Type == "security_stamp").Select(c => c.Value).SingleOrDefault();
                    if (security_stamp != null)
                    {
                        var db_security_stamp = await _userManager.GetSecurityStampAsync(user);
                        if (db_security_stamp != security_stamp)
                            return;
                    }
                }

                // 判断用户是否被锁定，如果未被锁定或锁定时间已过，则用户是活跃的
                context.IsActive =
                    !user.LockoutEnabled ||
                    !user.LockoutEnd.HasValue ||
                    user.LockoutEnd <= DateTime.Now;
            }
        }

        // 从用户对象中提取声明集合
        private IEnumerable<Claim> GetClaimsFromUser(ApplicationUser user)
        {
            var claims = new List<Claim>
                {
                    // 添加用户标识、用户名等基本声明信息
                    new Claim(JwtClaimTypes.Subject, user.Id),
                    new Claim(JwtClaimTypes.PreferredUserName, user.UserName),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
                };

            // 如果用户的 Name 不为空，则添加 name 声明
            if (!string.IsNullOrWhiteSpace(user.Name))
                claims.Add(new Claim("name", user.Name));

            // 如果用户的 LastName 不为空，则添加 last_name 声明
            if (!string.IsNullOrWhiteSpace(user.LastName))
                claims.Add(new Claim("last_name", user.LastName));

            // 如果用户的信用卡号码不为空，则添加 card_number 声明
            if (!string.IsNullOrWhiteSpace(user.CardNumber))
                claims.Add(new Claim("card_number", user.CardNumber));

            // 如果用户的持卡人姓名不为空，则添加 card_holder 声明
            if (!string.IsNullOrWhiteSpace(user.CardHolderName))
                claims.Add(new Claim("card_holder", user.CardHolderName));

            // 如果用户的安全码不为空，则添加 card_security_number 声明
            if (!string.IsNullOrWhiteSpace(user.SecurityNumber))
                claims.Add(new Claim("card_security_number", user.SecurityNumber));

            // 如果用户的信用卡有效期不为空，则添加 card_expiration 声明
            if (!string.IsNullOrWhiteSpace(user.Expiration))
                claims.Add(new Claim("card_expiration", user.Expiration));

            // 如果用户所在的城市不为空，则添加 address_city 声明
            if (!string.IsNullOrWhiteSpace(user.City))
                claims.Add(new Claim("address_city", user.City));

            // 如果用户的国家不为空，则添加 address_country 声明
            if (!string.IsNullOrWhiteSpace(user.Country))
                claims.Add(new Claim("address_country", user.Country));

            // 如果用户所在的州不为空，则添加 address_state 声明
            if (!string.IsNullOrWhiteSpace(user.State))
                claims.Add(new Claim("address_state", user.State));

            // 如果用户的街道地址不为空，则添加 address_street 声明
            if (!string.IsNullOrWhiteSpace(user.Street))
                claims.Add(new Claim("address_street", user.Street));

            // 如果用户的邮政编码不为空，则添加 address_zip_code 声明
            if (!string.IsNullOrWhiteSpace(user.ZipCode))
                claims.Add(new Claim("address_zip_code", user.ZipCode));

            // 如果用户管理器支持邮箱，则添加 Email 相关声明
            if (_userManager.SupportsUserEmail)
            {
                claims.AddRange(new[]
                {
                        new Claim(JwtClaimTypes.Email, user.Email),
                        new Claim(JwtClaimTypes.EmailVerified, user.EmailConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
                    });
            }

            // 如果用户管理器支持电话号码且用户的电话号码不为空，则添加手机号码相关声明
            if (_userManager.SupportsUserPhoneNumber && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                claims.AddRange(new[]
                {
                        new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber),
                        new Claim(JwtClaimTypes.PhoneNumberVerified, user.PhoneNumberConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
                    });
            }

            return claims;
        }
    }
}
