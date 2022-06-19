using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using WinUi.Infrastructure;
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
    private Navigation _navigation;

    public MainWindow(Navigation navigation)
    {
        this.InitializeComponent();

        _navigation = navigation;
        _navigation.InitializeNavigationBase(this.navigation);

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
        "GoIcon", typeof(IconElement), typeof(IconElement), new PropertyMetadata(default(IconElement)));

    private void ConnectedServers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is not ConnectedRedisServer connectedServer)
                    continue;

                var serverInfo = connectedServer.ServerEntry;

                var disconnectMenuItem = new MenuFlyoutItem()
                {
                    Text = "Disconnect",
                    Tag = connectedServer
                };

                disconnectMenuItem.Click += DisconnectMenuItem_Click;

                var newMenuItem = new NavigationViewItem()
                {
                    Content = serverInfo.Server.Name,
                    Tag = connectedServer,
                    Icon = new SymbolIcon(Symbol.Folder),
                    ContextFlyout = new MenuFlyout()
                    {
                        Items =
                        {
                            disconnectMenuItem
                        }
                    },
                };
                newMenuItem.PointerPressed += RedisServer_PointerPressed;
                navigation.MenuItems.Add(newMenuItem);
            }
        }

        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is not ConnectedRedisServer connectedServer)
                    continue;

                var removeIdx = -1;
                for (int i = 0; i < navigation.MenuItems.Count; i++)
                {
                    object? menu = navigation.MenuItems[i];

                    if(menu is NavigationViewItem navViewItem && navViewItem.Tag is ConnectedRedisServer menuServer && menuServer == connectedServer)
                    {
                        removeIdx = i;
                        break;
                    }
                }

                if (removeIdx >= 0)
                {
                    navigation.MenuItems.RemoveAt(removeIdx);
                }
            }
        }
    }

    private async void RedisServer_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is NavigationViewItem { Tag: ConnectedRedisServer connectedServer } menuItem)
        {
            var pointer = e.GetCurrentPoint(menuItem);
            if (pointer.Properties.IsMiddleButtonPressed)
            {
                await Disconnect(connectedServer);
            }
        }
    }

    private async void DisconnectMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: ConnectedRedisServer connectedServer })
        {
            await Disconnect(connectedServer);
        }
    }

    private async Task Disconnect(ConnectedRedisServer connectedServer)
    {
        if (connectedServer.ServerEntry.IsBusy)
            return;

        if (this._navigation.IsCurrentlyAtServer(connectedServer))
        {
            this._navigation.NavigateToServersList();
        }

        this._servers.ConnectedServers.Remove(connectedServer);

        connectedServer.ServerEntry.EntryState = RedisServerListEntry.State.Disconnecting;

        try
        {
            await connectedServer.Connection.CloseAsync();
            connectedServer.Connection.Dispose();
        }
        finally
        {
            connectedServer.ServerEntry.EntryState = RedisServerListEntry.State.Disconnected;
        }
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        SaveServers(_servers);
    }

    private static void SaveServers(LoadedServers connections)
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrEmpty(folder))
        {
            folder = Path.Combine(folder, "rui");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var connectionsCachePath = Path.Combine(folder, "connections.json");
            var serializedState = connections.Serialize();
            File.WriteAllText(connectionsCachePath, serializedState);
        }
    }

    private static LoadedServers LoadServers()
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrEmpty(folder))
        {
            folder = Path.Combine(folder, "rui");
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
                    Console.WriteLine($"Could not deserialize state: {ex}");
                }
            }
        }

        return new LoadedServers();
    }

    private void NavigationSelected(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var transitionInfo = new SuppressNavigationTransitionInfo();

        if (args.SelectedItem is NavigationViewItem { Tag: ConnectedRedisServer server })
        {
            contentFrame.Navigate(typeof(ConnectedRedisServerPage), new ConnectedRedisServerPage.ConnectedRedisServerPageNavigationArgs(server), transitionInfo);
            return;
        }

        contentFrame.Navigate(typeof(RedisServers), new RedisServerNavigationParameters(_servers, _navigation), transitionInfo);
    }
}