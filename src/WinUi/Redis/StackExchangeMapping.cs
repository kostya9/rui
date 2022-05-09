using StackExchange.Redis;

namespace WinUi.Redis;
internal static class StackExchangeMapping
{
    public static ConfigurationOptions ToConnectionOptions(RedisServer server)
    {
        return new ConfigurationOptions
        {
            EndPoints =
                {
                    { server.Address, server.Port }
                },
            User = server.Username,
            Password = server.Password
        };
    }
}
