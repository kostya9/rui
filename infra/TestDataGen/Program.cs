// See https://aka.ms/new-console-template for more information


using StackExchange.Redis;

var connection = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions()
{
    EndPoints =
    {
        { "localhost", 6379 }
    }
});

foreach (var i in Enumerable.Range(0, 200))
{
    await connection.GetDatabase(0).StringSetAsync(new RedisKey($"somekey_{i}"), new RedisValue(i.ToString()));
}
