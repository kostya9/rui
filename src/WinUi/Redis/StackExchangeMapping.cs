using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
