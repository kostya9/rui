using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StackExchange.Redis;
using System.Collections.ObjectModel;
using System.Text.Json;
using WinUi.Pages;
using static WinUi.Pages.RedisServers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUi;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private LoadedServers _servers = new();

    public MainWindow()
    {
        this.InitializeComponent();

        this.Closed += OnClosed;

        OnStarted();
    }

    private void OnStarted()
    {
        // Can't do this in XAML
        // https://github.com/microsoft/microsoft-ui-xaml/issues/3689
        this.Title = "rui";

        // Can't do this the way the other icons are set up
        // https://github.com/microsoft/microsoft-ui-xaml/issues/6773
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.SetIcon("Assets/logo/spider.ico");

        _servers = LoadServers();

        this.navigation.SelectedItem = this.navigation.MenuItems[0];

        this._servers.ConnectedServers.CollectionChanged += ConnectedServers_CollectionChanged;
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        "Icon", typeof(IconElement), typeof(IconElement), new PropertyMetadata(default(IconElement)));

    private void ConnectedServers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is not ConnectedRedisServer connectedServer)
                    continue;

                var serverInfo = connectedServer.Server;

                navigation.MenuItems.Add(new NavigationViewItem()
                {
                    Content = serverInfo.Name,
                    Tag = serverInfo.Id,
                    Icon = new SymbolIcon(Symbol.Folder)
                });
            }
        }

        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is not RedisServer server)
                    continue;

                var removeIdx = -1;
                for (int i = 0; i < navigation.MenuItems.Count; i++)
                {
                    object? menu = navigation.MenuItems[i];

                    if(menu is NavigationViewItem navViewItem && navViewItem.Tag is Guid guid && guid == server.Id)
                    {
                        removeIdx = i;
                        break;
                    }
                }

                if (removeIdx >= 0)
                {
                    e.OldItems.RemoveAt(removeIdx);
                }
            }
        }
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        SaveServers(_servers);
    }

    private static void SaveServers(LoadedServers connections)
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        folder = Path.Combine(folder, "rui");
        if (folder != null)
        {
            var connectionsCachePath = Path.Combine(folder, "connections.json");
            var serializedState = connections.Serialize();
            File.WriteAllText(connectionsCachePath, serializedState);
        }
    }

    private static LoadedServers LoadServers()
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        folder = Path.Combine(folder, "rui");
        if (folder != null)
        {
            var connectionsCachePath = Path.Combine(folder, "connections.json");
            if (File.Exists(connectionsCachePath))
            {
                try
                {
                    var connectionsData = File.ReadAllText(connectionsCachePath);
                    return LoadedServers.Deserialize(connectionsData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not deserialize stateL {ex}");
                }
            }
        }

        return new LoadedServers();
    }

    private void NavigationSelected(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        contentFrame.NavigateToType(typeof(RedisServers), new RedisServerNavigationParameters { Servers = _servers }, null);
    }
}

public record RedisServer(string Name, string Address, int Port, string Username, string Password)
{
    public Guid Id { get; set; } = Guid.NewGuid();
};

public record ConnectedRedisServer(RedisServer Server, ConnectionMultiplexer Connection);

public class LoadedServers
{
    public ObservableCollection<RedisServer> RedisServers { get; set; } = new();

    public ObservableCollection<ConnectedRedisServer> ConnectedServers { get; set; } = new();

    public string Serialize()
    {
        var serializedState = new SerializedState
        {
            Servers = RedisServers.ToArray()
        };
        return JsonSerializer.Serialize(serializedState);
    }

    public static LoadedServers Deserialize(string data)
    {
        var serializedState = JsonSerializer.Deserialize<SerializedState>(data) ?? new SerializedState();
        return new LoadedServers
        {
            RedisServers = new ObservableCollection<RedisServer>(serializedState.Servers)
        };
    }

    private class SerializedState
    {
        public RedisServer[] Servers { get; set; } = Array.Empty<RedisServer>();
    }
}
