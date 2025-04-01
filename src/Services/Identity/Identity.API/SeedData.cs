namespace Microsoft.eShopOnContainers.Services.Identity.API;

public class SeedData
{
    /// <summary>
    /// 用于确保种子数据（例如默认用户）存在于数据库中。
    /// 此方法会进行数据库迁移、校验用户是否存在、以及创建新用户。
    /// </summary>
    public static async Task EnsureSeedData(IServiceScope scope, IConfiguration configuration, Microsoft.Extensions.Logging.ILogger logger)
    {
        // 创建重试策略，用于在出现异常时重试操作。
        var retryPolicy = CreateRetryPolicy(configuration, logger);

        // 从服务提供者中获取数据库上下文实例
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // 执行重试策略，确保数据库迁移和数据种子操作具有一定的容错性
        await retryPolicy.ExecuteAsync(async () =>
        {
            // 执行数据库迁移（应用所有待处理的迁移）
            await context.Database.MigrateAsync();

            // 获取用户管理器，用于操作用户数据
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 检查用户 "alice" 是否存在
            var alice = await userMgr.FindByNameAsync("alice");
            if (alice == null)
            {
                // 如果 "alice" 不存在，则创建该用户实例
                alice = new ApplicationUser
                {
                    UserName = "alice",
                    Email = "AliceSmith@email.com",
                    EmailConfirmed = true,
                    CardHolderName = "Alice Smith",
                    CardNumber = "4012888888881881",
                    CardType = 1,
                    City = "Redmond",
                    Country = "U.S.",
                    Expiration = "12/24",
                    Id = Guid.NewGuid().ToString(),
                    LastName = "Smith",
                    Name = "Alice",
                    PhoneNumber = "1234567890",
                    ZipCode = "98052",
                    State = "WA",
                    Street = "15703 NE 61st Ct",
                    SecurityNumber = "123"
                };

                // 同步创建用户，并检查操作是否成功
                var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    // 如果创建失败，则抛出异常，异常详情来自错误信息列表中的第一个错误
                    throw new Exception(result.Errors.First().Description);
                }
                logger.LogDebug("alice created");
            }
            else
            {
                logger.LogDebug("alice already exists");
            }

            // 检查用户 "bob" 是否存在
            var bob = await userMgr.FindByNameAsync("bob");
            if (bob == null)
            {
                // 如果 "bob" 不存在，则创建该用户实例
                bob = new ApplicationUser
                {
                    UserName = "bob",
                    Email = "BobSmith@email.com",
                    EmailConfirmed = true,
                    CardHolderName = "Bob Smith",
                    CardNumber = "4012888888881881",
                    CardType = 1,
                    City = "Redmond",
                    Country = "U.S.",
                    Expiration = "12/24",
                    Id = Guid.NewGuid().ToString(),
                    LastName = "Smith",
                    Name = "Bob",
                    PhoneNumber = "1234567890",
                    ZipCode = "98052",
                    State = "WA",
                    Street = "15703 NE 61st Ct",
                    SecurityNumber = "456"
                };

                // 异步创建用户，并判断操作结果是否成功
                var result = await userMgr.CreateAsync(bob, "Pass123$");
                if (!result.Succeeded)
                {
                    // 如果创建失败，则抛出异常，异常信息来自错误列表中的第一个错误
                    throw new Exception(result.Errors.First().Description);
                }
                logger.LogDebug("bob created");
            }
            else
            {
                logger.LogDebug("bob already exists");
            }
        });
    }

    /// <summary>
    /// 根据配置和日志记录器创建重试策略。
    /// 若配置启用了重试，则重试数据库迁移和初始化操作，间隔为5秒。
    /// 否则返回一个空操作策略（无重试）。
    /// </summary>
    private static AsyncPolicy CreateRetryPolicy(IConfiguration configuration, Microsoft.Extensions.Logging.ILogger logger)
    {
        var retryMigrations = false;
        bool.TryParse(configuration["RetryMigrations"], out retryMigrations);

        // 如果配置了重试数据库迁移，则创建带有重试逻辑的策略
        if (retryMigrations)
        {
            return Policy.Handle<Exception>()
                .WaitAndRetryForeverAsync(
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, retry, timeSpan) =>
                    {
                        // 在每次重试时记录警告日志，包含异常类型、消息及重试次数
                        logger.LogWarning(
                            exception,
                            "Exception {ExceptionType} with message {Message} detected during database migration (retry attempt {retry})",
                            exception.GetType().Name,
                            exception.Message,
                            retry);
                    }
                );
        }

        // 如果未启用重试，则返回无操作策略
        return Policy.NoOpAsync();
    }
}
