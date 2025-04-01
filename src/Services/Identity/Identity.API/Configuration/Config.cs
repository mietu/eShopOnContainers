namespace Microsoft.eShopOnContainers.Services.Identity.API.Configuration
{
    public class Config
    {
        // 获取系统中的所有Api资源
        // 这里定义了各种API服务的标识名称和描述。
        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
                {
                    // "orders" API资源，描述为 "Orders Service"
                    new ApiResource("orders", "Orders Service"),
                    // "basket" API资源，描述为 "Basket Service"
                    new ApiResource("basket", "Basket Service"),
                    // "mobileshoppingagg" API资源，描述为 "Mobile Shopping Aggregator"
                    new ApiResource("mobileshoppingagg", "Mobile Shopping Aggregator"),
                    // "webshoppingagg" API资源，描述为 "Web Shopping Aggregator"
                    new ApiResource("webshoppingagg", "Web Shopping Aggregator"),
                    // "orders.signalrhub" API资源，描述为 "Ordering Signalr Hub"
                    new ApiResource("orders.signalrhub", "Ordering Signalr Hub"),
                    // "webhooks" API资源，描述为 "Webhooks registration Service"
                    new ApiResource("webhooks", "Webhooks registration Service"),
                };
        }

        // 获取系统中对外暴露的Api范围
        // ApiScope用于限制客户端访问特定的API，类似IdentityServer 3.x中的API资源。
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
                {
                    // "orders" ApiScope，描述为 "Orders Service"
                    new ApiScope("orders", "Orders Service"),
                    // "basket" ApiScope，描述为 "Basket Service"
                    new ApiScope("basket", "Basket Service"),
                    // "mobileshoppingagg" ApiScope，描述为 "Mobile Shopping Aggregator"
                    new ApiScope("mobileshoppingagg", "Mobile Shopping Aggregator"),
                    // "webshoppingagg" ApiScope，描述为 "Web Shopping Aggregator"
                    new ApiScope("webshoppingagg", "Web Shopping Aggregator"),
                    // "orders.signalrhub" ApiScope，描述为 "Ordering Signalr Hub"
                    new ApiScope("orders.signalrhub", "Ordering Signalr Hub"),
                    // "webhooks" ApiScope，描述为 "Webhooks registration Service"
                    new ApiScope("webhooks", "Webhooks registration Service"),
                };
        }

        // 获取系统中的身份资源
        // 身份资源包含了用户标识、姓名、邮箱等信息，供身份验证过程中使用
        public static IEnumerable<IdentityResource> GetResources()
        {
            return new List<IdentityResource>
                {
                    // OpenId资源是必须的，用于标识用户
                    new IdentityResources.OpenId(),
                    // Profile资源用于获取用户的基本信息，如姓名、头像等
                    new IdentityResources.Profile()
                };
        }

        // 获取允许访问资源的客户端配置
        // 根据传入的IConfiguration来动态获取客户端重定向URL、注销URL等设置
        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            return new List<Client>
                {
                    // JavaScript客户端配置
                    new Client
                    {
                        ClientId = "js",
                        ClientName = "eShop SPA OpenId Client",
                        // 使用隐式授权类型，适用于公共客户端
                        AllowedGrantTypes = GrantTypes.Implicit,
                        AllowAccessTokensViaBrowser = true,
                        // 登录后重定向的URI
                        RedirectUris =           { $"{configuration["SpaClient"]}/" },
                        // 不需要用户确认授权
                        RequireConsent = false,
                        // 注销后重定向的URI
                        PostLogoutRedirectUris = { $"{configuration["SpaClient"]}/" },
                        // 允许跨域来源配置
                        AllowedCorsOrigins =     { $"{configuration["SpaClient"]}" },
                        // 允许访问的范围包括OpenId、Profile及定义的各个API
                        AllowedScopes =
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            "orders",
                            "basket",
                            "webshoppingagg",
                            "orders.signalrhub",
                            "webhooks"
                        },
                    },
                    // Xamarin客户端配置
                    new Client
                    {
                        ClientId = "xamarin",
                        ClientName = "eShop Xamarin OpenId Client",
                        // 使用混合授权类型，便于在后通道获取访问令牌
                        AllowedGrantTypes = GrantTypes.Hybrid,                    
                        // 后台获取访问令牌，配置密钥
                        ClientSecrets =
                        {
                            new Secret("secret".Sha256())
                        },
                        // 登录重定向URI
                        RedirectUris = { configuration["XamarinCallback"] },
                        RequireConsent = false,
                        // 强制使用PKCE防止代码拦截攻击
                        RequirePkce = true,
                        // 注销后重定向URI
                        PostLogoutRedirectUris = { $"{configuration["XamarinCallback"]}/Account/Redirecting" },
                        // 允许访问的范围包括OpenId、Profile、离线访问及相关API
                        AllowedScopes = new List<string>
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.OfflineAccess,
                            "orders",
                            "basket",
                            "mobileshoppingagg",
                            "webhooks"
                        },
                        // 允许请求刷新令牌以实现长期API访问
                        AllowOfflineAccess = true,
                        AllowAccessTokensViaBrowser = true
                    },
                    // MVC客户端配置
                    new Client
                    {
                        ClientId = "mvc",
                        ClientName = "MVC Client",
                        ClientSecrets = new List<Secret>
                        {
                            new Secret("secret".Sha256())
                        },
                        // 客户端的公共URI
                        ClientUri = $"{configuration["MvcClient"]}",
                        // 授权类型为授权码
                        AllowedGrantTypes = GrantTypes.Code,
                        AllowAccessTokensViaBrowser = false,
                        RequireConsent = false,
                        AllowOfflineAccess = true,
                        // 始终在IdToken中包含用户声明
                        AlwaysIncludeUserClaimsInIdToken = true,
                        RequirePkce = false,
                        // 登录重定向URI
                        RedirectUris = new List<string>
                        {
                            $"{configuration["MvcClient"]}/signin-oidc"
                        },
                        // 注销重定向URI
                        PostLogoutRedirectUris = new List<string>
                        {
                            $"{configuration["MvcClient"]}/signout-callback-oidc"
                        },
                        // 允许访问的范围包括身份及相关API
                        AllowedScopes = new List<string>
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.OfflineAccess,
                            "orders",
                            "basket",
                            "webshoppingagg",
                            "orders.signalrhub",
                            "webhooks"
                        },
                        // 设置访问令牌和身份令牌的存活时间为2小时
                        AccessTokenLifetime = 60 * 60 * 2, // 2 hours
                        IdentityTokenLifetime = 60 * 60 * 2 // 2 hours
                    },
                    // Webhooks客户端配置
                    new Client
                    {
                        ClientId = "webhooksclient",
                        ClientName = "Webhooks Client",
                        ClientSecrets = new List<Secret>
                        {
                            new Secret("secret".Sha256())
                        },
                        // 客户端的公共URI
                        ClientUri = $"{configuration["WebhooksWebClient"]}",
                        AllowedGrantTypes = GrantTypes.Code,
                        AllowAccessTokensViaBrowser = false,
                        RequireConsent = false,
                        AllowOfflineAccess = true,
                        AlwaysIncludeUserClaimsInIdToken = true,
                        // 登录重定向URI
                        RedirectUris = new List<string>
                        {
                            $"{configuration["WebhooksWebClient"]}/signin-oidc"
                        },
                        // 注销重定向URI
                        PostLogoutRedirectUris = new List<string>
                        {
                            $"{configuration["WebhooksWebClient"]}/signout-callback-oidc"
                        },
                        // 允许访问的范围仅包括Webhooks API
                        AllowedScopes = new List<string>
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.OfflineAccess,
                            "webhooks"
                        },
                        AccessTokenLifetime = 60 * 60 * 2, // 2 hours
                        IdentityTokenLifetime = 60 * 60 * 2 // 2 hours
                    },
                    // MVC客户端测试配置
                    new Client
                    {
                        ClientId = "mvctest",
                        ClientName = "MVC Client Test",
                        ClientSecrets = new List<Secret>
                        {
                            new Secret("secret".Sha256())
                        },
                        // 客户端公共URI
                        ClientUri = $"{configuration["Mvc"]}",
                        AllowedGrantTypes = GrantTypes.Code,
                        AllowAccessTokensViaBrowser = true,
                        RequireConsent = false,
                        AllowOfflineAccess = true,
                        // 登录重定向URI
                        RedirectUris = new List<string>
                        {
                            $"{configuration["MvcClient"]}/signin-oidc"
                        },
                        // 注销重定向URI
                        PostLogoutRedirectUris = new List<string>
                        {
                            $"{configuration["MvcClient"]}/signout-callback-oidc"
                        },
                        // 允许访问的范围包括身份及相关API
                        AllowedScopes = new List<string>
                        {
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.OfflineAccess,
                            "orders",
                            "basket",
                            "webshoppingagg",
                            "webhooks"
                        },
                    },
                    // Basket Swagger UI配置
                    new Client
                    {
                        ClientId = "basketswaggerui",
                        ClientName = "Basket Swagger UI",
                        AllowedGrantTypes = GrantTypes.Implicit,
                        AllowAccessTokensViaBrowser = true,
                        // Swagger登录重定向URI
                        RedirectUris = { $"{configuration["BasketApiClient"]}/swagger/oauth2-redirect.html" },
                        // Swagger注销重定向URI
                        PostLogoutRedirectUris = { $"{configuration["BasketApiClient"]}/swagger/" },
                        // 仅允许访问basket API
                        AllowedScopes =
                        {
                            "basket"
                        }
                    },
                    // Ordering Swagger UI配置
                    new Client
                    {
                        ClientId = "orderingswaggerui",
                        ClientName = "Ordering Swagger UI",
                        AllowedGrantTypes = GrantTypes.Implicit,
                        AllowAccessTokensViaBrowser = true,
                        RedirectUris = { $"{configuration["OrderingApiClient"]}/swagger/oauth2-redirect.html" },
                        PostLogoutRedirectUris = { $"{configuration["OrderingApiClient"]}/swagger/" },
                        // 仅允许访问orders API
                        AllowedScopes =
                        {
                            "orders"
                        }
                    },
                    // Mobile Shopping Aggregator Swagger UI配置
                    new Client
                    {
                        ClientId = "mobileshoppingaggswaggerui",
                        ClientName = "Mobile Shopping Aggregattor Swagger UI",
                        AllowedGrantTypes = GrantTypes.Implicit,
                        AllowAccessTokensViaBrowser = true,
                        RedirectUris = { $"{configuration["MobileShoppingAggClient"]}/swagger/oauth2-redirect.html" },
                        PostLogoutRedirectUris = { $"{configuration["MobileShoppingAggClient"]}/swagger/" },
                        // 仅允许访问mobileshoppingagg API
                        AllowedScopes =
                        {
                            "mobileshoppingagg"
                        }
                    },
                    // Web Shopping Aggregator Swagger UI配置
                    new Client
                    {
                        ClientId = "webshoppingaggswaggerui",
                        ClientName = "Web Shopping Aggregattor Swagger UI",
                        AllowedGrantTypes = GrantTypes.Implicit,
                        AllowAccessTokensViaBrowser = true,
                        RedirectUris = { $"{configuration["WebShoppingAggClient"]}/swagger/oauth2-redirect.html" },
                        PostLogoutRedirectUris = { $"{configuration["WebShoppingAggClient"]}/swagger/" },
                        // 允许访问webshoppingagg及basket API
                        AllowedScopes =
                        {
                            "webshoppingagg",
                            "basket"
                        }
                    },
                    // WebHooks Swagger UI配置
                    new Client
                    {
                        ClientId = "webhooksswaggerui",
                        ClientName = "WebHooks Service Swagger UI",
                        AllowedGrantTypes = GrantTypes.Implicit,
                        AllowAccessTokensViaBrowser = true,
                        RedirectUris = { $"{configuration["WebhooksApiClient"]}/swagger/oauth2-redirect.html" },
                        PostLogoutRedirectUris = { $"{configuration["WebhooksApiClient"]}/swagger/" },
                        // 仅允许访问webhooks API
                        AllowedScopes =
                        {
                            "webhooks"
                        }
                    }
                };
        }
    }
}