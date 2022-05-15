using System.Collections.ObjectModel;
using System.Text.Json;

namespace WinUi;

public record LoadedServers(ObservableCollection<RedisServerListEntry> RedisServers, ObservableCollection<ConnectedRedisServer> ConnectedServers)
{
    public LoadedServers() : this(new ObservableCollection<RedisServerListEntry>(), new ObservableCollection<ConnectedRedisServer>())
    {

    }

    public string Serialize()
    {
        var serializedState = new SerializedState
        {
            Servers = RedisServers.Select(x => x.Server).ToArray()
        };
        return JsonSerializer.Serialize(serializedState);
    }

    public static LoadedServers Deserialize(string data)
    {
        var serializedState = JsonSerializer.Deserialize<SerializedState>(data) ?? new SerializedState();
        var redisServers = new ObservableCollection<RedisServerListEntry>(
            serializedState.Servers.Select(x => new RedisServerListEntry(x)).ToArray());
        var connectedServers = new ObservableCollection<ConnectedRedisServer>();

        return new LoadedServers(redisServers, connectedServers);
    }

    private class SerializedState
    {
        public RedisServer[] Servers { get; set; } = Array.Empty<RedisServer>();
    }
}