namespace Microsoft.eShopOnContainers.Services.Identity.API.Services
{
    /// <summary>
    /// 使用实体框架操作用户登录的服务实现。
    /// </summary>
    public class EFLoginService : ILoginService<ApplicationUser>
    {
        // 用户管理器，用于处理用户的增删改查
        private UserManager<ApplicationUser> _userManager;

        // 登录管理器，用于处理用户的登录操作
        private SignInManager<ApplicationUser> _signInManager;

        /// <summary>
        /// 构造函数，注入UserManager和SignInManager。
        /// </summary>
        /// <param name="userManager">管理应用用户的对象</param>
        /// <param name="signInManager">管理用户登录的对象</param>
        public EFLoginService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// 根据用户的电子邮件地址查找应用用户。
        /// </summary>
        /// <param name="user">用户的电子邮件地址</param>
        /// <returns>返回匹配的ApplicationUser对象</returns>
        public async Task<ApplicationUser> FindByUsername(string user)
        {
            return await _userManager.FindByEmailAsync(user);
        }

        /// <summary>
        /// 验证指定用户的凭证（密码）。
        /// </summary>
        /// <param name="user">待验证的用户</param>
        /// <param name="password">用户的密码</param>
        /// <returns>验证成功返回true，否则返回false</returns>
        public async Task<bool> ValidateCredentials(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        /// <summary>
        /// 使用默认配置（持久登录）对用户进行签入操作。
        /// </summary>
        /// <param name="user">待签入的用户</param>
        /// <returns>异步操作</returns>
        public Task SignIn(ApplicationUser user)
        {
            return _signInManager.SignInAsync(user, isPersistent: true);
        }

        /// <summary>
        /// 使用给定的身份验证属性对用户进行签入操作。
        /// </summary>
        /// <param name="user">待签入的用户</param>
        /// <param name="properties">身份验证属性</param>
        /// <param name="authenticationMethod">可选的身份验证方法</param>
        /// <returns>异步操作</returns>
        public Task SignInAsync(ApplicationUser user, AuthenticationProperties properties, string authenticationMethod = null)
        {
            return _signInManager.SignInAsync(user, properties, authenticationMethod);
        }
    }
}
