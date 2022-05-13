using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using StackExchange.Redis;
using System.Diagnostics;
using WinUi.Infrastructure;
using WinUi.Redis;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUi.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RedisServers : Page
{
    private RedisServerNavigationParameters? _navProperties;

    public RedisServers()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        this._navProperties = (RedisServerNavigationParameters)e.Parameter;
        base.OnNavigatedTo(e);
    }

    private async void addNewServerBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_navProperties == null)
            return;

        var connections = _navProperties.Servers;

        var addRedisServerDialog = new AddRedisServerDialog();
        addRedisServerDialog.XamlRoot = Content.XamlRoot;

        var result = await addRedisServerDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            if (addRedisServerDialog.Result != null)
            {
                connections.RedisServers.Add(new RedisServerListEntry(addRedisServerDialog.Result));
            }
        }
    }

    private async void Servers_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: RedisServerListEntry entry })
        {
            await ConnectAsync(entry);
        }
    }

    private async void connect_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: RedisServerListEntry entry })
        {
            await ConnectAsync(entry);
        }
    }

    private async Task ConnectAsync(RedisServerListEntry entry)
    {
        if (entry.IsConnecting)
            return;

        entry.IsConnecting = true;
        var connectedServer = await ConnectAsync(entry.Server);
        entry.IsConnecting = false;
        if (connectedServer != null)
        {
            _navProperties?.Navigation.TryNavigateToServer(connectedServer);
        }
    }

    private async void removeServer_Click(object sender, RoutedEventArgs e)
    {
        if (_navProperties == null)
            return;

        var connections = _navProperties.Servers;

        if (e.OriginalSource is FrameworkElement { DataContext: RedisServerListEntry entry})
        {
            var dialog = new ContentDialog();
            dialog.Title = "Are you sure?";
            dialog.Content = $"Are you sure you want to delete server '{entry.Server.Name}'?";
            dialog.PrimaryButtonText = "Delete";
            dialog.SecondaryButtonText = "Cancel";

            dialog.XamlRoot = this.Content.XamlRoot;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                connections.RedisServers.Remove(entry);
            }
        }
    }

    private async Task<ConnectedRedisServer?> ConnectAsync(RedisServer server)
    {
        if (_navProperties == null)
            return null;

        var connections = _navProperties.Servers;

        try
        {
            foreach (var existingConnectedServer in connections.ConnectedServers)
            {
                // Already connected, skipping
                if (existingConnectedServer.Server.Id == server.Id)
                {
                    return existingConnectedServer;
                }
            }

            var connection = await ConnectionMultiplexer.ConnectAsync(
                StackExchangeMapping.ToConnectionOptions(server));
            var connectedServer = new ConnectedRedisServer(server, connection);
            connections.ConnectedServers.Add(connectedServer);

            return connectedServer;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not connect to redis: {ex}");
            return null;
        }
    }

    public record RedisServerNavigationParameters(LoadedServers Servers, Navigation Navigation);
}
