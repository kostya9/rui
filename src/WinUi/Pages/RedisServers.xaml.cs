using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using StackExchange.Redis;
using System.Diagnostics;
using WinUi.Redis;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUi.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RedisServers : Page
{
    private LoadedServers? _connections;

    public RedisServers()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        this._connections = ((RedisServerNavigationParameters)e.Parameter).Servers;
        base.OnNavigatedTo(e);
    }

    private async void addNewServerBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_connections == null)
            return;

        var addRedisServerDialog = new AddRedisServerDialog();
        addRedisServerDialog.XamlRoot = Content.XamlRoot;

        var result = await addRedisServerDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            if (addRedisServerDialog.Result != null)
            {
                _connections.RedisServers.Add(addRedisServerDialog.Result);
            }
        }
    }

    private async void Servers_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (_connections == null)
            return;

        try
        {
            if (e.OriginalSource is FrameworkElement { DataContext: RedisServer server })
            {
                foreach (var connectedServer in _connections.ConnectedServers)
                {
                    // Already connected, skipping
                    if (connectedServer.Server.Id == server.Id)
                    {
                        return;
                    }
                }

                var connection = await ConnectionMultiplexer.ConnectAsync(
                    StackExchangeMapping.ToConnectionOptions(server));
                _connections.ConnectedServers.Add(new(server, connection));
            }
            
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not connect to redis: {ex}");
        }
    }

    public class RedisServerNavigationParameters
    {
        public LoadedServers Servers { get; set; } = new();
    }

    private void removeServer_Click(object sender, RoutedEventArgs e)
    {
        if (_connections == null)
            return;

        if(e.OriginalSource is FrameworkElement { DataContext: RedisServer server})
        {
            _connections.RedisServers.Remove(server);
        }
    }
}
