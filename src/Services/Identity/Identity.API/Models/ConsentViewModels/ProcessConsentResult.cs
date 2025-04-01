namespace Microsoft.eShopOnContainers.Services.Identity.API.Models.ConsentViewModels
{
    // ProcessConsentResult 类用于处理用户的同意请求后返回的结果
    public class ProcessConsentResult
    {
        // 如果 RedirectUri 不为空，则表示需要重定向到该 URI
        public bool IsRedirect => RedirectUri != null;
        // 重定向的 URI，当用户同意或拒绝授权时可能会用到
        public string RedirectUri { get; set; }
        // 客户端信息，表示当前请求的客户端
        public Client Client { get; set; }

        // 如果 ViewModel 不为空，则表示需要显示同意界面
        public bool ShowView => ViewModel != null;
        // 同意界面所需的视图模型，包含客户端详情以及请求的权限信息
        public ConsentViewModel ViewModel { get; set; }

        // 如果 ValidationError 不为空，则表示存在验证错误信息
        public bool HasValidationError => ValidationError != null;
        // 验证错误的详细描述，通常用于反馈错误消息给用户
        public string ValidationError { get; set; }
    }
}